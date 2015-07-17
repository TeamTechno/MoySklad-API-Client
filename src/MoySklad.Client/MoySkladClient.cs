using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Xml.Linq;
using RestSharp;

namespace MoySklad.Client
{
    public static class MoySkladClient
    {
        #region Fields

        #endregion

        #region Private Methods

        private static RestClient CreateClient()
        {
            if (string.IsNullOrEmpty(UserName))
                throw new InvalidOperationException($"{nameof(Password)} must be set");
            if (string.IsNullOrEmpty(Password))
                throw new InvalidOperationException($"{nameof(Password)} must be set");
            return new RestClient("https://online.moysklad.ru/exchange/rest/ms/xml")
            {
                Authenticator = new HttpBasicAuthenticator(UserName, Password)
            };
        }

        private static async Task<IRestResponse> ExecuteRequestAsync(string resource, Method method = Method.GET)
        {
            var response = await ExecuteRequestAsync(resource, (IEnumerable<Parameter>) null, method);
            if (response.StatusCode != HttpStatusCode.OK)
                throw new InvalidOperationException($"Request failed: {response.StatusDescription}");
            return response;
        }

        private static async Task<IRestResponse> ExecuteRequestAsync(string resource, Parameter parameter, Method method)
        {
            return await ExecuteRequestAsync(resource, new List<Parameter> {parameter}, method);
        }

        private static async Task<IRestResponse> ExecuteRequestAsync(string resource, IEnumerable<Parameter> parameters,
            Method method)
        {
            var req = new RestRequest(resource, method);
            if (parameters != null)
            {
                req.Parameters.AddRange(parameters);
            }
            return await CreateClient().ExecuteTaskAsync(req);
        }

        private static Parameter CreateXmlBodyParameter(string body)
        {
            return new Parameter
            {
                Name = "application/xml",
                Value = body,
                Type = ParameterType.RequestBody
            };
        }

        private static string UppercaseFirst(string s)
        {
            if (string.IsNullOrEmpty(s))
            {
                return string.Empty;
            }
            var a = s.ToCharArray();
            a[0] = char.ToUpper(a[0]);
            return new string(a);
        }

        #endregion

        #region Public

        public static string UserName { get; set; }
        public static string Password { get; set; }

        public static async Task<T> InsertUpdateEntityAsync<T>(T entity) where T : class
        {
            var response = await ExecuteRequestAsync(UppercaseFirst(typeof (T).Name),
                CreateXmlBodyParameter(entity.ToXmlWithoutXmlDeclaration()),
                Method.PUT);
            return StringXmlSerializer.FromXml<T>(response.Content);
        }

        public static async Task<T> GetEntityByNameAsync<T>(string name) where T : class
        {
            var rslt = (await GetEntityByFilterAsync<T>($"name={name}")).ToArray();
            if (rslt.Length != 1) return default(T);
            return rslt[0];
        }

        public static async Task<IEnumerable<T>> GetEntityByFilterAsync<T>(string filter) where T : class
        {
            var req = new RestRequest($"{UppercaseFirst(typeof (T).Name)}/list?filter={{filter}}", Method.GET);
            req.AddUrlSegment("filter", filter);
            var response = await CreateClient().ExecuteTaskAsync(req);
            var collection = XDocument.Load(new StringReader(response.Content)).Element("collection");
            return collection != null && collection.HasElements
                ? collection.Elements().Select(paymentIn => StringXmlSerializer.FromXml<T>(paymentIn.ToString()))
                : new Collection<T>();
        }

        public static async Task<IEnumerable<T>> GetMoySkladEntitiesAsync<T>()
        {
            var response = await ExecuteRequestAsync($"{UppercaseFirst(typeof (T).Name)}/list");
            var collection = XDocument.Load(new StringReader(response.Content)).Element("collection");
            return collection != null && collection.HasElements
                ? collection.Elements().Select(paymentIn => StringXmlSerializer.FromXml<T>(paymentIn.ToString()))
                : new Collection<T>();
        }

        public static async Task RemoveAllEntitiesAsync<T>()
        {
            var list = new IdCollection();
            list.Items.AddRange(
                (await GetMoySkladEntitiesAsync<T>()).Select(x => x.GetType().GetProperty("uuid").GetValue(x).ToString()));
            await RemoveEntityListAsync<T>(list);
        }

        public static async Task<IRestResponse> RemoveEntityListAsync<T>(IdCollection collection)
        {
            if (!collection.Items.Any())
                return null;
            var outputXml = collection.ToXmlWithoutXmlDeclaration();
            return
                await
                    ExecuteRequestAsync($"{UppercaseFirst(typeof (T).Name)}/list/delete",
                        CreateXmlBodyParameter(outputXml), Method.POST);
        }

        #endregion
    }
}