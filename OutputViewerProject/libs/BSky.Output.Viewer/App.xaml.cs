using BSky.ConfigService.Services;
using BSky.ConfService.Intf.Interfaces;
using BSky.Lifetime;
using BSky.Lifetime.Interfaces;
using BSky.Lifetime.Services;
using Microsoft.Practices.Unity;
using System.Windows;

namespace BSky.Output.Viewer
{

    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        UnityContainer container = new UnityContainer();
        public App()
        {
            LifetimeService.Instance.Container = container;
            container.RegisterInstance<ILoggerService>(new LoggerService());/// For Application log. Starts with default level "Error"
            container.RegisterInstance<IConfigService>(new BSky.ConfigService.Services.ConfigService());//For App Config file
            container.RegisterInstance<IAdvancedLoggingService>(new AdvancedLoggingService());//For Advanced Logging
        }
    }
}
