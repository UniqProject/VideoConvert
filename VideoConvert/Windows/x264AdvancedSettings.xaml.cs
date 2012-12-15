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

using System.Windows;
using VideoConvert.Core.Profiles;

namespace VideoConvert.Windows
{
    /// <summary>
    /// Interaktionslogik für x264AdvancedSettings.xaml
    /// </summary>
    public partial class X264AdvancedSettings
    {
        public X264Profile Profile { get; set; }
        
        public X264AdvancedSettings()
        {
            InitializeComponent();
        }

        private void WindowLoaded(object sender, RoutedEventArgs e)
        {
            SetSettingsFromProfile();
        }

        public void SetProfile(X264Profile inProfile)
        {
            Profile = inProfile;
        }

        private void SetSettingsFromProfile()
        {
            UseDeblocking.IsChecked = Profile.UseDeblocking;
            DeblockingStrength.Value = Profile.DeblockingStrength;
            DeblockingThreshold.Value = Profile.DeblockingThreshold;

            UseCABAC.IsEnabled = Profile.AVCProfile > 0;
            UseCABAC.IsChecked = Profile.UseCabac;

            GOPCalculation.SelectedIndex = Profile.GopCalculation;

            MaxGOPSize.Value = Profile.MaxGopSize;
            MinGOPSize.Value = Profile.MinGopSize;

            UseOpenGOP.IsChecked = Profile.UseOpenGop;

            NumSlices.Value = Profile.NumSlices;
            MaxSliceSizeBytes.Value = Profile.MaxSliceSizeBytes;
            MaxSliceSizeBlocks.Value = Profile.MaxSliceSizeBlocks;

            UseWeightedPred.IsEnabled = Profile.AVCProfile > 0;
            UseWeightedPred.IsChecked = Profile.AVCProfile > 0 && Profile.UseWeightedPred;

            BFrameBias.IsEnabled = Profile.AVCProfile > 0;
            BFrameBias.Value = Profile.BFrameBias;

            AdaptiveBFrames.IsEnabled = Profile.AVCProfile > 0;
            AdaptiveBFrames.SelectedIndex = Profile.AdaptiveBFrames;

            BPyramid.IsEnabled = Profile.AVCProfile > 0;
            BPyramid.SelectedIndex = Profile.BPyramid;

            // Initialise as last to prevent exceptions
            NumBFrames.IsEnabled = Profile.AVCProfile > 0;
            NumBFrames.Value = 16;
            NumBFrames.Value = Profile.NumBFrames;

            NumRef.Value = Profile.NumRefFrames;
            NumExtraIFrames.Value = Profile.NumExtraIFrames;

            PFrameWeightedPrediction.SelectedIndex = Profile.PFrameWeightedPrediction;

            InterlacedMode.SelectedIndex = Profile.InterlaceMode;

            Pulldown.SelectedIndex = Profile.Pulldown;
            UseAdaptiveIFrameDecision.IsChecked = Profile.UseAdaptiveIFrameDecision;

            QuantizerMin.Value = Profile.QuantizerMin;
            QuantizerMax.Value = Profile.QuantizerMax;
            QuantizerDelta.Value = Profile.QuantizerDelta;
            QuantizerRatioIP.Value = Profile.QuantizerRatioIP;
            QuantizersRatioPB.Value = Profile.QuantizerRatioPB;
            QuantizerDeadZoneInter.Value = Profile.DeadZoneInter;
            QuantizerDeadZoneIntra.Value = Profile.DeadZoneIntra;
            QuantizerChromaQPOffset.Value = Profile.ChromaQPOffset;
            QuantizerCredits.Value = Profile.CreditsQuantizer;

            VBVBuffSize.IsEnabled = Profile.EncodingMode != 1;
            VBVBuffSize.Value = Profile.VBVBufSize;

            VBVMaxRate.IsEnabled = Profile.EncodingMode != 1;
            VBVMaxRate.Value = Profile.VBVMaxRate;

            VBVInitialBuffer.IsEnabled = Profile.EncodingMode != 1;
            VBVInitialBuffer.Value = Profile.VBVInitialBuffer;

            BitrateVariance.IsEnabled = Profile.EncodingMode != 1 && Profile.EncodingMode != 4;
            BitrateVariance.Value = Profile.BitrateVariance;

            QuantizerCompression.IsEnabled = Profile.EncodingMode != 1;
            QuantizerCompression.Value = Profile.QuantizerCompression;

            TempBlurFrameComplexity.IsEnabled = Profile.EncodingMode != 1 && Profile.EncodingMode != 4;
            TempBlurFrameComplexity.Value = Profile.TempBlurFrameComplexity;

            TempBlurQuant.IsEnabled = Profile.EncodingMode != 1 && Profile.EncodingMode != 4;
            TempBlurQuant.Value = Profile.TempBlurQuant;

            AdaptiveQuantizersMode.IsEnabled = Profile.EncodingMode != 1;
            AdaptiveQuantizersMode.SelectedIndex = Profile.AdaptiveQuantizersMode;

            AdaptiveQuantizersStrength.IsEnabled = Profile.EncodingMode != 1;
            AdaptiveQuantizersStrength.Value = Profile.AdaptiveQuantizersStrength;

            QuantizerMatrix.IsEnabled = Profile.AVCProfile > 1;
            QuantizerMatrix.SelectedIndex = Profile.QuantizerMatrix;

            NumFramesLookahead.IsEnabled = Profile.EncodingMode != 1;
            NumFramesLookahead.Value = Profile.NumFramesLookahead;

            UseMBTree.IsEnabled = Profile.EncodingMode != 1;
            UseMBTree.IsChecked = Profile.EncodingMode == 1 || Profile.UseMBTree;

            UseChromaMotionEstimation.IsChecked = Profile.UseChromaMotionEstimation;
            MotionEstimationRange.Value = Profile.MotionEstimationRange;
            MotionEstimationAlgorithm.SelectedIndex = Profile.MotionEstimationAlgorithm;
            SubPixelRefinement.SelectedIndex = Profile.SubPixelRefinement;

            MVPredictionMod.IsEnabled = Profile.AVCProfile > 0 && Profile.NumBFrames > 0;
            MVPredictionMod.SelectedIndex = Profile.AVCProfile > 0 && Profile.NumBFrames > 0 ? Profile.MVPredictionMod : 2;

            Trellis.SelectedIndex = Profile.Trellis;
            PsyRDStrength.Value = Profile.PsyRDStrength;
            PsyTrellisStrength.Value = Profile.PsyTrellisStrength;

            UseNoMixedReferenceFrames.IsEnabled = Profile.AVCProfile > 0 && Profile.NumRefFrames > 1;
            UseNoMixedReferenceFrames.IsChecked = Profile.UseNoMixedReferenceFrames && Profile.NumRefFrames > 1;

            UseNoDCTDecimation.IsChecked = Profile.UseNoDCTDecimation;
            UseNoFastPSkip.IsChecked = Profile.UseNoFastPSkip;
            UseNoPsychovisualEnhancements.IsChecked = Profile.UseNoPsychovisualEnhancements;
            NoiseReduction.Value = Profile.NoiseReduction;

            MacroBlocksPartitionsAdaptiveDCT.IsEnabled = Profile.AVCProfile > 1;
            MacroBlocksPartitionsAdaptiveDCT.IsChecked = Profile.MacroBlocksPartitionsAdaptiveDCT && Profile.AVCProfile > 1;

            MacroBlocksPartitionsI4x4.IsChecked = Profile.MacroBlocksPartitionsI4X4;
            MacroBlocksPartitionsP4x4.IsChecked = Profile.MacroBlocksPartitionsP4X4;
            MacroBlocksPartitionsI8x8.IsChecked = Profile.MacroBlocksPartitionsI8X8;
            MacroBlocksPartitionsP8x8.IsChecked = Profile.MacroBlocksPartitionsP8X8;
            MacroBlocksPartitionsB8x8.IsChecked = Profile.MacroBlocksPartitionsB8X8;
            
            MacroBlocksPartitions.SelectedIndex = Profile.AVCProfile > 0 ? Profile.MacroBlocksPartitions : 2;

            HRDInfo.SelectedIndex = Profile.HRDInfo;
            UseAccessUnitDelimiters.IsChecked = Profile.UseAccessUnitDelimiters;
            UseFakeInterlaced.IsChecked = Profile.UseFakeInterlaced;
            UseBluRayCompatibility.IsChecked = Profile.UseBluRayCompatibility;

            VUIRange.SelectedIndex = Profile.VUIRange;
            UseForcePicStruct.IsChecked = Profile.UseForcePicStruct;
            ColorPrimaries.SelectedIndex = Profile.ColorPrimaries;
            Transfer.SelectedIndex = Profile.Transfer;
            ColorMatrix.SelectedIndex = Profile.ColorMatrix;
            UseAutoSelectColorSettings.IsChecked = Profile.UseAutoSelectColorSettings;

            UsePSNRCalculation.IsChecked = Profile.UsePSNRCalculation;
            UseSSIMCalculation.IsChecked = Profile.UseSSIMCalculation;
            ForceSAR.SelectedIndex = Profile.ForceSAR;
            UseAutoSelectSAR.IsChecked = Profile.UseAutoSelectSAR;

            NumThreads.Value = Profile.NumThreads;
            UseNonDeterministic.IsChecked = Profile.UseNonDeterministic;
            UseThreadInput.IsChecked = Profile.UseThreadInput;
            UseSlowFirstPass.IsChecked = Profile.UseSlowFirstPass;

            CustomCommandLine.Text = Profile.CustomCommandLine;
        }

