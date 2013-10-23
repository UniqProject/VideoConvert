// --------------------------------------------------------------------------------------------------------------------
// <copyright file="IAviSynthGenerator.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices.Utilities.Interfaces
{
    using System.Drawing;
    using Interop.Model;

    internal interface IAviSynthGenerator
    {
        /// <summary>
        /// Generates AviSynth script used for video encoding
        /// </summary>
        /// <param name="videoInfo">All video properties</param>
        /// <param name="changeFps">Defines whether framerate should be changed</param>
        /// <param name="targetFps">Sets target framerate</param>
        /// <param name="resizeTo">Sets target video resolution</param>
        /// <param name="stereoEncoding">Defines, which stereo encoding mode should be used</param>
        /// <param name="stereoVideoInfo">Sets all parameters for stereo encoding</param>
        /// <param name="isDvdResolution">Defines whether target resolution is used for DVD encoding</param>
        /// <param name="subtitleFile">Sets subtitle file for hardcoding into video</param>
        /// <param name="subtitleOnlyForced">Defines whether only forced captions should be hardcoded</param>
        /// <param name="skipScaling"></param>
        /// <returns>Path to AviSynth script</returns>
        string Generate(VideoInfo videoInfo, bool changeFps, float targetFps, Size resizeTo,
            StereoEncoding stereoEncoding, StereoVideoInfo stereoVideoInfo, bool isDvdResolution,
            string subtitleFile, bool subtitleOnlyForced, bool skipScaling);

        /// <summary>
        /// Creates AviSynth script used to determine black borders for cropping
        /// </summary>
        /// <param name="inputFile">Path to source file</param>
        /// <param name="targetFps">Sets framerate of the source file</param>
        /// <param name="streamLength">Sets duration of the source file, in seconds</param>
        /// <param name="videoSize"></param>
        /// <param name="aspectRatio"></param>
        /// <param name="frameCount">Calculated amount of frames</param>
        /// <returns>Path to AviSynth script</returns>
        string GenerateCropDetect(string inputFile, float targetFps, double streamLength, Size videoSize,
            float aspectRatio, out int frameCount);

        /// <summary>
        /// Generates simple script used to check whether AviSynth is installed on system
        /// </summary>
        /// <returns>Path to AviSynth script</returns>
        string GenerateTestFile();

        /// <summary>
        /// Generates AviSynth script used for audio encoding
        /// </summary>
        /// <param name="inputFile">Path to input file</param>
        /// <param name="inFormat">Format of input file</param>
        /// <param name="inFormatProfile">Format profile of input file</param>
        /// <param name="inChannels">Channel count of input file</param>
        /// <param name="outChannels">Target channel count</param>
        /// <param name="inSampleRate">Samplerate of input file</param>
        /// <param name="outSampleRate">Target samplerate</param>
        /// <returns>Path to AviSynth script</returns>
        string GenerateAudioScript(string inputFile, string inFormat, string inFormatProfile, 
            int inChannels, int outChannels, int inSampleRate, 
            int outSampleRate);

        /// <summary>
        /// Imports NicAudio plugin
        /// </summary>
        /// <returns></returns>
        string ImportNicAudio();

        /// <summary>
        /// Imports ffmpegsource (ffms2) plugin
        /// </summary>
        /// <returns></returns>
        string ImportFFMPEGSource();
    }
}