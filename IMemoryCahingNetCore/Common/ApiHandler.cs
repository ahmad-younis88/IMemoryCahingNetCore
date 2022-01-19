using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;

namespace IMemoryCahingNetCore.Common
{
    public static class ApiHandler<T> where T : class
    {
        private static HttpClient _httpClient = new HttpClient();
        public static async Task<List<T>> GetAll(string Url)
        {
            var uriBuilder = new UriBuilder(Url);
            var httpResponse = await _httpClient.GetAsync(uriBuilder.Uri);
            return JsonConvert.DeserializeObject<List<T>>(await httpResponse.Content.ReadAsStringAsync());
        }

        public static async Task<T> GetById(string Url)
        {
            var uriBuilder = new UriBuilder(Url);
            var httpResponse = await _httpClient.GetAsync(uriBuilder.Uri);
            return JsonConvert.DeserializeObject<T>(await httpResponse.Content.ReadAsStringAsync());
        }
    }
}
