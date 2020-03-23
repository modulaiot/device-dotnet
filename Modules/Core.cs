using System;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Modules
{
    public interface ICoreSettings : IModule
    {
        string TestParam { get; set; }
    }
    public class CoreSettings : Module, ICoreSettings
    {
        public string TestParam { get => Settings["TestParam"] as string; set => Settings["TestParam"] = value; }

        public CoreSettings(ISettings settings)
        {

        }
    }
}