using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using ICSharpCode.SharpZipLib.Zip;
using UacUpdater.Properties;
using UpdateCore;

namespace UacUpdater
{
    public partial class UpdaterForm : Form
    {
        private readonly string _updateFile;
        private readonly string _mainAppFile;
        private readonly List<PackageInfo> _updateList;
        private bool _error;

        public UpdaterForm()
        {
            InitializeComponent();
            string[] parameters = Environment.GetCommandLineArgs();
            if (parameters.Count() >= 3)
            {
                _updateFile = parameters[1];
                _mainAppFile = parameters[2];

                _updateList = new List<PackageInfo>();

                try
                {
                    _updateList = Updater.LoadUpdateList(_updateFile);
                }
                catch (Exception ex)
                {
                    _error = true;
                    Log.AppendText("Malformed update file:");
                    Log.AppendText(Environment.NewLine);
                    Log.AppendText(File.ReadAllText(_updateFile));
                }
                finally
                {
                    UpdateTimer.Enabled = true;
                }
                
            }
            else
                Close();
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            UpdateTimer.Enabled = false;

            if (_updateList.Count > 0)
            {
                OverallProgress.Maximum = _updateList.Count;

                BackgroundWorker bw = new BackgroundWorker();
                bw.DoWork += BwDoWork;
                bw.WorkerSupportsCancellation = false;
                bw.WorkerReportsProgress = true;
                bw.ProgressChanged += BwProgressChanged;
                bw.RunWorkerCompleted += BwRunWorkerCompleted;
                bw.RunWorkerAsync();
            }
        }

        void BwRunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            string path = Path.GetDirectoryName(_mainAppFile);
            if (path != null)
            {
                Process main = new Process {StartInfo = new ProcessStartInfo{ FileName = _mainAppFile, WorkingDirectory = path}};
                main.Start();
            }
            Application.Exit();
        }

        void BwProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            switch (e.ProgressPercentage)
            {
                case -5:
                    Log.AppendText((string) e.UserState);
                    Log.AppendText(Environment.NewLine);
                    break;
                case -6:
                    OverallProgress.PerformStep();
                    break;
                case -7:
                    PackageProgress.Maximum = (int) e.UserState;
                    PackageProgress.Value = 0;
                    break;
                case -8:
                    PackageProgress.Value = (int) e.UserState;
                    break;
                case -9:
                    Status.Text = Resources.ExtractingStatus + (string) e.UserState;
                    break;
            }
        }

        void BwDoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bwWorker = (BackgroundWorker) sender;

            foreach (PackageInfo pInfo in _updateList)
            {
                if (!Directory.Exists(pInfo.Destination))
                    Directory.CreateDirectory(pInfo.Destination);
                bwWorker.ReportProgress(-5, "Updating Package " + pInfo.PackageName + " Version " + pInfo.Version);
                using (ZipFile zFile = new ZipFile(pInfo.PackageLocation))
                {
                    bwWorker.ReportProgress(-7, (int)zFile.Count);

                    foreach (ZipEntry entry in zFile)
                    {
                        bwWorker.ReportProgress(-8, (int)entry.ZipFileIndex);
                        bwWorker.ReportProgress(-9, entry.Name);

                        string outPath = Path.Combine(pInfo.Destination, entry.Name);
                        
                        if (entry.IsDirectory && !Directory.Exists(outPath))
                            Directory.CreateDirectory(outPath);
                        else if (entry.IsFile)
                        {
                            using (Stream zStream = zFile.GetInputStream(entry),
                                   outFile = new FileStream(outPath, FileMode.Create))
                            {
                                zStream.CopyTo(outFile);
                            }
                        }
                    }
                    if (pInfo.WriteVersion)
                    {
                        using (StreamWriter str = new StreamWriter(Path.Combine(pInfo.Destination, "version"), false))
                        {
                            str.Write(pInfo.Version);
                        }
                    }
                }
                File.Delete(pInfo.PackageLocation);
                bwWorker.ReportProgress(-6);
            }
        }
    }
}
