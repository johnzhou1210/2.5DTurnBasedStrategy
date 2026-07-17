using System;
using System.Collections.Generic;

namespace StrategyGame.Utils {
    public static class ServiceLocator {
        private static readonly Dictionary<Type, object> Services = new();
        public static void Register<T>(T service) {
            Services[typeof(T)] = service;
        }
        public static T Get<T>() {
            if (Services.TryGetValue(typeof(T), out var service))
                return (T)service;
            throw new Exception($"ServiceLocator.Get: Service {typeof(T).Name} has not been registered.");
        }
        public static bool TryGet<T>(out T service) {
            if (Services.TryGetValue(typeof(T), out var obj)) {
                service = (T)obj;
                return true;
            }
            service = default;
            return false;
        }
        public static void Unregister<T>() {
            Services.Remove(typeof(T));
        }
    }
}
