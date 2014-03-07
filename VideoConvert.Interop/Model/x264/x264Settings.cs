// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Settings.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   The x264 Settings
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.x264
{
    using System;

    /// <summary>
    /// The x264 Settings
    /// </summary>
    public class X264Settings
    {
        /// <summary>
        /// Get default number of ref-frames for given parameters
        /// </summary>
        /// <param name="oPreset">encoding preset</param>
        /// <param name="oTuningMode">tuning mode</param>
        /// <param name="oDevice">target device</param>
        /// <returns>number of ref frames</returns>
        public static int GetDefaultNumberOfRefFrames(int oPreset, int oTuningMode, X264Device oDevice)
        {
            return GetDefaultNumberOfRefFrames(oPreset, oTuningMode, oDevice, -1, -1, -1);
        }

        /// <summary>
        /// Get default number of ref-frames for given parameters
        /// </summary>
        /// <param name="oPreset">encoding preset</param>
        /// <param name="oTuningMode">tuning mode</param>
        /// <param name="oDevice">target device</param>
        /// <param name="iLevel">AVC-Level</param>
        /// <param name="hRes">horizontal resolution</param>
        /// <param name="vRes">vertical resolution</param>
        /// <returns>number of ref frames</returns>
        public static int GetDefaultNumberOfRefFrames(int oPreset, int oTuningMode, X264Device oDevice, int iLevel, int hRes, int vRes)
        {
            var iDefaultSetting = 1;
            switch (oPreset)
            {
                case 0:
                case 1:
                case 2: iDefaultSetting = 1; break;
                case 3:
                case 4: iDefaultSetting = 2; break;
                case 5: iDefaultSetting = 3; break;
                case 6: iDefaultSetting = 5; break;
                case 7: iDefaultSetting = 8; break;
                case 8:
                case 9: iDefaultSetting = 16; break;
            }

            if (oTuningMode == 1 && iDefaultSetting > 1) // animation
                iDefaultSetting *= 2;

            if (iDefaultSetting > 16)
                iDefaultSetting = 16;

            if (oDevice != null && oDevice.ReferenceFrames > -1)
                iDefaultSetting = Math.Min(oDevice.ReferenceFrames, iDefaultSetting);

            if (iLevel > -1 && hRes > 0 && vRes > 0)
            {
                var iMaxRefForLevel = GetMaxRefForLevel(iLevel, hRes, vRes);
                if (iMaxRefForLevel > -1 && iMaxRefForLevel < iDefaultSetting)
                    iDefaultSetting = iMaxRefForLevel;
            }

            return iDefaultSetting;
        }

        /// <summary>
        /// Get maximum ref frames for given AVC-level
        /// </summary>
        /// <param name="level">AVC-Level</param>
        /// <param name="hRes">horizontal resolution</param>
        /// <param name="vRes">vertical resolution</param>
        /// <returns>number of ref frames</returns>
        public static int GetMaxRefForLevel(int level, int hRes, int vRes)
        {
            if (level < 0 || hRes <= 0 || vRes <= 0 || level >= 16)  // Unrestricted/Autoguess
                return -1;

            var maxDpb = 0;  // the maximum picture decoded buffer for the given level
            switch (level)
            {
                case 0: // level 1
                    maxDpb = 396;
                    break;
                case 1: // level 1.1
                    maxDpb = 900;
                    break;
                case 2: // level 1.2
                    maxDpb = 2376;
                    break;
                case 3: // level 1.3
                    maxDpb = 2376;
                    break;
                case 4: // level 2
                    maxDpb = 2376;
                    break;
                case 5: // level 2.1
                    maxDpb = 4752;
                    break;
                case 6: // level 2.2
                    maxDpb = 8100;
                    break;
                case 7: // level 3
                    maxDpb = 8100;
                    break;
                case 8: // level 3.1
                    maxDpb = 18000;
                    break;
                case 9: // level 3.2
                    maxDpb = 20480;
                    break;
                case 10: // level 4
                    maxDpb = 32768;
                    break;
                case 11: // level 4.1
                    maxDpb = 32768;
                    break;
                case 12: // level 4.2
                    maxDpb = 34816;
                    break;
                case 13: // level 5
                    maxDpb = 110400;
                    break;
                case 14: // level 5.1
                    maxDpb = 184320;
                    break;
                case 15: // level 5.2
                    maxDpb = 184320;
                    break;
            }

            var frameHeightInMbs = (int)Math.Ceiling((double)vRes / 16);
            var frameWidthInMbs = (int)Math.Ceiling((double)hRes / 16);
            var maxRef = (int)Math.Floor((double)maxDpb / (frameHeightInMbs * frameWidthInMbs));
            return Math.Min(maxRef, 16);
        }

        /// <summary>
        /// Get default number of B-frames for given parameters
        /// </summary>
        /// <param name="oPresetLevel">encoding preset</param>
        /// <param name="oTuningMode">tuning mode</param>
        /// <param name="oAvcProfile">AVC-Profile</param>
        /// <param name="oDevice">Target device</param>
        /// <returns>number of b-frames</returns>
        public static int GetDefaultNumberOfBFrames(int oPresetLevel, int oTuningMode, int oAvcProfile, X264Device oDevice)
        {
            var iDefaultSetting = 0;
            if (oAvcProfile == 0) // baseline
                return iDefaultSetting;

            switch (oPresetLevel)
            {
                case 0: iDefaultSetting = 0; break;
                case 1:
                case 2:
                case 3:
                case 4:
                case 5:
                case 6:
                case 7: iDefaultSetting = 3; break;
                case 8: iDefaultSetting = 8; break;
                case 9: iDefaultSetting = 16; break;
            }

            if (oTuningMode == 1) // animation
                iDefaultSetting += 2;
            if (iDefaultSetting > 16)
                iDefaultSetting = 16;

            if (oDevice != null && oDevice.BFrames > -1)
                return Math.Min(oDevice.BFrames, iDefaultSetting);
            return iDefaultSetting;
        }

        /// <summary>
        /// Get default weightp value for given parameters
        /// </summary>
        /// <param name="oPresetLevel">encoding preset</param>
        /// <param name="oTuningMode">tuning mode</param>
        /// <param name="oAvcProfile">AVC-Profile</param>
        /// <param name="bBlurayCompat">bluray compatibility</param>
        /// <returns>weightp value</returns>
        public static int GetDefaultNumberOfWeightp(int oPresetLevel, int oTuningMode, int oAvcProfile, bool bBlurayCompat)
        {
            if (oAvcProfile == 0) // baseline
                return 0;
            if (oTuningMode == 6) // Fast Decode
                return 0;

            var iDefaultSetting = 0;
            switch (oPresetLevel)
            {
                case 0: iDefaultSetting = 0; break;
                case 1:
                case 2:
                case 3:
                case 4: iDefaultSetting = 1; break;
                case 5:
                case 6:
                case 7:
                case 8:
                case 9: iDefaultSetting = 2; break;
            }
            return bBlurayCompat ? Math.Min(iDefaultSetting, 1) : iDefaultSetting;
        }

        /// <summary>
        /// Get default AQ mode
        /// </summary>
        /// <param name="oPresetLevel">encoding preset</param>
        /// <param name="oTuningMode">tuning mode</param>
        /// <returns>AQ Mode</returns>
        public static int GetDefaultAqMode(int oPresetLevel, int oTuningMode)
        {
            if (oTuningMode == 5) // SSIM
                return 2;

            if (oTuningMode == 4 || oPresetLevel == 0) // PSNR
                return 0;

            return 1;
        }

        /// <summary>
        /// Gets minimum required AVC Level for encoding
        /// </summary>
        /// <param name="hRes"></param>
        /// <param name="vRes"></param>
        /// <param name="fpsN"></param>
        /// <param name="fpsD"></param>
        /// <param name="bitrate"></param>
        /// <param name="encMode"></param>
        /// <param name="avcProfile"></param>
        /// <returns></returns>
        public static int GetMinLevelForRes(int hRes, int vRes, int fpsN, int fpsD, int bitrate, int encMode, int avcProfile)
        {
            float fps = (float) fpsN / fpsD;

            int mBlocksWidth = (int) Math.Ceiling((double) hRes / 16);
            int mBlocksHeight = (int) Math.Ceiling((double) vRes / 16);

            int mBlocksSec = (int) Math.Ceiling(mBlocksWidth * mBlocksHeight * fps);

            int avcLevel = 16;

            if (mBlocksSec <= 1485)
                avcLevel = 0; // avc level 1 / 1b
            else if (mBlocksSec <= 3000)
                avcLevel = 1; // avc level 1.1
            else if (mBlocksSec <= 6000)
                avcLevel = 2; // avc level 1.2
            else if (mBlocksSec <= 11880)
                avcLevel = 3; // avc level 1.3
            else if (mBlocksSec <= 19800)
                avcLevel = 5; // avc level 2.1
            else if (mBlocksSec <= 20250)
                avcLevel = 6; // avc level 2.2
            else if (mBlocksSec <= 40500)
                avcLevel = 7; // avc level 3
            else if (mBlocksSec <= 108000)
                avcLevel = 8; // avc level 3.1
            else if (mBlocksSec <= 216000)
                avcLevel = 9; // avc level 3.2
            else if (mBlocksSec <= 245760)
                avcLevel = 10; // avc level 4
            else if (mBlocksSec <= 522240)
                avcLevel = 12; // avc level 4.2
            else if (mBlocksSec <= 589824)
                avcLevel = 13; // avc level 5
            else if (mBlocksSec <= 983040)
                avcLevel = 14; // avc level 5.1
            else if (mBlocksSec <= 2073600)
                avcLevel = 15; // avc level 5.2

            switch (avcLevel)
            {
                case 3: // avc level 1.3
                    switch (encMode)
                    {
                        case 0: // abr
                        case 2: // 2-pass
                        case 3: // 3-pass
                            switch (avcProfile)
                            {
                                case 0: // baseline
                                case 1: // main
                                    if (bitrate > 768)
                                        avcLevel = 4; // avc level 2
                                    break;
                                default: // high
                                    if (bitrate > 960)
                                        avcLevel = 4; // avc level 2
                                    break;
                            }
                            break;
                    }
                    break;
                case 10:
                    switch (encMode)
                    {
                        case 0: // abr
                        case 2: // 2-pass
                        case 3: // 3-pass
                            switch (avcProfile)
                            {
                                case 0: // baseline
                                case 1: // main
                                    if (bitrate > 20000)
                                        avcLevel = 11; // avc level 4.1
                                    break;
                                default: // high
                                    if (bitrate > 25000)
                                        avcLevel = 11; // avc level 4.1
                                    break;
                            }
                            break;
                    }
                    break;
            }

            return avcLevel;
        }
    }
}
