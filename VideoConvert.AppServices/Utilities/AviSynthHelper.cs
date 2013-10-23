// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AviSynthHelper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Utilities
{
    using System.Drawing;
    using System.Linq;
    using Interop.Model;
    using Services.Interfaces;

    public class AviSynthHelper
    {
        private readonly IAppConfigService _appConfig;

        public AviSynthHelper(IAppConfigService appConfig)
        {
            this._appConfig = appConfig;
        }

        public void GenerateAviSynthScript(EncodeInfo task, Size resizeTo)
        {
            SubtitleInfo sub = task.SubtitleStreams.FirstOrDefault(item => item.HardSubIntoVideo);
            string subFile = string.Empty;
            bool keepOnlyForced = false;
            if (sub != null)
            {
                subFile = sub.TempFile;
                keepOnlyForced = sub.KeepOnlyForcedCaptions;
            }

            AviSynthGenerator avs = new AviSynthGenerator(_appConfig);

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