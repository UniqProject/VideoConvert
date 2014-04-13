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
        }

        #endregion
    }
}