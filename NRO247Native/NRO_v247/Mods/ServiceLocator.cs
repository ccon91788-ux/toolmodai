using System;
using System.Collections.Generic;

namespace NRO_v247.Mods
{
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> _services = new Dictionary<Type, object>();

        public static void Register<T>(T service)
        {
            if (service == null) return;
            _services[typeof(T)] = service;
        }

        public static void Unregister<T>()
        {
            if (_services.ContainsKey(typeof(T)))
            {
                _services.Remove(typeof(T));
            }
        }

        public static T Get<T>()
        {
            if (_services.TryGetValue(typeof(T), out object service))
            {
                return (T)service;
            }
            return default(T); // Returns null for reference types if not found
        }
    }
}
