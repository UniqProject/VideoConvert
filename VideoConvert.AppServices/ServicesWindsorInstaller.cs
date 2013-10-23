
namespace VideoConvert.AppServices
{
    using Castle.MicroKernel.Registration;
    using Castle.MicroKernel.SubSystems.Configuration;
    using Castle.Windsor;
    using Services;
    using Services.Interfaces;

    public class ServicesWindsorInstaller  : IWindsorInstaller
    {
        #region Implementation of IWindsorInstaller

        /// <summary>
        /// Performs the installation in the <see cref="T:Castle.Windsor.IWindsorContainer"/>.
        /// </summary>
        /// <param name="container">The container.</param><param name="store">The configuration store.</param>
        public void Install(IWindsorContainer container, IConfigurationStore store)
        {
            container.Register(Component.For<IUserSettingService>().ImplementedBy<UserSettingService>());
            container.Register(Component.For<IAppConfigService>().ImplementedBy<AppConfigService>());
            container.Register(Component.For<IProcessingService>().ImplementedBy<ProcessingService>());
        }

        #endregion
    }
}