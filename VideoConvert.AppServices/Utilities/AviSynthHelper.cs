// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AviSynthHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Avisynth helper class
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Utilities
{
    using Interop.Model;
    using Services.Interfaces;
    using System.Drawing;
    using System.Linq;

    /// <summary>
    /// Avisynth helper class
    /// </summary>
    public class AviSynthHelper
    {
        private readonly IAppConfigService _appConfig;

        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="appConfig">Application configuration service</param>
        public AviSynthHelper(IAppConfigService appConfig)
        {
            this._appConfig = appConfig;
        }

        /// <summary>
        /// Creates a new AviSynth script
        /// </summary>
        /// <param name="task"></param>
        /// <param name="resizeTo"></param>
        public void GenerateAviSynthScript(EncodeInfo task, Size resizeTo)
        {
            var sub = task.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            var subFile = string.Empty;
            var keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            var avs = new AviSynthGenerator(_appConfig);

            if ((task.EncodingProfile.OutFormat == OutputType.OutputBluRay) ||
                (task.EncodingProfile.OutFormat == OutputType.OutputAvchd))
            {
                task.AviSynthScript = avs.Generate(task.VideoStream,
                                                    false,
                                                    0f,
                                                    resizeTo,
                                                    StereoEncoding.None, new StereoVideoInfo(),
                                                    false,
                                                    subFile,
                                                    keepOnlyForced,
                                                    _appConfig.Use64BitEncoders 
                                                    && _appConfig.UseFfmpegScaling);
            }
            else
            {
                task.AviSynthScript = avs.Generate(task.VideoStream,
                                                    false,
                                                    0f,
                                                    resizeTo,
                                                    task.EncodingProfile.StereoType,
                                                    task.StereoVideoStream,
                                                    false,
                                                    subFile,
                                                    keepOnlyForced,
                                                    _appConfig.Use64BitEncoders &&
                                                    _appConfig.UseFfmpegScaling);
                if (!string.IsNullOrEmpty(avs.StereoConfigFile))
                    task.AviSynthStereoConfig = avs.StereoConfigFile;
            }
        }
    }
}