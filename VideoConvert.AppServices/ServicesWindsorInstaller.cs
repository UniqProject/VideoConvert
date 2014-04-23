// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ServicesWindsorInstaller.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvert.AppServices source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   Services installer for Castle.Windsor
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvert.AppServices
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Services;
    using Services.Interfaces;
    using VideoConvert.AppServices.Decoder;
    using VideoConvert.AppServices.Decoder.Interfaces;
    using VideoConvert.AppServices.Demuxer;
    using VideoConvert.AppServices.Demuxer.Interfaces;
    using VideoConvert.AppServices.Encoder;
    using VideoConvert.AppServices.Encoder.Interfaces;
    using VideoConvert.AppServices.Muxer;
    using VideoConvert.AppServices.Muxer.Interfaces;

    /// <summary>
    /// Services installer for Castle.Windsor
    /// </summary>
    public class ServicesWindsorInstaller  : IWindsorInstaller
    {

        #region Implementation of IWindsorInstaller

        /// <summary>
        ///   Performs the installation in the <see cref = "IWindsorContainer" />.
        /// </summary>
        /// <param name = "container">The container.</param>
        /// <param name = "store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IUserSettingService>().ImplementedBy<UserSettingService>());
            container.Register(Component.For<IAppConfigService>().ImplementedBy<AppConfigService>());
            container.Register(Component.For<IProcessingService>().ImplementedBy<ProcessingService>());

            // Decoder
            container.Register(Component.For<IDecoderFfmpegGetCrop>().ImplementedBy<DecoderFfmpegGetCrop>());
            container.Register(Component.For<IDecoderFfmsIndex>().ImplementedBy<DecoderFfmsIndex>());

            // Demuxer
            container.Register(Component.For<IDemuxerEac3To>().ImplementedBy<DemuxerEac3To>());
            container.Register(Component.For<IDemuxerFfmpeg>().ImplementedBy<DemuxerFfmpeg>());
            container.Register(Component.For<IDemuxerMplayer>().ImplementedBy<DemuxerMplayer>());
            container.Register(Component.For<IDemuxerMkvExtractSubtitle>().ImplementedBy<DemuxerMkvExtractSubtitle>());

            // Encoder
            container.Register(Component.For<IEncoderBdSup2Sub>().ImplementedBy<EncoderBdSup2Sub>());
            container.Register(Component.For<IEncoderFfmpegAc3>().ImplementedBy<EncoderFfmpegAc3>());
            container.Register(Component.For<IEncoderFfmpegDvd>().ImplementedBy<EncoderFfmpegDvd>());
            container.Register(Component.For<IEncoderLame>().ImplementedBy<EncoderLame>());
            container.Register(Component.For<IEncoderNeroAac>().ImplementedBy<EncoderNeroAac>());
            container.Register(Component.For<IEncoderOggEnc>().ImplementedBy<EncoderOggEnc>());
            container.Register(Component.For<IEncoderX264>().ImplementedBy<EncoderX264>());
            container.Register(Component.For<IEncoderFfmpegX264>().ImplementedBy<EncoderFfmpegX264>());

            // Muxer
            container.Register(Component.For<IFileWorker>().ImplementedBy<FileWorker>());
            container.Register(Component.For<IMuxerDvdAuthor>().ImplementedBy<MuxerDvdAuthor>());
            container.Register(Component.For<IMuxerMkvMerge>().ImplementedBy<MuxerMkvMerge>());
            container.Register(Component.For<IMuxerMp4Box>().ImplementedBy<MuxerMp4Box>());
            container.Register(Component.For<IMuxerMplex>().ImplementedBy<MuxerMplex>());
            container.Register(Component.For<IMuxerSpuMux>().ImplementedBy<MuxerSpuMux>());
            container.Register(Component.For<IMuxerTsMuxeR>().ImplementedBy<MuxerTsMuxeR>());

            // Queue Handler
            container.Register(Component.For<IQueueProcessor>().ImplementedBy<QueueProcessor>());
        }

        #endregion
    }
}