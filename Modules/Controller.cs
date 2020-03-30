using System;
using System.Threading.Tasks;
using System.Net.Http;
using ModulaIOT.Device.Models;
using Microsoft.AspNetCore.SignalR.Client;


namespace ModulaIOT.Device.Modules
{
    public interface IController : IModule
    {
        string Host { get; }
        string Key { get; }
        bool Adopted { get; }
    }

    public class Controller : ModuleSettings, IController
    {
        private readonly HubConnection _client;
        private readonly IConfiguration _config;
        public string Host { get => GetWithDefault<string>(_config.ControllerHost); private set => Set(value); } // fix with defaults ?
        public string Key { get => GetWithDefault<string>(_config.ControllerKey); private set => Set(value); }
        public bool Adopted { get => Get<bool>(); private set => Set(value); }


        public Controller(IModuleProvider provider, string id) : base(provider, id)
        {
            _config = _provider.Get<IConfiguration>("Configuration");
            var url = new UriBuilder(Host);
            url.Path = "/device";
            _client = new HubConnectionBuilder()
               .WithUrl(url.Uri)
               .Build();
        }

        protected override async Task OnUse()
        {
            await base.OnUse();
            await _config.Use();
            await _client.StartAsync();
            var result = await _client.InvokeAsync<string>("Inform", "Device");
            if (!await Handshake()) throw new Exception("Could not handshake with server.");
        }

        protected override async Task OnRelease()
        {
            await _client.StopAsync();
            await _config.Release();
            await base.OnRelease();
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

    public static class InformApi
    {
        public class Request
        {
            public string? Id { get; set; }
        }
        public class Response
        {
            public string? Key { get; set; }
        }
    }
}