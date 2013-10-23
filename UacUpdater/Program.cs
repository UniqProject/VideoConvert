// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Program.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the UacUpdater source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace UacUpdater
{
    using System;
    using System.Windows.Forms;

    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            string[] parameters = null;
            try
            {
                parameters = Environment.GetCommandLineArgs();
            }
            catch (Exception ex)
            {

            }
            if (parameters == null || parameters.Length < 3) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdaterForm());
        }
    }
}
