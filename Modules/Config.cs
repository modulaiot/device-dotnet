
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Modules
{
    public interface IConfiguration : IModule
    {
        string DeviceId { get; set; }
        string SettingsPath { get; set; }
        string ControllerHost { get; set; }
        string ControllerKey { get; set; }
    }

    public class Configuration : Module, IConfiguration
    {
        public string DeviceId { get; set; }
        public string SettingsPath { get; set; }
        public string ControllerHost { get; set; }
        public string ControllerKey { get; set; }

        public Configuration(IModuleProvider provider, string id) : base(provider, id)
        {
            DeviceId = System.Net.Dns.GetHostName();
            SettingsPath = "settings.json";
            ControllerHost = "https://modulaiot:5000";
            ControllerKey = "modulaiot";
        }
    }
}