        private void SaveSettingsToProfile()
        {
            Profile.UseDeblocking = UseDeblocking.IsChecked.GetValueOrDefault();
            Profile.DeblockingStrength = DeblockingStrength.Value.GetValueOrDefault();
            Profile.DeblockingThreshold = DeblockingThreshold.Value.GetValueOrDefault();
            Profile.UseCabac = UseCABAC.IsChecked.GetValueOrDefault();

            Profile.GopCalculation = GOPCalculation.SelectedIndex;
            Profile.MaxGopSize = MaxGOPSize.Value.GetValueOrDefault();
            Profile.MinGopSize = MinGOPSize.Value.GetValueOrDefault();
            Profile.UseOpenGop = UseOpenGOP.IsChecked.GetValueOrDefault();

            Profile.NumSlices = NumSlices.Value.GetValueOrDefault();
            Profile.MaxSliceSizeBytes = MaxSliceSizeBytes.Value.GetValueOrDefault();
            Profile.MaxSliceSizeBlocks = MaxSliceSizeBlocks.Value.GetValueOrDefault();

            Profile.UseWeightedPred = UseWeightedPred.IsChecked.GetValueOrDefault();
            Profile.BFrameBias = BFrameBias.Value.GetValueOrDefault();
            Profile.AdaptiveBFrames = AdaptiveBFrames.SelectedIndex;
            Profile.BPyramid = BPyramid.SelectedIndex;
            Profile.NumBFrames = NumBFrames.Value.GetValueOrDefault();

            Profile.NumRefFrames = NumRef.Value.GetValueOrDefault();
            Profile.NumExtraIFrames = NumExtraIFrames.Value.GetValueOrDefault();
            Profile.PFrameWeightedPrediction = PFrameWeightedPrediction.SelectedIndex;
            Profile.InterlaceMode = InterlacedMode.SelectedIndex;
            Profile.Pulldown = Pulldown.SelectedIndex;
            Profile.UseAdaptiveIFrameDecision = UseAdaptiveIFrameDecision.IsChecked.GetValueOrDefault();

            Profile.QuantizerMin = QuantizerMin.Value.GetValueOrDefault();
            Profile.QuantizerMax = QuantizerMax.Value.GetValueOrDefault();
            Profile.QuantizerDelta = QuantizerDelta.Value.GetValueOrDefault();
            Profile.QuantizerRatioIP = (float)QuantizerRatioIP.Value.GetValueOrDefault();
            Profile.QuantizerRatioPB = (float)QuantizersRatioPB.Value.GetValueOrDefault();
            Profile.DeadZoneInter = QuantizerDeadZoneInter.Value.GetValueOrDefault();
            Profile.DeadZoneIntra = QuantizerDeadZoneIntra.Value.GetValueOrDefault();
            Profile.ChromaQPOffset = QuantizerChromaQPOffset.Value.GetValueOrDefault();
            Profile.CreditsQuantizer = QuantizerCredits.Value.GetValueOrDefault();

            Profile.VBVBufSize = VBVBuffSize.Value.GetValueOrDefault();
            Profile.VBVMaxRate = VBVMaxRate.Value.GetValueOrDefault();
            Profile.VBVInitialBuffer = (float)VBVInitialBuffer.Value.GetValueOrDefault();
            Profile.BitrateVariance = (float)BitrateVariance.Value.GetValueOrDefault();
            Profile.QuantizerCompression = (float)QuantizerCompression.Value.GetValueOrDefault();
            Profile.TempBlurFrameComplexity = TempBlurFrameComplexity.Value.GetValueOrDefault();
            Profile.TempBlurQuant = (float)TempBlurQuant.Value.GetValueOrDefault();

            Profile.AdaptiveQuantizersMode = AdaptiveQuantizersMode.SelectedIndex;
            Profile.AdaptiveQuantizersStrength = (float)AdaptiveQuantizersStrength.Value.GetValueOrDefault();

            Profile.QuantizerMatrix = QuantizerMatrix.SelectedIndex;

            Profile.NumFramesLookahead = NumFramesLookahead.Value.GetValueOrDefault();

            Profile.UseMBTree = UseMBTree.IsChecked.GetValueOrDefault();

            Profile.UseChromaMotionEstimation = UseChromaMotionEstimation.IsChecked.GetValueOrDefault();
            Profile.MotionEstimationRange = MotionEstimationRange.Value.GetValueOrDefault();
            Profile.MotionEstimationAlgorithm = MotionEstimationAlgorithm.SelectedIndex;
            Profile.SubPixelRefinement = SubPixelRefinement.SelectedIndex;

            Profile.MVPredictionMod = MVPredictionMod.SelectedIndex;
            Profile.Trellis = Trellis.SelectedIndex;
            Profile.PsyRDStrength = (float)PsyRDStrength.Value.GetValueOrDefault();
            Profile.PsyTrellisStrength = (float)PsyTrellisStrength.Value.GetValueOrDefault();
            Profile.UseNoMixedReferenceFrames = UseNoMixedReferenceFrames.IsChecked.GetValueOrDefault();
            Profile.UseNoDCTDecimation = UseNoDCTDecimation.IsChecked.GetValueOrDefault();
            Profile.UseNoFastPSkip = UseNoFastPSkip.IsChecked.GetValueOrDefault();
            Profile.UseNoPsychovisualEnhancements = UseNoPsychovisualEnhancements.IsChecked.GetValueOrDefault();
            Profile.NoiseReduction = NoiseReduction.Value.GetValueOrDefault();

            Profile.MacroBlocksPartitionsAdaptiveDCT = MacroBlocksPartitionsAdaptiveDCT.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitionsI4X4 = MacroBlocksPartitionsI4x4.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitionsP4X4 = MacroBlocksPartitionsP4x4.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitionsI8X8 = MacroBlocksPartitionsI8x8.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitionsP8X8 = MacroBlocksPartitionsP8x8.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitionsB8X8 = MacroBlocksPartitionsB8x8.IsChecked.GetValueOrDefault();
            Profile.MacroBlocksPartitions = MacroBlocksPartitions.SelectedIndex;

            Profile.HRDInfo = HRDInfo.SelectedIndex;
            Profile.UseAccessUnitDelimiters = UseAccessUnitDelimiters.IsChecked.GetValueOrDefault();
            Profile.UseFakeInterlaced = UseFakeInterlaced.IsChecked.GetValueOrDefault();
            Profile.UseBluRayCompatibility = UseBluRayCompatibility.IsChecked.GetValueOrDefault();

            Profile.VUIRange = VUIRange.SelectedIndex;
            Profile.UseForcePicStruct = UseForcePicStruct.IsChecked.GetValueOrDefault();
            Profile.ColorPrimaries = ColorPrimaries.SelectedIndex;
            Profile.Transfer = Transfer.SelectedIndex;
            Profile.ColorMatrix = ColorMatrix.SelectedIndex;
            Profile.UseAutoSelectColorSettings = UseAutoSelectColorSettings.IsChecked.GetValueOrDefault();

            Profile.UsePSNRCalculation = UsePSNRCalculation.IsChecked.GetValueOrDefault();
            Profile.UseSSIMCalculation = UseSSIMCalculation.IsChecked.GetValueOrDefault();
            Profile.ForceSAR = ForceSAR.SelectedIndex;
            Profile.UseAutoSelectSAR = UseAutoSelectSAR.IsChecked.GetValueOrDefault();

            Profile.NumThreads = NumThreads.Value.GetValueOrDefault();
            Profile.UseNonDeterministic = UseNonDeterministic.IsChecked.GetValueOrDefault();
            Profile.UseThreadInput = UseThreadInput.IsChecked.GetValueOrDefault();
            Profile.UseSlowFirstPass = UseSlowFirstPass.IsChecked.GetValueOrDefault();

            Profile.CustomCommandLine = CustomCommandLine.Text;
        }

