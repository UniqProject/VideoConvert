// --------------------------------------------------------------------------------------------------------------------
// <copyright file="FileWorker.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The FileWorker
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Muxer
{
    using Interfaces;
    using Interop.EventArgs;
    using Interop.Model;
    using log4net;
    using Services.Base;
    using Services.Interfaces;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Threading;
    using ThreadState = System.Threading.ThreadState;

    /// <summary>
    /// The FileWorker
    /// </summary>
    public class FileWorker : EncodeBase, IFileWorker
    {
        /// <summary>
        /// Errorlog
        /// </summary>
        private static readonly ILog Log = LogManager.GetLogger(typeof (FileWorker));

        #region Private Variables

        /// <summary>
        /// Start time of the current Encode;
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The Current Task
        /// </summary>
        private EncodeInfo _currentTask;

        private string _inputFile;
        private string _outputFile;

        private long _fileSizeToCopy;

        private Thread _copyThread;
        private long _totalCopied;

        #endregion

        /// <summary>
        /// Initializes a new instance of the <see cref="FileWorker"/> class.
        /// </summary>
        /// <param name="appConfig">
        /// The user Setting Service.
        /// </param>
        public FileWorker(IAppConfigService appConfig) : base(appConfig)
        {
        }

        #region Public Methods

        /// <summary>
        /// 
        /// </summary>
        /// <param name="encodeQueueTask"></param>
        /// <exception cref="Exception"></exception>
        public override void Start(EncodeInfo encodeQueueTask)
        {
            try
            {
                if (this.IsEncoding)
                    throw new Exception("FileCopy is already running");

                this.IsEncoding = true;
                this._currentTask = encodeQueueTask;

                this._copyThread = new Thread(CopyWorker);
                this._copyThread.Start();

                this._startTime = DateTime.Now;

                // Fire the Encode Started Event
                this.InvokeEncodeStarted(EventArgs.Empty);
            }
            catch (Exception exc)
            {
                Log.Error(exc);
                this._currentTask.ExitCode = -1;
                this.IsEncoding = false;
                this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(false, exc, exc.Message));
            }
        }

        private void CopyWorker()
        {
            if (this._currentTask.NextStep == EncodingStep.CopyTempFile)
            {
                this._inputFile = this._currentTask.InputFile;
                this._outputFile = this._currentTask.TempInput;
            }
            else
            {
                this._inputFile = this._currentTask.TempOutput;
                this._outputFile = this._currentTask.OutputFile;
            }

            var fileList = new List<FileInfo>();
            var dirList = new List<DirectoryInfo>();

            var isDir = File.GetAttributes(_inputFile) == FileAttributes.Directory;

            if (isDir)
            {
                var tempDirList = Directory.EnumerateDirectories(_inputFile).ToList();
                var tempFileList = Directory.EnumerateFiles(_inputFile).ToList();

                dirList.AddRange(tempDirList.Select(dir => new DirectoryInfo(dir)));
                fileList.AddRange(tempFileList.Select(file => new FileInfo(file)));
            }
            else
                fileList.Add(new FileInfo(_inputFile));

            
            _fileSizeToCopy = fileList.Sum(fileInfo => fileInfo.Length);

            if (isDir)
            {
                foreach (var info in dirList)
                {
                    var targetDir = info.FullName.Replace(_inputFile, _outputFile);
                    Directory.CreateDirectory(targetDir);
                }
            }

            foreach (var info in fileList)
            {
                var targetFile = info.FullName.Replace(_inputFile, _outputFile);
                ExecuteCopy(info.FullName, targetFile);
            }


            // handle temp files
            if (isDir && this._currentTask.NextStep == EncodingStep.MoveOutFile)
            {
                foreach (var info in dirList)
                {
                    this._currentTask.TempFiles.Add(info.FullName);
                }
            }

            // finish worker
            this._currentTask.ExitCode = 0;
            this._currentTask.CompletedStep = this._currentTask.NextStep;
            this.IsEncoding = false;
            this.InvokeEncodeCompleted(new EncodeCompletedEventArgs(true, null, string.Empty));
        }

        private void ExecuteCopy(string inFile, string outFile)
        {
            using (FileStream fromStream = new FileStream(inFile, FileMode.Open),
                              toStream = new FileStream(outFile, FileMode.CreateNew))
            {
                var reportTime = DateTime.Now;
                var totalFile = fromStream.Length;
                long current = 0;
                var buffer = new Byte[1048576]; // 1 mbyte buffer
                var secRemaining = 0;

                do
                {
                    var read = fromStream.Read(buffer, 0, buffer.Length);
                    toStream.Write(buffer, 0, read);
                    current += read;
                    this._totalCopied += read;

                    var progress = (float) this._totalCopied / this._fileSizeToCopy * 100f;
                    var elapsedTime = DateTime.Now - this._startTime;
                    var remainingSize = this._fileSizeToCopy - this._totalCopied;

                    var speed = 0d;
                    if (elapsedTime.TotalSeconds > 0)
                    {
                        speed = this._totalCopied/elapsedTime.TotalSeconds;
                    }

                    if (speed > 0)
                    {
                        secRemaining = (int)Math.Floor(remainingSize/speed);
                    }

                    var remainingTime = TimeSpan.FromSeconds(secRemaining);

                    if (reportTime.AddSeconds(1) <= DateTime.Now)
                    {
                        var eventArgs = new EncodeProgressEventArgs
                        {
                            AverageFrameRate = 0,
                            CurrentFrameRate = 0,
                            EstimatedTimeLeft = remainingTime,
                            PercentComplete = progress,
                            ElapsedTime = elapsedTime,
                        };
                        this.InvokeEncodeStatusChanged(eventArgs);
                        reportTime = DateTime.Now;
                    }
                } while (totalFile != current);
            }

            // handle temp files
            if (this._currentTask.NextStep == EncodingStep.MoveOutFile)
                this._currentTask.TempFiles.Add(inFile);
        }

        /// <summary>
        /// Kill the CLI process
        /// </summary>
        public override void Stop()
        {
            try
            {
                if (this._copyThread != null && this._copyThread.ThreadState == ThreadState.Running)
                    this._copyThread.Abort();
            }
            catch (Exception exc)
            {
                Log.Error(exc);
            }
            this.IsEncoding = false;
        }

        /// <summary>
        /// Shutdown the service.
        /// </summary>
        public void Shutdown()
        {
            // Nothing to do.
        }

        #endregion
    }
}