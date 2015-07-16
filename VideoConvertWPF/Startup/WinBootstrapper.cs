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
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using Caliburn.Metro;
    using Caliburn.Micro;
    using Castle.Core;
    using Castle.Core.Internal;
    using Castle.MicroKernel.Registration;
    using Castle.Windsor;
    using VideoConvert.AppServices;
    using VideoConvertWPF.ViewModels;
    using VideoConvertWPF.ViewModels.Interfaces;

    public class WinBootstrapper : CaliburnMetroCompositionBootstrapper<IShellViewModel>
    {
        private IWindsorContainer _winContainer;

        protected override void Configure()
        {
            _winContainer = new WindsorContainer();
            
            _winContainer.Register(Component.For<IWindowManager>().ImplementedBy<AppWindowManager>());
            _winContainer.Register(Component.For<IEventAggregator>().ImplementedBy<EventAggregator>());

            // Initialise the ApplicationServices IWindsorInstaller
            _winContainer.Register(Component.For<IWindsorInstaller>().ImplementedBy<ServicesWindsorInstaller>());
            _winContainer.Install(_winContainer.ResolveAll<IWindsorInstaller>());

            // Views
            _winContainer.Register(
                Component.For<IMainViewModel>().ImplementedBy<MainViewModel>().LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
                Component.For<IShellViewModel>().ImplementedBy<ShellViewModel>().LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
                Component.For<IStreamSelectViewModel>()
                    .ImplementedBy<StreamSelectViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
                Component.For<IOptionsViewModel>()
                    .ImplementedBy<OptionsViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
                Component.For<IChangeLogViewModel>()
                    .ImplementedBy<ChangeLogViewModel>()
                    .LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
                Component.For<IEncodeViewModel>()
                .ImplementedBy<EncodeViewModel>()
                .LifeStyle.Is(LifestyleType.Singleton));

            _winContainer.Register(
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
            return string.IsNullOrWhiteSpace(key) ? _winContainer.Resolve(service) : _winContainer.Resolve<object>(key, new { });
            
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
            return _winContainer.ResolveAll(service).Cast<object>();
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
                .Where(property => _winContainer.Kernel.HasComponent(property.PropertyType))
                .ForEach(property => property.SetValue(instance, _winContainer.Resolve(property.PropertyType), null));
        }
    }
}