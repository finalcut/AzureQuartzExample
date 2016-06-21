using System.Diagnostics;
using System.Net;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.WindowsAzure.ServiceRuntime;
using Quartz;
using Quartz.Impl;
using Example.SchedulerService.Jobs;
using System.Reflection;
using System.Collections.Generic;
using System;
using log4net;
using Example.SchedulerService.Config;
using Autofac;
using Quartz.Spi;

namespace Example.SchedulerService
{
    public class WorkerRole : RoleEntryPoint
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        private readonly ManualResetEvent runCompleteEvent = new ManualResetEvent(false);

        private IScheduler scheduler;
        private ILog Logger { get { return log4net.LogManager.GetLogger(this.GetType()); } }


        public override void Run()
        {
            Trace.TraceInformation("WorkerRole1 is running");

            try
            {
                this.RunAsync(this.cancellationTokenSource.Token).Wait();
            }
            finally
            {
                this.runCompleteEvent.Set();
            }
        }

        public override bool OnStart()
        {
            IContainer container = AutofacConfig.Register();
            ConfigureScheduler(container);
            // Set the maximum number of concurrent connections
            ServicePointManager.DefaultConnectionLimit = 12;
            return base.OnStart();
        }
        private void ConfigureScheduler(IContainer container)
        {
            scheduler = container.Resolve<IScheduler>();
            IEnumerable<Type> jobs = JobLoader.GetJobs();

            foreach(var job in jobs){
                Logger.Info(job.Name.ToString() + " configuring...");
                job.GetMethod("Configure", BindingFlags.Public | BindingFlags.Static).Invoke(null, new object[] { scheduler });
            }

            scheduler.Start();
        }

        public override void OnStop()
        {
            scheduler.Shutdown(false);
            scheduler = null;
            base.OnStop();
        }

        private async Task RunAsync(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                //just keep the job alive.
                await Task.Delay(1000);
            }
        }

    
    }
}
