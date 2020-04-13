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
        bool Connected { get; }
    }

    public class Controller : Module, IController
    {
        private readonly HubConnection _client;
        private string Key
        {
            get => _section["key"];
            set => _section["key"] = value;
        }

        public string Host => _section["host"];
        public bool Adopted
        {
            get => _section.GetValue("adopted", false);
            set => _section.SetValue("adopted", value);
        }
        public bool Connected => _client.State == HubConnectionState.Connected;

        public Controller(string id, IConfiguration config) : base(id, config)
        {
            var url = new UriBuilder(Host);
            url.Path = "/device";
            _client = new HubConnectionBuilder()
               .WithUrl(url.Uri)
               .Build();
        }

        public override async Task Run(IModuleLifetime lifetime)
        {
            await _client.StartAsync();
            await Handshake();
            await WaitUntilUnused();
            await _client.StopAsync();
        }

        private async Task<bool> Handshake(int count = 0)
        {
            if (Adopted) return true;

            var result = await _client.InvokeAsync<string>("Inform", "Device");

            return true;
        }
    }
}


namespace Microsoft.Extensions.DependencyInjection
{
    public static class ControllerExtensions
    {
        public static IServiceCollection AddController(this IServiceCollection services)
        {
            services.AddModule<IController, Controller>("controller");
            return services;
        }
    }
}
