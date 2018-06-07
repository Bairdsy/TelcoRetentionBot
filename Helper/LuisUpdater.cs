using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

using System.Threading.Tasks;
using System.Text;
using System.Net.Http;

using Newtonsoft.Json;

using MultiDialogsBot.Helper;
using MultiDialogsBot.Database;



namespace MultiDialogsBot.Helper
{
    [Serializable]
    public class LuisUpdater
    {

        enum ESteps
        {
            Add = 0,
            Train,
            Publish
        }
        class PublishReqBody
        {
            public PublishReqBody()
            {
                versionId = "0.1";
                isStagering = false;
                region = "westus";
            }

            public string versionId { get; set; }
            public bool isStagering { get; set; }
            public string region { get; set; }
        }

        class IntentEntityStatus
        {    
            public string ModelId { get; set; }
            public StatusDetails details { get; set; }

        }

        class StatusDetails
        {
            public string statusId { get; set; }
            public string status { get; set; }
            public int exampleCount { get; set; }
            public string trainingDateTime { get; set; }
        }

        class IntentUtterance
        {
            public IntentUtterance(string intent,string utterance)
            {   
                IntentName = intent;
                Text = utterance;    
                Entities = new List<string>();
            }

            public string Text { get; set; }
            public string IntentName { get; set; }
            public List<string> Entities { get; set; }
        }


        // NOTE: Replace this example LUIS application ID with the ID of your LUIS application.
        static string appID = "0ffacaae-8314-4b1d-af4d-68718eb880f6";

        // NOTE: Replace this example LUIS application version number with the version number of your LUIS application.
        static string appVersion = "0.1";

        // NOTE: Replace this example LUIS authoring key with a valid key.
        static string authoringKey = "a3a20fb04cad4cfcaf7b821bd1eb9a19";

        static string host = "https://westus.api.cognitive.microsoft.com";
        static string path = "/luis/api/v2.0/apps/" + appID + "/versions/" + appVersion + "/";
        static string publishPath = "/luis/api/v2.0/apps/" + appID + "/publish";

        MongoDBAccess dBAccess = MongoDBAccess.Instance;


        public string debug;

        public async Task<bool> CheckTrainStatus()
        {
            bool done;

            done = await IsTrainCompleteAsync();
            return done;
        }
        public async Task<bool> UpdateUtteranceAsync(string intent,string utterance)
        {
            string jsonRep ;
            HttpResponseMessage resp1, resp2;
            IntentUtterance utterance2Add;
            Task<HttpResponseMessage> httpResponseMessage;


            debug = "I'm going to send the http requests to LUIS\r\n";

            if (dBAccess.WasSeen(intent, utterance))
            {    
                utterance2Add = new IntentUtterance(intent, utterance);
                jsonRep = JsonConvert.SerializeObject(utterance2Add);
                httpResponseMessage = HttpSendPostAsync(jsonRep, ESteps.Add);
                resp1 = await httpResponseMessage;
                if (!resp1.IsSuccessStatusCode)
                    throw new Exception("Error...could not add the utterance to LUIS, reason = " + resp1.ReasonPhrase);
                string x = await resp1.Content.ReadAsStringAsync();
                debug += "\r\nFirst Response = " + x + "\r\n" ;
                httpResponseMessage = HttpSendPostAsync(jsonRep, ESteps.Train);   /* To kick start training process */
                resp2 = await httpResponseMessage;
                x = await resp2.Content.ReadAsStringAsync();
                debug += "Response from training order\r\n" + x + "\r\n\r\n";

                if (!resp2.IsSuccessStatusCode)
                    throw new Exception("Error...could not train LUIS, reason = " + resp2.ReasonPhrase);
                /*
                 *  We will need to wait 
                 *  until LUIS is trained 
                 */
                return true;
            }
            else
            {
                debug += "nothing got sent";
                return false;
            }
        }

        public async Task<string> PublishLuisAsync()
        {
            string jsonPublishCommand =  JsonConvert.SerializeObject(new PublishReqBody()) ;
            HttpResponseMessage resp;     

            resp = await HttpSendPostAsync(jsonPublishCommand, ESteps.Publish);
            return await resp.Content.ReadAsStringAsync();
        }

        private async Task<HttpResponseMessage> HttpSendGetAsync()
        {
            StringBuilder sb = new StringBuilder(host);

            using (HttpClient client = new HttpClient())
            {
                using (HttpRequestMessage req = new HttpRequestMessage())
                {
                    sb.Append(path);
                    req.Method = HttpMethod.Get;
                    sb.Append("train");
                    req.RequestUri = new Uri(sb.ToString());
                    req.Headers.Add("Ocp-Apim-Subscription-Key", authoringKey);
                    return await client.SendAsync(req);
                }
            }
        }

        private async Task<HttpResponseMessage> HttpSendPostAsync(string jsonMessage,ESteps step)
        {
            HttpClient httpClient;
            HttpRequestMessage req;
            StringBuilder uriStrBuilder = new StringBuilder(host);

            using (httpClient = new HttpClient())
            {
                using (req = new HttpRequestMessage())
                {
                    if (ESteps.Publish == step)
                        uriStrBuilder.Append( publishPath);
                    else
                    {
                        jsonMessage = string.Concat("[", jsonMessage, "]");
                        uriStrBuilder.Append(path);
                        if (ESteps.Add == step)
                            uriStrBuilder.Append("examples");
                        else
                            uriStrBuilder.Append("train");
                    }
                    req.Method = HttpMethod.Post;
                    req.RequestUri = new Uri(uriStrBuilder.ToString());
                    req.Headers.Add("Ocp-Apim-Subscription-Key", authoringKey);
                    req.Content = new StringContent(jsonMessage,Encoding.UTF8,"text/json");

                    debug += string.Concat("URL = ", req.RequestUri.AbsoluteUri, "\r\nBody Contents = ", jsonMessage);

                    return await httpClient.SendAsync(req);
                }
            }
        }

        private async Task<bool> IsTrainCompleteAsync()
        {
            HttpResponseMessage httpResponseMessage;
            List<IntentEntityStatus> intentEntityStatuses;
            string respContents;

            httpResponseMessage = await HttpSendGetAsync();
            respContents = await httpResponseMessage.Content.ReadAsStringAsync();
            intentEntityStatuses = JsonConvert.DeserializeObject<List<IntentEntityStatus>>(respContents);
            foreach (var stateDetail in intentEntityStatuses)
                if (stateDetail.details.status != "Success")
                    return false;
            return true;
        }

    }
}