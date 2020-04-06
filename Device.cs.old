using System;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ModulaIOT.Device.Models;
using ModulaIOT.Device.Modules;


namespace ModulaIOT.Device
{
    public class DeviceBuilder
    {
        private ModuleProvider _provider;
        private Action<IModuleProvider>? _with;
        private Action<IConfiguration>? _withConfiguration;

        public DeviceBuilder()
        {
            _provider = new ModuleProvider();
        }

        public ModulaIOTDevice Build()
        {
            _provider.Register<Controller>("ControllerModule");

            _provider.Add<Configuration>("Configuration");
            _provider.Add<FileSettings>("FileSettings");
            _provider.Add<Loader>("Loader");

            if (_with != null)
            {
                _with(_provider);
            }
            if (_withConfiguration != null)
            {
                _withConfiguration(_provider.Get<IConfiguration>("Configuration"));
            }

            return new ModulaIOTDevice(_provider);
        }

        public void With(Action<IModuleProvider> fn)
        {
            _with = fn;
        }
        public void WithConfiguration(Action<IConfiguration> fn)
        {
            _withConfiguration = fn;
        }
    }

    public class ModulaIOTDevice
    {
        private readonly IModuleProvider _provider;
        private readonly IModule _loader;

        public ModulaIOTDevice(IModuleProvider provider)
        {
            _provider = provider;
            _loader = _provider.Get("Loader");
        }

        public async Task Run()
        {
            await _loader.Use();
            var controller = _provider.Get<IController>("Controller");
            await controller.Use();

            await controller.Release();
            await _loader.Release();
        }
    }
}
