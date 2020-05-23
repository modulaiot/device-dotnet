using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ModulaIOT.Device.Models
{
    public interface IModuleSettings
    {
        T Get<T>(string key);
        void Set<T>(string key, T value);
    }

    public interface IModuleCapability
    {
        int Min { get; }
        int Max { get; }
        IList<string> Skills { get; }
        Task Load();
    }

    public interface IModule
    {
        string Name { get; }
        IModuleSettings Settings { get; }
    }

    public interface IService : IModule
    {
        Task Start();
        Task Stop();
    }

    public interface ILifetime : IModule
    {
        Task Run();
    }

    public interface ISensor<T> : ILifetime
    {
        T Value { get; }
        void SetValue(T Value);
    }

    public interface ISwitch : ISensor<Boolean>
    {
        void On();
        void Off();
        void Toggle();
    }
}