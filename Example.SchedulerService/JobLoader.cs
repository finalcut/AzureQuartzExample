using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Example.SchedulerService
{
    public class JobLoader
    {

        public static IEnumerable<Type> GetJobs()
        {
            string nameSpace = "Example.SchedulerService.Jobs";
            return Assembly.GetExecutingAssembly().GetTypes().Where(t => 
                   String.Equals(t.Namespace, nameSpace, StringComparison.Ordinal)
                   && t.GetInterface("IJob") == typeof(IJob)
                   && t.BaseType == typeof(BaseJob)
                   && !t.IsInterface
                   ).OrderBy(t => t.Name);
        }
 

    }
}
