using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.FileExtensions;
using Microsoft.Extensions.Configuration.Json;
using ModulaIOT.Device.Models;
using ModulaIOT.Device.Modules;

using System.Net.Http;

namespace ModulaIOT.Device
{
    public class ModulaIOTDeviceBuilder
    {
        private Action<IServiceCollection> ConfigureServicesFunc;

        private string ConfigPath { get; }


        public ModulaIOTDeviceBuilder(string configPath = "settings.json")
        {
            ConfigPath = configPath;


            // ModuleSettings.Register<CoreSettings>("CoreSettings");

            // ServiceProvider.GetServices()

            // var settings = ServiceProvider.GetService<ISettings>();
            // var coreConfig = settings.Get<CoreSettings>("Test");
            // Console.WriteLine("CoreConfig: " + coreConfig.TestParam);
            // this.configPath = configPath;
        }

        public ModulaIOTDevice Build()
        {
            // Register Services
            var services = new ServiceCollection()
                .AddSingleton<IConfiguration>(new ConfigurationBuilder()
                    .AddJsonFile(ConfigPath, true, true)
                    .Build()
                )
                .AddSingleton<ISettings, Settings>();

            // Register Main Modules
            services
                .UseModule<CoreSettings>();

            // Register User Services and Modules
            if (ConfigureServicesFunc != null) ConfigureServicesFunc(services);

            // Register Device
            services.AddSingleton<ModulaIOTDevice>();

            // Build Provider
            var provider = services.BuildServiceProvider();

            // Load Settings
            provider.GetService<ISettings>().Load();

            // Retrun Device Instance
            return provider.GetService<ModulaIOTDevice>();
        }

        public ModulaIOTDeviceBuilder ConfigureServices(Action<IServiceCollection> func)
        {
            ConfigureServicesFunc = func;
            return this;
        }


    }

    public class ModulaIOTDevice
    {
        public ISettings Settings { get; }

        public ModulaIOTDevice(ISettings settings)
        {
            Settings = settings;
            var core = settings.Get<CoreSettings>("Test");
            Console.WriteLine("Core: " + core.TestParam);
        }

        public async Task Run()
        {
            var client = new HttpClient();
            var res = await client.GetAsync("http://localhost:5000/api/device/get");
            Console.WriteLine(await res.Content.ReadAsStringAsync());
        }

    }
}
