using Autofac.Core;
using log4net;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Example.Logging
{
    public class AutoFacLoggingModule : Autofac.Module
    {
        private static Dictionary<System.Type, ILog> loggers = new Dictionary<System.Type, ILog>();
        private static void InjectLoggerProperties(object instance)
        {
            var instanceType = instance.GetType();

            // Get all the injectable properties to set.
            // If you wanted to ensure the properties were only UNSET properties,
            // here's where you'd do it.
            var properties = instanceType
              .GetProperties(BindingFlags.Public | BindingFlags.Instance)
              .Where(p => p.PropertyType == typeof(ILog) && p.CanWrite && p.GetIndexParameters().Length == 0);

            // Set the properties located.
            foreach (var propToSet in properties)
            {
                if (!loggers.ContainsKey(instanceType))
                {
                    loggers.Add(instanceType, LogManager.GetLogger(instanceType));
                }
                ILog logger = null;
                loggers.TryGetValue(instanceType, out logger);
                propToSet.SetValue(instance, logger, null);
            }
        }

        private static void OnComponentPreparing(object sender, PreparingEventArgs e)
        {
            var t = e.Component.Activator.LimitType;
            e.Parameters = e.Parameters.Union(
              new[] {
                new ResolvedParameter((p, i) => p.ParameterType == typeof(ILog), (p, i) => LogManager.GetLogger(t)),
              }
            );
        }

        protected override void AttachToComponentRegistration(IComponentRegistry componentRegistry, IComponentRegistration registration)
        {
            // Handle constructor parameters.
            registration.Preparing += OnComponentPreparing;

            // Handle properties.
            registration.Activated += (sender, e) => InjectLoggerProperties(e.Instance);
        }
    }
}
