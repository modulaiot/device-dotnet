using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulaIOT.Device.Models;
using ModulaIOT.Device.Modules;

using System.Net.Http;

namespace ModulaIOT.Device
{
    public interface IDeviceConfiguration
    {
        string SettingsPath { get; set; }
    }

    public class DeviceConfiguration : IDeviceConfiguration
    {
        public string SettingsPath { get; set; }
    }

    public class ModulaIOTDeviceBuilder
    {
        private Action<IServiceCollection> _configureServices;

        private Action<IDeviceConfiguration> _configureConfig;

        private readonly DeviceConfiguration _config = new DeviceConfiguration
        {
            SettingsPath = "settings.json"
        };

        public ModulaIOTDeviceBuilder()
        {
        }

        public ModulaIOTDevice Build()
        {
            // User Config
            if (_configureConfig != null) _configureConfig(_config);

            // Register Services
            var services = new ServiceCollection()
                .AddLogging(builder =>
                {
                    builder
                        .AddConsole();
                })
                .AddSingleton<IDeviceConfiguration>(_config)
                .AddSingleton<ISettings, Settings>()
                .AddSingleton<IModules, Models.Modules>();

            // Register Main Modules
            services
                .UseModule<ICoreSettings, CoreSettings>(singleton: true);

            // Register User Services and Modules
            if (_configureServices != null) _configureServices(services);

            // Register Device
            services.AddSingleton<ModulaIOTDevice>();

            // Build Provider
            var provider = services.BuildServiceProvider();

            // Retrun Device Instance
            return provider.GetService<ModulaIOTDevice>();
        }

        public ModulaIOTDeviceBuilder ConfigureServices(Action<IServiceCollection> func)
        {
            _configureServices = func;
            return this;
        }

        public ModulaIOTDeviceBuilder ConfigureConfig(Action<IDeviceConfiguration> func)
        {
            _configureConfig = func;
            return this;
        }
    }

    public class ModulaIOTDevice : ILoadable, IAsyncDisposable
    {
        public IModules Modules { get; }
        public IServiceProvider Provider { get; }

        public ModulaIOTDevice(IModules modules, ICoreSettings coreSettings, IServiceProvider provider)
        {

            Modules = modules;
            Provider = provider;
        }

        public async Task Load()
        {
            // Load Modules
            await Modules.Load();


            var client = new HttpClient();
            var res = await client.GetAsync("http://localhost:5000/api/device/get");
            Console.WriteLine(await res.Content.ReadAsStringAsync());

        }

        public async Task Save()
        {
            // Save Modules
            await Modules.Save();
        }

        public async ValueTask DisposeAsync()
        {
            await Save();
        }
    }
}
