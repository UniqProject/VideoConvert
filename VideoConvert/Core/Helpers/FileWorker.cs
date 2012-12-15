//============================================================================
// VideoConvert - Fast Video & Audio Conversion Tool
// Copyright © 2012 JT-Soft
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA  02111-1307  USA
//=============================================================================

using System;
using System.ComponentModel;
using System.IO;
using log4net;

namespace VideoConvert.Core.Helpers
{
    class FileWorker
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(FileWorker));

        private BackgroundWorker _bw;

        private string _inFile = string.Empty;
        private string _outFile = string.Empty;

        public void SetFiles(string input, string ouput)
        {
            _inFile = input;
            _outFile = ouput;
        }

        public void MoveFile(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker) sender;

            string status = Processing.GetResourceString("fileworker_move_file");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0, status);

            DoCopyFile();

            if (File.GetAttributes(_inFile) == FileAttributes.Directory)
            {
                Directory.Delete(_inFile, true);
            }
            else
            {
                File.Delete(_inFile);
            }
        }

        public void CopyFile(object sender, DoWorkEventArgs e)
        {
            _bw = (BackgroundWorker) sender;

            string status = Processing.GetResourceString("fileworker_copy_file");
            _bw.ReportProgress(-10, status);
            _bw.ReportProgress(0);

            DoCopyFile();

        }

        private void DoCopyFile()
        {
            if (File.Exists(_outFile))
            {
                Log.DebugFormat("Output {0} exists, delete", _outFile);
                if (File.GetAttributes(_outFile) == FileAttributes.Directory)
                {
                    Directory.Delete(_outFile);
                }
                else
                {
                    File.Delete(_outFile);
                }
            }

            if ((!File.Exists(_inFile)) && (!Directory.Exists(_inFile))) return;

            if (File.GetAttributes(_inFile) == FileAttributes.Directory)
            {
                string[] files = Directory.GetFiles(_inFile, "*", SearchOption.AllDirectories);
                string[] directories = Directory.GetDirectories(_inFile, "*", SearchOption.AllDirectories);

                foreach (string dirName in directories)
                {
                    string tempInDir = dirName.Remove(0, _inFile.Length + 1);
                    string tempOutDir = Path.Combine(_outFile, tempInDir);
                    ExecuteCopy(dirName, tempOutDir, true);
                }

                foreach (string fileName in files)
                {
                    string tempInFile = fileName.Remove(0, _inFile.Length + 1);
                    string tempOutFile = Path.Combine(_outFile, tempInFile);
                    ExecuteCopy(fileName, tempOutFile, false);
                }
            }
            else
            {
                ExecuteCopy(_inFile, _outFile, false);
            }
        }

        private void ExecuteCopy(string fromFile, string toFile, bool isDirectory)
        {
            string progressFormat = Processing.GetResourceString("fileworker_copy_progress");

            string outDir = isDirectory ? toFile : Path.GetDirectoryName(toFile);
            if (outDir != null && ((!Directory.Exists(outDir)) && (isDirectory)))
            {
                Directory.CreateDirectory(outDir, DirSecurity.CreateDirSecurity(SecurityClass.Everybody));
            }

            if (File.GetAttributes(fromFile) == FileAttributes.Directory) return;
            using (FileStream fromStream = new FileStream(fromFile, FileMode.Open),
                              toStream = new FileStream(toFile, FileMode.CreateNew))
            {
                long total = fromStream.Length;
                long current = 0;
                Byte[] buffer = new Byte[51200]; // 50kbyte buffer
                int secRemaining = 0;

                DateTime startTime = DateTime.Now;
                do
                {
                    int read = fromStream.Read(buffer, 0, buffer.Length);
                    toStream.Write(buffer, 0, read);

                    current += read;

                    long prozent = current * 100 / total;
                    TimeSpan eta = startTime.Subtract(DateTime.Now);
                    int kbRemaining = ((int)current - (int)total) / 1024;
                    if (eta.Seconds != 0)
                    {
                        double kbs = Math.Round(current / 1024d / (eta.Seconds), 2);
                        secRemaining = kbRemaining / (int)kbs;
                    }

                    string progressText = string.Format(progressFormat,
                                                        Path.GetFileName(fromFile), (int) (current/1024),
                                                        (int) (total/1024), secRemaining);

                    _bw.ReportProgress((int)prozent, progressText);

                } while (total != current);
            }
        }
    }
}
