using Example.Communication.Slack;
using Autofac;

namespace Example.Communication
{
    public class AutoFacCommunictionsModule : Module
    {
        public string slackWebhookURL { get; set; }

        public AutoFacCommunictionsModule()
        {
        }

        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            var slackClient = new SlackClient(slackWebhookURL);
            builder.RegisterInstance(slackClient).As<SlackClient>();

        }
    }
}
