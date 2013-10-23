// --------------------------------------------------------------------------------------------------------------------
// <copyright file="x264Settings.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.Interop source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.Interop.Model.x264
{
    using System;

    public class X264Settings
    {
        public static int GetDefaultNumberOfRefFrames(int oPreset, int oTuningMode, X264Device oDevice)
        {
            return GetDefaultNumberOfRefFrames(oPreset, oTuningMode, oDevice, -1, -1, -1);
        }

        public static int GetDefaultNumberOfRefFrames(int oPreset, int oTuningMode, X264Device oDevice, int iLevel, int hRes, int vRes)
        {
            int iDefaultSetting = 1;
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
                int iMaxRefForLevel = GetMaxRefForLevel(iLevel, hRes, vRes);
                if (iMaxRefForLevel > -1 && iMaxRefForLevel < iDefaultSetting)
                    iDefaultSetting = iMaxRefForLevel;
            }

            return iDefaultSetting;
        }

        public static int GetMaxRefForLevel(int level, int hRes, int vRes)
        {
            if (level < 0 || hRes <= 0 || vRes <= 0 || level >= 15)  // Unrestricted/Autoguess
                return -1;

            int maxDPB = 0;  // the maximum picture decoded buffer for the given level
            switch (level)
            {
                case 0: // level 1
                    maxDPB = 396;
                    break;
                case 1: // level 1.1
                    maxDPB = 900;
                    break;
                case 2: // level 1.2
                    maxDPB = 2376;
                    break;
                case 3: // level 1.3
                    maxDPB = 2376;
                    break;
                case 4: // level 2
                    maxDPB = 2376;
                    break;
                case 5: // level 2.1
                    maxDPB = 4752;
                    break;
                case 6: // level 2.2
                    maxDPB = 8100;
                    break;
                case 7: // level 3
                    maxDPB = 8100;
                    break;
                case 8: // level 3.1
                    maxDPB = 18000;
                    break;
                case 9: // level 3.2
                    maxDPB = 20480;
                    break;
                case 10: // level 4
                    maxDPB = 32768;
                    break;
                case 11: // level 4.1
                    maxDPB = 32768;
                    break;
                case 12: // level 4.2
                    maxDPB = 34816;
                    break;
                case 13: // level 5
                    maxDPB = 110400;
                    break;
                case 14: // level 5.1
                    maxDPB = 184320;
                    break;
            }

            int frameHeightInMbs = (int)Math.Ceiling((double)vRes / 16);
            int frameWidthInMbs = (int)Math.Ceiling((double)hRes / 16);
            int maxRef = (int)Math.Floor((double)maxDPB / (frameHeightInMbs * frameWidthInMbs));
            return Math.Min(maxRef, 16);
        }

        public static int GetDefaultNumberOfBFrames(int oPresetLevel, int oTuningMode, int oAVCProfile, X264Device oDevice)
        {
            int iDefaultSetting = 0;
            if (oAVCProfile == 0) // baseline
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

        public static int GetDefaultNumberOfWeightp(int oPresetLevel, int oTuningMode, int oAVCProfile, bool bBlurayCompat)
        {
            if (oAVCProfile == 0) // baseline
                return 0;
            if (oTuningMode == 6) // Fast Decode
                return 0;

            int iDefaultSetting = 0;
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

        public static int GetDefaultAQMode(int oPresetLevel, int oTuningMode)
        {
            if (oTuningMode == 5) // SSIM
                return 2;

            if (oTuningMode == 4 || oPresetLevel == 0) // PSNR
                return 0;

            return 1;
        }
    }
}
