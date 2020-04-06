using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Models
{

    public interface IController : IModule
    {
        string Host { get; }
        bool Adopted { get; }
    }

    public class Controller : Module, IController
    {
        private readonly HubConnection _client;
        private string Key { get => _section["key"]; set => _section["key"] = value; }

        public string Host => _section["host"];
        public bool Adopted { get => _section.GetValue<bool>("adopted", false); set => _section.SetValue("adopted", value); }

        public Controller(string id, IConfiguration config) : base(id, config)
        {
            var url = new UriBuilder(Host);
            url.Path = "/device";
            _client = new HubConnectionBuilder()
               .WithUrl(url.Uri)
               .Build();
        }

        public override async Task Run()
        {
            await Handshake();
        }

        private async Task<bool> Handshake(int count = 0)
        {
            try
            {
                if (Key != null) return true;

                var result = await _client.InvokeAsync<string>("Inform", "Device");

                return true;
            }
            catch
            {
                if (count < 5) return await Handshake(count + 1);
                return false;
            }
        }
    }
}


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ControllerExtensions
    {
        public static IServiceCollection AddController(this IServiceCollection services)
        {
            // services.AddModuleConfig<IControllerConfiguration, ControllerConfiguration>();
            services.AddModule<IController, Controller>("controller");
            return services;
        }
    }
}
