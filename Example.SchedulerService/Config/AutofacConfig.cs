using Example.Communication;
using Example.Logging;
using Example.SchedulerService.Jobs;
using Autofac;
using Microsoft.WindowsAzure;

namespace Example.SchedulerService.Config
{
    public class AutofacConfig
    {

        public static IContainer Register()
        {
            var builder = new ContainerBuilder();

            // register the logging module
            builder.RegisterModule(new AutoFacLoggingModule());


            builder.RegisterModule(new AutoFacCommunictionsModule()
            {
                slackWebhookURL = CloudConfigurationManager.GetSetting("slackWebhookURL").ToString()
            });

            builder.RegisterModule(new AutoFacJobModule());

            return builder.Build();
        }
    }
}