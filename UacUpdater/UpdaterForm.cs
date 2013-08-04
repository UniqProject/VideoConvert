using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using UacUpdater.Properties;
using UpdateCore;
using SevenZip;

namespace UacUpdater
{
    public partial class UpdaterForm : Form
    {
        private readonly string _updateFile;
        private readonly string _mainAppFile;
        private readonly List<PackageInfo> _updateList;

        public UpdaterForm()
        {
            InitializeComponent();
            string[] parameters = Environment.GetCommandLineArgs();
            if (parameters.Count() >= 3)
            {
                _updateFile = parameters.GetValue(1) as string;
                _mainAppFile = parameters.GetValue(2) as string;

                if (_updateFile == null || _mainAppFile == null)
                {
                    Close();
                    return;
                }

                _updateList = new List<PackageInfo>();

                try
                {
                    _updateList = Updater.LoadUpdateList(_updateFile);
                }
                catch (Exception ex)
                {
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
            {
                Close();
            }
        }

        private void UpdateTimerTick(object sender, EventArgs e)
        {
            UpdateTimer.Enabled = false;

            if (_updateList.Count <= 0) return;

            OverallProgress.Maximum = _updateList.Count;

            BackgroundWorker bw = new BackgroundWorker();
            bw.DoWork += BwDoWork;
            bw.WorkerSupportsCancellation = false;
            bw.WorkerReportsProgress = true;
            bw.ProgressChanged += BwProgressChanged;
            bw.RunWorkerCompleted += BwRunWorkerCompleted;
            bw.RunWorkerAsync();
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
                else
                {
                    if (pInfo.ClearDirectory || pInfo.RecursiveClearDirectory)
                        bwWorker.ReportProgress(-5, "Cleaning up target folder");
                    List<string> fileEntries = new List<string>();
                    List<string> dirEntries = new List<string>();
                    if (pInfo.ClearDirectory)
                    {
                        fileEntries = Directory.GetFiles(pInfo.Destination, "*", SearchOption.TopDirectoryOnly).ToList();
                    }
                    else if (pInfo.RecursiveClearDirectory)
                    {
                        fileEntries = Directory.GetFiles(pInfo.Destination, "*", SearchOption.AllDirectories).ToList();
                        dirEntries =
                            Directory.GetDirectories(pInfo.Destination, "*", SearchOption.AllDirectories).ToList();
                    }
                    foreach (string fileEntry in fileEntries)
                    {
                        File.Delete(fileEntry);
                    }
                    foreach (string dirEntry in dirEntries)
                    {
                        Directory.Delete(dirEntry);
                    }
                    
                }
                bwWorker.ReportProgress(-5, "Updating Package " + pInfo.PackageName + " Version " + pInfo.Version);

                try
                {
                    using (SevenZipExtractor zFile = new SevenZipExtractor(pInfo.PackageLocation))
                    {
                        bwWorker.ReportProgress(-7, (int)zFile.FilesCount);

                        zFile.FileExtractionStarted += (o, args) =>
                        {
                            bwWorker.ReportProgress(-8, args.FileInfo.Index);
                            bwWorker.ReportProgress(-9, args.FileInfo.FileName);
                        };
                        zFile.ExtractArchive(pInfo.Destination);

                        if (pInfo.WriteVersion)
                        {
                            using (StreamWriter str = new StreamWriter(Path.Combine(pInfo.Destination, "version"), false))
                            {
                                str.Write(pInfo.Version);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    
                    
                }
                

                File.Delete(pInfo.PackageLocation);
                bwWorker.ReportProgress(-6);
            }
        }
    }
}
