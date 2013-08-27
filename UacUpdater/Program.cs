using System;
using System.Windows.Forms;
using System.Linq;

namespace UacUpdater
{
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
            if (parameters == null || parameters.Count() < 3) return;

            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new UpdaterForm());
        }
    }
}
