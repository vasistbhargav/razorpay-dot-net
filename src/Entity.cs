using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Razorpay.Api
{
    public class Entity
    {
        public dynamic Attributes;
        private RestClient client;

        protected Entity Fetch(string id)
        {
            if (string.IsNullOrWhiteSpace(id))
            {
                string message = "id is null";
                throw new ArgumentNullException(message);
            }

            string entityUrl = GetEntityUrl();
            string relativeUrl = string.Format("{0}/{1}", entityUrl, id);
            List<Entity> entitiesList = Request(relativeUrl, HttpMethod.Get, null);
            return entitiesList[0];
        }

        protected List<Entity> All(Dictionary<string, object> options = null)
        {
            string entityUrl = GetEntityUrl();
            return Request(entityUrl, HttpMethod.Get, options);
        }

        protected List<Entity> Request(string relativeUrl, HttpMethod verb, Dictionary<string, object> options)
        {
            client = new RestClient();
            string postData = string.Empty;

            if ((verb == HttpMethod.Get) && (options != null))
            {
                string queryString = QueryString(options);

                relativeUrl = relativeUrl + "?" + queryString;
            }
            else if (verb == HttpMethod.Post)
            {
                postData = JsonConvert.SerializeObject(options);
            }

            string responseStr = client.MakeRequest(relativeUrl, verb, postData);

            dynamic response = JsonConvert.DeserializeObject(responseStr);

            List<Entity> entities = Build(response);

            return entities;
        }

        protected static string QueryString(IDictionary<string, object> dict)
        {
            var list = new List<string>();
            foreach (var item in dict)
            {
                string param = string.Format("{0}={1}", item.Key, item.Value);
                list.Add(param);
            }

            return string.Join("&", list);
        }

        private List<Entity> Build(dynamic response)
        {
            List<Entity> entities = new List<Entity>();

            if (response["entity"] == "collection")
            {
                foreach (dynamic item in response["items"])
                {
                    entities.Add(ParseEntity(item));
                }
            }
            else
            {
                entities.Add(ParseEntity(response));
            }

            return entities;
        }

        private Entity ParseEntity(dynamic response)
        {
            Entity entity = null;
            if (response["entity"] == "payment")
            {
                entity = new Payment();
            }
            else if (response["entity"] == "refund")
            {
                entity = new Refund();
            }
            else if (response["entity"] == "order")
            {
                entity = new Order();
            }
            else
            {
                entity = new Entity();
            }
            entity.Attributes = response;

            return entity;
        }

        protected string GetEntityUrl()
        {
            return this.GetType().Name.ToLower() + "s";
        }

        public dynamic this[string key]
        {
            get
            {
                return Attributes[key];
            }
            set
            {
                Attributes[key] = value;
            }
        }
    }
}