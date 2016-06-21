using Quartz;
using Example.SchedulerService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Example.SchedulerService.Jobs
{
    public class EmailQueueProcessor : BaseJob, IJob
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

            string key = "totalMessages";
            string msg = "";

            try
            {
                var results = PostRequest("email/send");

                if (results.ContainsKey(key))
                {
                    int c;
                    if (Int32.TryParse(results[key], out c))
                    {
                        msg = "Results: (" + c + " Total | " +
                            results["failedMessages"] + " Failed | " +
                            results["successfulMessages"] + " Sent)";
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
            IJobDetail job = JobBuilder.Create<EmailQueueProcessor>()
                .WithIdentity("EmailQueueProcessor", "admin")
                .RequestRecovery(true)
                .Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithIdentity("EmailQueueProcessorTrigger", "admin")
                .WithSimpleSchedule(x => x
                    .WithIntervalInMinutes(10)
                    .RepeatForever())
                 .Build();

           sch.ScheduleJob(job, trigger);
        }

    }
}
