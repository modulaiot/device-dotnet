using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Configuration;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device
{
    public class DeviceBuilder
    {

        private ServiceCollection _services;

        public DeviceBuilder()
        {
            _services = new ServiceCollection();
        }

        public IDeviceModule Build()
        {
            _services.AddSingleton<IDeviceModule, DeviceModule>();

            return _services
                .BuildServiceProvider()
                .GetService<IDeviceModule>();
        }
        public DeviceBuilder ConfigureDefaults()
        {
            _services
                .AddLogging(builder =>
                {
                    builder.AddConsole();
                })
                .AddConfiguration(builder =>
                {
                    builder
                        .AddDefaultConfiguration()
                        .AddWriteableJsonFile("settings.json");
                })
                .AddModuleLifetime()
                .AddController();
            // .AddTemperature(builder =>
            // {
            //     builder.AddBme280();
            // });
            // .AddReporter(reporterBuilder =>
            // {
            //     reporterBuilder.AddHomeassistant();
            // })

            return this;
        }


    }


    public interface IDeviceModule
    {
        string Id { get; }
        string Name { get; }
        Task Run();
    }



    public class DeviceModule : IDeviceModule
    {
        private readonly IConfiguration _config;
        private readonly IModuleLifetime _lifetime;

        public string Id => _config["id"];
        public string Name => _config["name"];

        public DeviceModule(IConfiguration config, IModuleLifetime lifetime)
        {
            _config = config;
            _lifetime = lifetime;
        }

        public Task Run()
        {
            return _lifetime.Run();
        }
    }
}