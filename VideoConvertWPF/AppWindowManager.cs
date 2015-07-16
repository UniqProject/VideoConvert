// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AppWindowManager.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF
{
    using Caliburn.Metro.Core;
    using MahApps.Metro.Controls;
    using VideoConvertWPF.Views;

    public class AppWindowManager : MetroWindowManager
    {
        public override MetroWindow CreateCustomWindow(object view, bool windowIsView)
        {
            if (windowIsView)
            {
                return view as ShellView;
            }

            return new ShellView
            {
                Content = view
            };
        }
    }
}