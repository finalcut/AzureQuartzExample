using Newtonsoft.Json;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Text;

//A simple C# class to post messages to a Slack channel
//Note: This class uses the Newtonsoft Json.NET serializer available via NuGet
namespace Example.Communication.Slack
{
    public class SlackClient
    {
        private readonly Uri _uri;
        private readonly Encoding _encoding = new UTF8Encoding();


        public SlackClient(string slackWebhookURL)
        {
            _uri = new Uri(slackWebhookURL);
        }

        private bool isProperlyInitialized()
        {
            return _uri.AbsoluteUri.Length > 0;
        }

        //Post a message using simple strings
        public void PostMessage(string text, string username = null, string channel = null)
        {
            if (isProperlyInitialized())
            {
                Payload payload = new Payload()
                {
                    Channel = channel,
                    Username = username,
                    Text = text
                };

                PostMessage(payload);
            }
        }

        //Post a message using a Payload object
        public void PostMessage(Payload payload)
        {
            if (isProperlyInitialized())
            {
                string payloadJson = JsonConvert.SerializeObject(payload);

                using (WebClient client = new WebClient())
                {
                    NameValueCollection data = new NameValueCollection();
                    data["payload"] = payloadJson;

                    var response = client.UploadValues(_uri, "POST", data);

                    //The response text is usually "ok"
                    string responseText = _encoding.GetString(response);
                }
            }
        }
    }

    //This class serializes into the Json payload required by Slack Incoming WebHooks
    public class Payload
    {
        [JsonProperty("channel")]
        public string Channel { get; set; }

        [JsonProperty("username")]
        public string Username { get; set; }

        [JsonProperty("text")]
        public string Text { get; set; }
    }
}