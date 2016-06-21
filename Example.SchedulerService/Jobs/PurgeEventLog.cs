using Quartz;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Example.SchedulerService;

namespace Example.SchedulerService.Jobs
{
    public class PurgeEventLog : BaseJob, IJob
    {
        internal override string JobName
        {
            get
            {
                return ClassName;
            }
        }


        public void Execute(IJobExecutionContext context)
        {
            StartJob();
            
            string msg = "";
            string key = "eventLogEntriesPurgedCount";

            try
            {
                var results = PostRequest("eventlog/purge");
                if (results.ContainsKey(key))
                {
                    int c;
                    if (Int32.TryParse(results[key], out c))
                    {
                        msg = "Results: (" + c.ToString() + " Purged)";
                    }
                }
            }
            catch (Exception e)
            {
                msg = e.ToString();
            }
            EndJob(msg);
        }


        public new static void Configure(IScheduler sch)
        {
            IJobDetail job = JobBuilder.Create<PurgeEventLog>()
                .WithIdentity("PurgeEventLog", "admin")
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("PurgeEventLogTrigger", "admin")
                .WithSimpleSchedule(x => x
                //.WithIntervalInSeconds(10)
                .WithInterval(new TimeSpan(1, 0, 0, 0, 0)) //one day
                .RepeatForever())
                .StartAt(new DateTimeOffset(2016, 1, 1, 22, 0, 0, new TimeSpan(0, 0, 0))) // starting at 10pm
                .Build();

            sch.ScheduleJob(job, trigger);

        }
    }
}
