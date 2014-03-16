// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WinBootstrapper.cs" company="JT-Soft (https://github.com/UniqProject/VideoConvert)">
//   This file is part of the VideoConvertWPF source code - It may be used under the terms of the GNU General Public License.
// </copyright>
// <summary>
//   
// </summary>
// --------------------------------------------------------------------------------------------------------------------

namespace VideoConvertWPF.Startup
{
    using Caliburn.Metro;
    using Caliburn.Micro;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using VideoConvert.AppServices;
    using VideoConvert.AppServices.Decoder;
    using VideoConvert.AppServices.Decoder.Interfaces;
    using VideoConvert.AppServices.Demuxer;
    using VideoConvert.AppServices.Demuxer.Interfaces;
    using VideoConvert.AppServices.Encoder;
    using VideoConvert.AppServices.Encoder.Interfaces;
    using VideoConvert.AppServices.Muxer;
    using VideoConvert.AppServices.Muxer.Interfaces;
    using VideoConvert.AppServices.Services;
    using VideoConvert.AppServices.Services.Interfaces;
    using ViewModels;
    using ViewModels.Interfaces;

    public class WinBootstrapper : CaliburnMetroCompositionBootstrapper<IShellViewModel>
    {
        private IWindsorContainer _winContainer;

        protected override void Configure()
        {
            this._winContainer = new WindsorContainer();
            
            this._winContainer.Register(Component.For<IWindowManager>().ImplementedBy<AppWindowManager>());
            this._winContainer.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>());

            // Initialise the ApplicationServices IWindsorInstaller
            this._winContainer.Register(Component.For<IWindsorInstaller>().ImplementedBy<ServicesWindsorInstaller>());
            this._winContainer.Install(_winContainer.ResolveAll<IWindsorInstaller>());

            // Decoders
            this._winContainer.Register(
                Component.For<IDecoderFfmpegGetCrop>()
                    .ImplementedBy<DecoderFfmpegGetCrop>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IDecoderFfmsIndex>()
                    .ImplementedBy<DecoderFfmsIndex>()
                    .LifeStyle.Is(LifestyleType.Singleton));


            // Demuxers
            this._winContainer.Register(
                Component.For<IDemuxerEac3To>().ImplementedBy<DemuxerEac3To>().LifeStyle.Is(LifestyleType.Singleton));
            
            this._winContainer.Register(
                Component.For<IDemuxerFfmpeg>().ImplementedBy<DemuxerFfmpeg>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IDemuxerMplayer>().ImplementedBy<DemuxerMplayer>().LifeStyle.Is(LifestyleType.Singleton));
            

            // Encoders
            this._winContainer.Register(
                Component.For<IEncoderBdSup2Sub>()
                    .ImplementedBy<EncoderBdSup2Sub>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderFfmpegAc3>()
                    .ImplementedBy<EncoderFfmpegAc3>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderFfmpegDvd>()
                    .ImplementedBy<EncoderFfmpegDvd>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderLame>().ImplementedBy<EncoderLame>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderNeroAac>().ImplementedBy<EncoderNeroAac>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderOggEnc>().ImplementedBy<EncoderOggEnc>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderX264>().ImplementedBy<EncoderX264>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncoderFfmpegX264>()
                    .ImplementedBy<EncoderFfmpegX264>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            // Muxers
            this._winContainer.Register(
                Component.For<IFileWorker>().ImplementedBy<FileWorker>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IMuxerDvdAuthor>().ImplementedBy<MuxerDvdAuthor>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IMuxerMkvMerge>().ImplementedBy<MuxerMkvMerge>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IMuxerMp4Box>().ImplementedBy<MuxerMp4Box>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IMuxerMplex>().ImplementedBy<MuxerMplex>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IMuxerSpuMux>().ImplementedBy<MuxerSpuMux>().LifeStyle.Is(LifestyleType.Singleton));

            // Queue Handler
            this._winContainer.Register(
                Component.For<IQueueProcessor>().ImplementedBy<QueueProcessor>().LifeStyle.Is(LifestyleType.Singleton));

            // Views
            this._winContainer.Register(
                Component.For<IMainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IShellViewModel>().ImplementedBy<ShellViewModel>().LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IStreamSelectViewModel>()
                    .ImplementedBy<StreamSelectViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IOptionsViewModel>()
                    .ImplementedBy<OptionsViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IChangeLogViewModel>()
                    .ImplementedBy<ChangeLogViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IEncodeViewModel>()
                .ImplementedBy<EncodeViewModel>()
                .LifeStyle.Is(LifestyleType.Singleton));

            this._winContainer.Register(
                Component.For<IAboutViewModel>().ImplementedBy<AboutViewModel>().LifeStyle.Is(LifestyleType.Singleton));
        }

        protected override IEnumerable<Assembly> SelectAssemblies()
        {
            return AppDomain.CurrentDomain.GetAssemblies();
        }

        /// <summary>
        /// Get an Instance of a service
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <param name="key">
        /// The key.
        /// </param>
        /// <returns>
        /// The Service Requested
        /// </returns>
        protected override object GetInstance(Type service, string key)
        {
            return string.IsNullOrWhiteSpace(key) ? this._winContainer.Resolve(service) : this._winContainer.Resolve<object>(key, new { });
            
            //string contract = string.IsNullOrEmpty(key) ? AttributedModelServices.GetContractName(service) : key;

            //var exports = _container.GetExportedValues<object>(contract);
            //return exports.FirstOrDefault();
        }

        /// <summary>
        /// Get all instances of a service
        /// </summary>
        /// <param name="service">
        /// The service.
        /// </param>
        /// <returns>
        /// A collection of instances of the requested service type.
        /// </returns>
        protected override IEnumerable<object> GetAllInstances(Type service)
        {
            return this._winContainer.ResolveAll(service).Cast<object>();
        }

        /// <summary>
        /// Build Up
        /// </summary>
        /// <param name="instance">
        /// The instance.
        /// </param>
        protected override void BuildUp(object instance)
        {
            instance.GetType().GetProperties()
                .Where(property => property.CanWrite && property.PropertyType.IsPublic)
                .Where(property => this._winContainer.Kernel.HasComponent(property.PropertyType))
                .ForEach(property => property.SetValue(instance, this._winContainer.Resolve(property.PropertyType), null));
        }
    }
}