        private void OkBtnClick(object sender, RoutedEventArgs e)
        {
            SaveSettingsToProfile();
            DialogResult = true;
        }


        private void MacroBlocksPartitionsSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            MacroBlocksPartitionsAdaptiveDCT.IsEnabled = (MacroBlocksPartitions.SelectedIndex == 2) && (Profile.AVCProfile > 1);
            MacroBlocksPartitionsI4x4.IsEnabled = MacroBlocksPartitions.SelectedIndex == 2;
            MacroBlocksPartitionsP4x4.IsEnabled = (MacroBlocksPartitions.SelectedIndex == 2) && (MacroBlocksPartitionsP8x8.IsChecked.GetValueOrDefault());
            MacroBlocksPartitionsI8x8.IsEnabled = (MacroBlocksPartitions.SelectedIndex == 2) && (MacroBlocksPartitionsAdaptiveDCT.IsChecked.GetValueOrDefault());
            MacroBlocksPartitionsP8x8.IsEnabled = MacroBlocksPartitions.SelectedIndex == 2;
            MacroBlocksPartitionsB8x8.IsEnabled = MacroBlocksPartitions.SelectedIndex == 2;

            if (MacroBlocksPartitions.SelectedIndex == 2) return;

            MacroBlocksPartitionsAdaptiveDCT.IsChecked = (MacroBlocksPartitions.SelectedIndex == 0 || MacroBlocksPartitions.SelectedIndex == 3) && Profile.AVCProfile > 1;
            MacroBlocksPartitionsI4x4.IsChecked = (MacroBlocksPartitions.SelectedIndex == 0) || (MacroBlocksPartitions.SelectedIndex == 3);
            MacroBlocksPartitionsP4x4.IsChecked = (MacroBlocksPartitions.SelectedIndex == 0);
            MacroBlocksPartitionsI8x8.IsChecked = ((MacroBlocksPartitions.SelectedIndex == 0) || (MacroBlocksPartitions.SelectedIndex == 3)) && (Profile.AVCProfile > 1);
            MacroBlocksPartitionsP8x8.IsChecked = (MacroBlocksPartitions.SelectedIndex == 0) || (MacroBlocksPartitions.SelectedIndex == 3);
            MacroBlocksPartitionsB8x8.IsChecked = (MacroBlocksPartitions.SelectedIndex == 0) || (MacroBlocksPartitions.SelectedIndex == 3);
        }

