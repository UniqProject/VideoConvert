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

using System.IO;
using System.Threading;
using System.Windows;
using VideoConvert.Core;
using log4net;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für ChangelogViewer.xaml
    /// </summary>
    public partial class ChangelogViewer
    {
        private static readonly ILog Log = LogManager.GetLogger(typeof(ChangelogViewer));
        public ChangelogViewer()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            string changeLogFile = Path.Combine(AppSettings.AppPath, "CHANGELOG");
            string lang = Thread.CurrentThread.CurrentUICulture.IetfLanguageTag;
            string localChangeLog = Path.ChangeExtension(changeLogFile, lang);
            string engChangeLog = Path.ChangeExtension(changeLogFile, "en-US");

            if (File.Exists(localChangeLog))
                changeLogFile = localChangeLog;
            else if (File.Exists(engChangeLog))
                changeLogFile = engChangeLog;

            try
            {
                using (StreamReader read = new StreamReader(changeLogFile))
                {
                    ChangeLogText.Text = read.ReadToEnd();
                }
            }
            catch (System.Exception exception)
            {
                Log.Error(exception);
            }
            
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
