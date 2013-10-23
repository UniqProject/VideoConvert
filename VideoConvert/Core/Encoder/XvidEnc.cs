using System;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using log4net;

namespace VideoConvert.Core.Encoder
{
    class XvidEnc
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(XvidEnc));

        private EncodeInfo _jobInfo;
        private const string Executable = "xvid_encraw.exe";

        private string _verInfo = string.Empty;
        readonly Regex _verObj = new Regex(@"^xvidcore build version: xvid-([\d\.\-]*(?>-dev)??).?$",
                        RegexOptions.Singleline | RegexOptions.Multiline);

        private BackgroundWorker _bw;

        public void SetJob(EncodeInfo job)
        {
            _jobInfo = job;
        }
        public string GetVersionInfo()
        {
            return GetVersionInfo(AppSettings.ToolsPath);
        }

        public string GetVersionInfo(string encPath)
        {
            string localExecutable = Path.Combine(encPath, Executable);

            using (Process encoder = new Process())
            {
                ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                {
                    CreateNoWindow = true,
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };
                
                encoder.StartInfo = parameter;

                bool started;
                try
                {
                    started = encoder.Start();
                }
                catch (Exception ex)
                {
                    started = false;
                    Log.ErrorFormat("xvid_enc exception: {0}", ex);
                }

                if (started)
                {
                    encoder.BeginOutputReadLine();
                    encoder.BeginErrorReadLine();

                    encoder.OutputDataReceived += OnVersionDataReceived;
                    encoder.ErrorDataReceived += OnVersionDataReceived;

                    encoder.WaitForExit(10000);
                    if (!encoder.HasExited)
                        encoder.Kill();
                    encoder.CancelOutputRead();
                    encoder.CancelErrorRead();
                }
            }

            // Debug info
            if (Log.IsDebugEnabled)
                Log.DebugFormat("xvid_enc \"{0:s}\" found", _verInfo);

            return _verInfo;
        }

        private void OnVersionDataReceived(object sender, DataReceivedEventArgs args)
        {
            string line = args.Data;
            if (string.IsNullOrEmpty(line)) return;
            Match result = _verObj.Match(line);
            Process sProcess = (Process)sender;
            Log.Info(line);
            if (result.Success)
            {
                _verInfo = result.Groups[1].Value;
                if (sProcess != null && !sProcess.HasExited)
                    sProcess.Kill();
            }
        }

        public void Process(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker)sender;

            string status = Processing.GetResourceString("spumux_muxing_status");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            string localExecutable = Path.Combine(AppSettings.ToolsPath, Executable);

            SubtitleInfo sub = _jobInfo.SubtitleStreams[_jobInfo.StreamId];

            if (!sub.HardSubIntoVideo && File.Exists(sub.TempFile))
            {
                string inFile = _jobInfo.VideoStream.TempFile;
                string outFile = Processing.CreateTempFile(inFile, string.Format("+{0}.mpg", sub.LangCode));

                using (Process encoder = new Process())
                {
                    ProcessStartInfo parameter = new ProcessStartInfo(localExecutable)
                    {
                        WorkingDirectory = AppSettings.DemuxLocation,
                        CreateNoWindow = true,
                        UseShellExecute = false,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true,
                        RedirectStandardInput = true,
                        Arguments =
                            string.Format("-s {0:0} \"{1}\"", _jobInfo.StreamId,
                                sub.TempFile)
                    };



                    encoder.StartInfo = parameter;

                    Log.InfoFormat("spumux {0:s}", parameter.Arguments);

                    FileStream readStream = new FileStream(inFile, FileMode.Open, FileAccess.Read, FileShare.Read);
                    FileStream writeStream = new FileStream(outFile, FileMode.Create, FileAccess.ReadWrite,
                        FileShare.Read);

                    encoder.ErrorDataReceived += OnDataReceived;

                    bool started;
                    try
                    {
                        started = encoder.Start();
                    }
                    catch (Exception ex)
                    {
                        started = false;
                        Log.ErrorFormat("spumux exception: {0}", ex);
                        _jobInfo.ExitCode = -1;
                    }

                    DateTime update = DateTime.Now;

                    if (started)
                    {
                        encoder.PriorityClass = AppSettings.GetProcessPriority();
                        encoder.BeginErrorReadLine();
                        Processing.CopyStreamToStream(readStream, encoder.StandardInput.BaseStream, 0x2500,
                            (src, dst, exc) =>
                            {
                                src.Close();
                                dst.Close();

                                if (exc == null) return;

                                Log.Debug(exc.Message);
                                Log.Debug(exc.StackTrace);
                            });

                        byte[] bufferOut = new byte[0x10000];
                        int readOut;

                        while (!encoder.HasExited)
                        {
                            if (_bw.CancellationPending)
                                encoder.Kill();

                            if ((readOut = encoder.StandardOutput.BaseStream.Read(bufferOut, 0, bufferOut.Length)) > 0)
                                writeStream.Write(bufferOut, 0, readOut);

                            try
                            {
                                if (DateTime.Now.Subtract(update).Milliseconds > 500)
                                {
                                    int progress =
                                        (int) Math.Round(readStream.Position/(double) readStream.Length*100d, 0);

                                    _bw.ReportProgress(progress, status);
                                    update = DateTime.Now;
                                }
                            }
                            catch (Exception ex)
                            {
                                Log.Debug(ex.Message);
                                Log.Debug(ex.StackTrace);
                            }
                        }

                        if (!encoder.StandardOutput.EndOfStream &&
                            (readOut = encoder.StandardOutput.BaseStream.Read(bufferOut, 0, bufferOut.Length)) > 0)
                            writeStream.Write(bufferOut, 0, readOut);

                        encoder.WaitForExit(10000);
                        encoder.CancelErrorRead();

                        _jobInfo.ExitCode = encoder.ExitCode;
                        Log.InfoFormat("Exit Code: {0:g}", _jobInfo.ExitCode);
                        if (_jobInfo.ExitCode == 0)
                        {
                            _jobInfo.VideoStream.TempFile = outFile;
                            _jobInfo.TempFiles.Add(inFile);
                            GetTempImages(sub.TempFile);
                            _jobInfo.TempFiles.Add(sub.TempFile);
                            _jobInfo.TempFiles.Add(Path.GetDirectoryName(sub.TempFile));
                        }
                    }
                    readStream.Dispose();
                    writeStream.Dispose();
                }
            }
            _bw.ReportProgress(100);
            _jobInfo.CompletedStep = _jobInfo.NextStep;
            e.Result = _jobInfo;
        }

        private void OnDataReceived(object s, DataReceivedEventArgs ea)
        {
            Log.InfoFormat("spumux: {0:s}", ea.Data);
        }

        private void GetTempImages(string inFile)
        {
            XmlDocument inSubFile = new XmlDocument();
            inSubFile.Load(inFile);
            XmlNodeList spuList = inSubFile.SelectNodes("//spu");

            if (spuList != null)
                foreach (XmlNode spu in spuList.Cast<XmlNode>().Where(spu => spu.Attributes != null))
                {
                    Debug.Assert(spu.Attributes != null, "spu.Attributes != null");
                    _jobInfo.TempFiles.Add(spu.Attributes["image"].Value);
                }
        }
    }
}