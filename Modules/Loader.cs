using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Modules
{
    public interface ILoader : IModule
    {

    }

    public class Loader : Module, ILoader
    {
        private readonly IFileSettings _settings;

        public Loader(IModuleProvider provider, string id) : base(provider, id, true)
        {
            _settings = _provider.Get<IFileSettings>("FileSettings");
        }

        protected override async Task OnUse()
        {
            await _settings.Use();
            foreach (var id in _settings.Keys())
            {
                var settings = _settings.Get<Dictionary<string, object>>(id);
                var type = settings.GetValueOrDefault("Type") as string;
                if (type != null)
                {
                    _provider.Build(type, id);
                }
            }
        }

        protected async override Task OnRelease()
        {
            await _settings.Release();
        }
    }
}