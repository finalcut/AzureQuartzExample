using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Configuration;
using Newtonsoft.Json.Linq;
using log4net;
using Quartz;
using Microsoft.WindowsAzure;
using Example.Communication.Slack;
using System.Net.Sockets;

namespace Example.SchedulerService
{
    public abstract class BaseJob
    {

        internal string dateTimeFormat = "dd MMM yyyy - HH:mm:ss";
        internal const bool useSlack = true;

        internal abstract string JobName{ get; }
        public static void Configure(IScheduler sch){
            throw new Exception("This must be overriden by the extending Job class");
        }

        public ILog Logger { get; set; }
        public SlackClient SlackClient { get; set; }


        /// <summary>
        /// gets the name of the class that inheirited this class.
        /// </summary>
        internal static string ClassName
        {
            get
            {
                string name = null;
                if (String.IsNullOrEmpty(name))
                {
                    var stack = new System.Diagnostics.StackTrace();

                    if (stack.FrameCount < 2)
                        return "";

                    name = (stack.GetFrame(1).GetMethod() as System.Reflection.MethodInfo).DeclaringType.Name;
                    if (name == "BaseJob" && stack.FrameCount >= 3)
                    {
                        name = (stack.GetFrame(2).GetMethod() as System.Reflection.MethodInfo).DeclaringType.Name;
                    }
                }
                return name;
            }
        }

        internal void StartJob()
        {
           //maybe log the start of the job...
            
        }


        public void EndJob(string result)
        {

            // maybe log the end of the job and any results..this shows us posting to slack

            try
            {
                var slackMessage = DateTime.Now.ToString(dateTimeFormat) + " " + JobName + "\n" + result;
                var log = Logger;
                Logger.Info(slackMessage);

                if (useSlack)
                {
                    SlackClient.PostMessage(
                       username: "EXAMPLE CLOUD SCHEDULER : " + GetLocalIPAddress(),
                       text: slackMessage,
                       channel: "#exampleScheduler");
                }
                if (result.Length > 0)
                {
                    System.Diagnostics.Debug.WriteLine(result);
                }
            }
            catch (Exception e)
            {
                // problem posting to slack.. 
                Logger.Error(e);
            }


        }


        private byte[] ConvertToData(Dictionary<string, object> values)
        {
            var x = JObject.FromObject(values);
            return Encoding.UTF8.GetBytes(x.ToString());
        }

        internal Dictionary<string, string> PostRequest(string uri)
        {
            return PostRequest(uri, new Dictionary<string, object>());
        }

        internal Dictionary<string, string> PostRequest(string uri, Dictionary<string, object> values)
        {
            Dictionary<string, string> results = new Dictionary<string, string>();

            using (WebClient client = new WebClient())
            {
                client.Headers.Add("JobService", CloudConfigurationManager.GetSetting("jobServiceKey").ToString());
                client.Headers.Add("Content-Type", "application/json");

                var address = CloudConfigurationManager.GetSetting("apiBaseUrl").ToString() + uri;

                byte[] response = client.UploadData(address, "POST", ConvertToData(values));

                string result = System.Text.Encoding.Default.GetString(response);

                JObject json = JObject.Parse(result);

                foreach (JProperty prop in json.Properties())
                {
                    results.Add(prop.Name, prop.Value.ToString());
                }
            }


            return results;
        }

        private static string GetLocalIPAddress()
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            foreach (var ip in host.AddressList)
            {
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                {
                    return ip.ToString();
                }
            }
            throw new Exception("Local IP Address Not Found!");
        }

    }
}
