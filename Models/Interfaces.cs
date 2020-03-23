using System;
using System.Threading.Tasks;

namespace ModulaIOT.Device.Models
{
    public interface ILoadable
    {
        Task Load();
        Task Save();
    }
}