// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ImageHolder.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Rendered subtitle caption
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.Subtitles
{
    /// <summary>
    /// Rendered subtitle caption
    /// </summary>
    public class ImageHolder
    {
        /// <summary>
        /// Path to image file
        /// </summary>
        public string FileName;

        /// <summary>
        /// Image width
        /// </summary>
        public int Width;

        /// <summary>
        /// Image height
        /// </summary>
        public int Height;
        
        /// <summary>
        /// Default constructor
        /// </summary>
        public ImageHolder()
        {
            FileName = string.Empty;
            Width = 0;
            Height = 0;
        }
    }
}