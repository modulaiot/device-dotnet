using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModulaIOT.Device.Models
{

    public interface IModuleProvider
    {
        T Add<T>(string id) where T : class, IModule;
        IModule Add(Type type, string id);
        IModule Add(IModule module);
        bool Has(string id);
        T Get<T>(string id) where T : class, IModule;
        IModule Get(string id);
        void Register<T>(string type) where T : class, IModule;
        IModule Build(string type, string id);
    }

    public interface IModule
    {
        string Id { get; }
        bool inUse { get; }
        Task Use();
        Task Release();
    }

    public class ModuleProvider : IModuleProvider
    {
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
        private readonly Dictionary<string, IModule> _modules = new Dictionary<string, IModule>();


        public T Add<T>(string id) where T : class, IModule
        {
            var type = typeof(T);
            var instance = Add(type, id) as T;
            if (instance == null) throw new Exception($"Failed to crate instance of \"{type}\"");
            return instance;
        }

        public IModule Add(Type type, string id)
        {
            var instance = Activator.CreateInstance(type, new object?[] { this, id }) as IModule;
            if (instance == null) throw new Exception($"Failed to crate instance of \"{type.Name}\"");
            return Add(instance);
        }

        public IModule Add(IModule module)
        {
            _modules.Add(module.Id, module);
            return module;
        }

        public bool Has(string id)
        {
            return _modules.ContainsKey(id);
        }

        public T Get<T>(string id) where T : class, IModule
        {
            var instance = Get(id) as T;
            if (instance == null) throw new Exception($"\"{id}\" can not be casted into \"{typeof(T).Name}\"");
            return instance;
        }

        public IModule Get(string id)
        {
            if (!_modules.ContainsKey(id)) throw new Exception($"\"{id}\" could not be found.");
            return _modules[id];
        }

        public void Register<T>(string type) where T : class, IModule
        {
            _types.Add(type, typeof(T));
        }

        public IModule Build(string type, string id)
        {
            return Add(_types[type], id);
        }
    }

    public class Module : IModule
    {
        private int _count;
        private readonly bool _once;
        private bool _isUsed = false;
        protected readonly IModuleProvider _provider;

        public string Id { get; }
        public bool inUse => _count > 0;

        public Module(IModuleProvider provider, string id, bool once = false)
        {
            _count = 0;
            _once = once;
            _provider = provider;
            Id = id;
        }

        public async Task Use()
        {
            if (_count == 0)
            {
                if ((_once && !_isUsed) || !_once) await OnUse(); // FIXME:
            }
            _count += 1;
        }

        public async Task Release()
        {
            _count -= 1;

            if (_count == 0)
            {
                var task = ((_once && !_isUsed) || !_once) ? OnRelease() : null; // FIXME:
                _isUsed = true;
                if (task != null) await task;
            }
        }

        protected virtual Task OnUse()
        {
            return Task.CompletedTask;
        }

        protected virtual Task OnRelease()
        {
            return Task.CompletedTask;
        }

    }
}