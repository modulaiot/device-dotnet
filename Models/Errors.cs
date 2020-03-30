using System;
using Microsoft.Extensions.Logging;

namespace ModulaIOT.Device.Models
{
    public static class EventIds
    {
        public static EventId ModuleLoadError = new EventId(20, "Module Load Error");
    }
}