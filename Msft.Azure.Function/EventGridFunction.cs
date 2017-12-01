using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Msft.Event.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Msft.Azure.Function
{
    public static class EventGridFunction
    {
        //https://blogs.msdn.microsoft.com/brandonh/2017/11/30/locally-debugging-an-azure-function-triggered-by-azure-event-grid/
        //https://docs.microsoft.com/en-us/azure/event-grid/security-authentication#webhook-event-delivery
        [FunctionName("EventGridFunction")]
        public static async Task<HttpResponseMessage> Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)]HttpRequestMessage req, TraceWriter log)
        {


            var payloadFromEventGrid = JToken.ReadFrom(new JsonTextReader(new StreamReader(await req.Content.ReadAsStreamAsync())));
            dynamic eventGridSoleItem = (payloadFromEventGrid as JArray)?.SingleOrDefault();
            if (eventGridSoleItem == null)
            {
                return req.CreateErrorResponse(HttpStatusCode.BadRequest, $@"Expecting only one item in the Event Grid message");
            }

            if (eventGridSoleItem.eventType == @"Microsoft.EventGrid.SubscriptionValidationEvent")
            {
                log.Verbose(@"Event Grid Validation event received.");
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(new
                    {
                        validationResponse = ((dynamic)payloadFromEventGrid)[0].data.validationCode
                    }))
                };
            }


            log.Info("C# HTTP trigger function processed a request.");
            // Get request body
            string data = await req.Content.ReadAsStringAsync();
            var theEvents = JsonConvert.DeserializeObject<List<CustomEvent<Account>>>(data);

            //TODO : handle the events in some way here


            return new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonConvert.SerializeObject(new
                {
                    status = "good"
                }))
            };
        }
    }
}
