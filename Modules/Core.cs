using System;
using System.Threading.Tasks;
using ModulaIOT.Device.Models;

namespace ModulaIOT.Device.Modules
{
    public interface ICoreModule : IModule
    {
        string TestParam { get; set; }
    }
    public class CoreSettings : IModule, ICoreModule
    {
        public string TestParam { get; set; }
        public string Id { get; set; }
        public string Name { get; set; }

        public CoreSettings(ISettings settings)
        {

        }

        public Task Run()
        {
            throw new NotImplementedException();
        }
    }
}