        private void MacroBlocksPartitionsCheckedChanged(object sender, RoutedEventArgs e)
        {
            MacroBlocksPartitionsP4x4.IsEnabled = (MacroBlocksPartitions.SelectedIndex == 2) && (MacroBlocksPartitionsP8x8.IsChecked.GetValueOrDefault());
            MacroBlocksPartitionsI8x8.IsEnabled = (MacroBlocksPartitions.SelectedIndex == 2) && (MacroBlocksPartitionsAdaptiveDCT.IsChecked.GetValueOrDefault());
            MacroBlocksPartitionsI8x8.IsChecked = MacroBlocksPartitionsAdaptiveDCT.IsChecked;
        }

        private void AdaptiveQuantizersModeSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            AdaptiveQuantizersStrength.IsEnabled = AdaptiveQuantizersMode.SelectedIndex > 0;
            AdaptiveQuantizersStrengthLabel.IsEnabled = AdaptiveQuantizersMode.SelectedIndex > 0;
        }

        private void TrellisSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PsyTrellisStrength.IsEnabled = Trellis.SelectedIndex > 0;
            PsyTrellisStrengthLabel.IsEnabled = Trellis.SelectedIndex > 0;

            QuantizerDeadZoneLabel.IsEnabled = Trellis.SelectedIndex == 0;
            QuantizerDeadZoneIntra.IsEnabled = Trellis.SelectedIndex == 0;
            QuantizerDeadZoneInter.IsEnabled = Trellis.SelectedIndex == 0;
        }

        private void SubPixelRefinementSelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            PsyRDStrength.IsEnabled = SubPixelRefinement.SelectedIndex > 4;
            PsyRDStrengthLabel.IsEnabled = SubPixelRefinement.SelectedIndex > 4;
            if (SubPixelRefinement.SelectedIndex >= 10)
            {
                Trellis.SelectedIndex = 2;
            }
        }

        private void NumBFramesValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsLoaded) return;

            switch (NumBFrames.Value.GetValueOrDefault())
            {
                case 0:
                    UseWeightedPred.IsChecked = false;
                    UseWeightedPred.IsEnabled = false;
                    BFrameBias.IsEnabled = false;
                    BFrameBias.Value = 0;
                    AdaptiveBFrames.IsEnabled = false;
                    AdaptiveBFrames.SelectedIndex = 1;
                    BPyramid.IsEnabled = false;
                    BPyramid.SelectedIndex = 2;
                    MVPredictionMod.IsEnabled = false;
                    MVPredictionMod.SelectedIndex = 2;
                    break;
                case 1:
                    UseWeightedPred.IsEnabled = true;
                    BFrameBias.IsEnabled = true;
                    AdaptiveBFrames.IsEnabled = true;
                    BPyramid.IsEnabled = false;
                    BPyramid.SelectedIndex = 2;
                    MVPredictionMod.IsEnabled = true;
                    break;
                default:
                    UseWeightedPred.IsEnabled = true;
                    BFrameBias.IsEnabled = true;
                    AdaptiveBFrames.IsEnabled = true;
                    BPyramid.IsEnabled = true;
                    MVPredictionMod.IsEnabled = true;
                    break;
            }
        }

        private void NumRefValueChanged(object sender, RoutedPropertyChangedEventArgs<object> e)
        {
            if (!IsLoaded) return;

            UseNoMixedReferenceFrames.IsEnabled = NumRef.Value.GetValueOrDefault() > 1;
            if (NumRef.Value.GetValueOrDefault() <= 1)
                UseNoMixedReferenceFrames.IsChecked = false;
        }
    }
}
