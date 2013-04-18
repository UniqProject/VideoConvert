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
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading;
using System.Windows;
using VideoConvert.Core;
using log4net;
using log4net.Appender;
using log4net.Config;
using log4net.Core;
using log4net.Filter;
using log4net.Layout;
using System.Windows.Media;
using System.Windows.Interop;

namespace VideoConvert
{
    /// <summary>
    /// Interaktionslogik für "App.xaml"
    /// </summary>
    public partial class App
    {

        private static readonly ILog Log = LogManager.GetLogger(typeof(App));
        private static bool _clearLog;
        public static CultureInfo Cinfo = new CultureInfo("en-US");

        private void ApplicationStartup(object sender, StartupEventArgs e)
        {
            bool savedFirstStart = AppSettings.FirstStart;
            if (savedFirstStart)
            {
                AppSettings.Upgrade();
// ReSharper disable ConditionIsAlwaysTrueOrFalse
                AppSettings.FirstStart = savedFirstStart;
// ReSharper restore ConditionIsAlwaysTrueOrFalse
            }

            ReconfigureLogger();
            ReconfigureLanguage(AppSettings.UseLanguage);

            Log.InfoFormat("Use Language: {0:s}", AppSettings.UseLanguage);
            Log.InfoFormat("VideoConvert v{0:s} started", AppSettings.GetAppVersion().ToString(4));
            Log.InfoFormat("OS-Version: {0:s}", Environment.OSVersion.VersionString);
            Log.InfoFormat("CPU-Count: {0:g}", Environment.ProcessorCount);
            Log.InfoFormat(".NET Version: {0:s}", Environment.Version.ToString(4));
            Log.InfoFormat("System Uptime: {0:s}", TimeSpan.FromMilliseconds(Environment.TickCount).ToString("c"));

            bool elevated = false;
            try
            {
                elevated = Processing.IsProcessElevated();
            }
            catch (Exception)
            {
                Log.Error("Could not determine process elevation status");
            }

            if (Environment.OSVersion.Version.Major >= 6)
                Log.InfoFormat("Process Elevated: {0:s}", elevated.ToString(Cinfo));

            ReconfigureRenderMode();
        }

        private void ReconfigureLanguage(string lang)
        {
            if (String.IsNullOrEmpty(lang) || lang == "system") return;

            Thread.CurrentThread.CurrentUICulture = new CultureInfo(lang);
            Thread.CurrentThread.CurrentCulture = new CultureInfo(lang);
        }

        internal static void ReconfigureRenderMode()
        {
            if (AppSettings.UseHardwareRendering)
            {
                RenderOptions.ProcessRenderMode = RenderMode.Default;
                Log.Info("Hardware rendering enabled");
            }
            else
            {
                RenderOptions.ProcessRenderMode = RenderMode.SoftwareOnly;
                Log.Info("Hardware rendering disabled");
            }
        }

        internal static void ReconfigureLogger()
        {

            string logFile = Path.Combine(AppSettings.AppSettingsPath, "ErrorLog.txt");
            
            if (Log.Logger.Repository.Configured)
            {
                Log.Logger.Repository.Shutdown();
                Log.Logger.Repository.ResetConfiguration();
            }

            if (_clearLog)
            {
                try
                {
                    File.Delete(logFile);
                }
// ReSharper disable EmptyGeneralCatchClause
                catch (Exception)
// ReSharper restore EmptyGeneralCatchClause
                {

                }
                _clearLog = false;
            }

            PatternLayout layout = new PatternLayout();
            LevelRangeFilter filter = new LevelRangeFilter();

            if (AppSettings.UseDebug)
            {
                layout.ConversionPattern = "%date{HH:mm:ss} [%thread] %-5level %logger.%method(%line) - %message%newline";

                filter.LevelMin = Level.All;
            }
            else
            {
                layout.ConversionPattern = "%date{HH:mm:ss} %-5level %logger - %message%newline";

                filter.LevelMin = Level.Warn;
            }

            filter.AcceptOnMatch = true;

            layout.ActivateOptions();

            FileAppender file = new FileAppender
                {
                    Layout = layout,
                    AppendToFile = true,
                    Encoding = new UTF8Encoding(),
                    File = logFile,
                    ImmediateFlush = true
                };
            file.AddFilter(filter);

            file.ActivateOptions();

            BasicConfigurator.Configure(file);

            if (AppSettings.UseDebug)
            {
                Log.Info("Debug information enabled");
            }
        }

        internal static void ClearLogFile()
        {
            _clearLog = true;
            ReconfigureLogger();
        }

        private void ApplicationDispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Fatal(e.Exception);
        }

        private void ApplicationExit(object sender, ExitEventArgs e)
        {
            VideoConvert.Properties.Settings.Default.Save();
        }
    }
}
