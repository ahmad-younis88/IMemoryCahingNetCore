using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace IMemoryCahingNetCore.ActionFilters
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class CacheActionFilter : ActionFilterAttribute
    {
        #region :: Property Keys

        private int Seconds = 0;

        private string CachingKey = "";

        private static IMemoryCache memoryCache;

        #endregion

        public CacheActionFilter(string cachingKey)
        {
            this.CachingKey = cachingKey;
        }

        public CacheActionFilter(string cachingKey, int seconds)
        {
            this.CachingKey = cachingKey;
            this.Seconds = seconds;
        }

        public override void OnActionExecuting(ActionExecutingContext filterContext)
        {
            // declaration 
            string newCachingKey = string.Empty;
            
            var config = filterContext.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            bool isEnable = config.GetValue<bool>("Caching:EnableCaching");
            if (isEnable)
            {
                // generate new caching-key with paramter or not
                newCachingKey = GenerateCachingKey(filterContext.ActionArguments);


                // read if have caching data
                var services = filterContext.HttpContext.RequestServices;
                memoryCache = services.GetService(typeof(IMemoryCache)) as IMemoryCache;
                var isCached = memoryCache.TryGetValue(newCachingKey, out var result);
                if (isCached)
                {
                    ContentResult content = new ContentResult();
                    content.ContentType = "application/json";
                    content.StatusCode = (int)HttpStatusCode.OK;
                    content.Content = JsonConvert.SerializeObject(result, new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });
                    filterContext.Result = content;
                    return;
                }
            }

            if (!string.IsNullOrEmpty(newCachingKey))
            {
                // pass caching-key to response header to can [use / read]  from [OnResultExecuted]
                filterContext.HttpContext.Response.Headers.Clear();
                filterContext.HttpContext.Response.Headers.Add(CachingKey, newCachingKey);
            }

            base.OnActionExecuting(filterContext);
        }


        public override void OnResultExecuted(ResultExecutedContext context)
        {
            // declaration area
            string newCachingKey = CachingKey;

            var config = context.HttpContext.RequestServices.GetService(typeof(IConfiguration)) as IConfiguration;
            bool isEnable = config.GetValue<bool>("Caching:EnableCaching"); // check if caching is enable or not
            if (isEnable)
            {
                var result = context.Result as ObjectResult;
                if (result != null)
                {
                    // check the value not null and status of HttpResult equal Ok
                    if (result.Value != null && context.Result is OkObjectResult)
                    {
                        // read second default if not pass second on same action
                        int second = this.Seconds;
                        if (second == 0) // when not passing number of second get default second from configuration
                        {
                            second = config.GetValue<int>("Caching:CacheSeconds");
                        }

                        // cache options setting
                        var cacheExpiryOptions = new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTime.Now.AddSeconds((second == 0 ? 60 : second)),
                            Priority = CacheItemPriority.High,
                            SlidingExpiration = TimeSpan.FromSeconds((second == 0 ? 60 : second))
                        };

                        // read caching-key from HttpContext Response header and get generated-caching-key
                        bool isValue = context.HttpContext.Response.Headers.TryGetValue(CachingKey, out var values);
                        if (isValue)
                        {
                            newCachingKey = values.ToString();
                        }

                        // save result data on ImemoryCache
                        memoryCache.Set(newCachingKey, result.Value, cacheExpiryOptions);
                    }
                }
            }

            base.OnResultExecuted(context);
        }

        /// <summary>
        /// generate caching-key with parampter to make unique key , 
        /// when change parampter value , then have new key
        /// </summary>
        /// <param name="argumentsData"></param>
        /// <returns></returns>

        private string GenerateCachingKey(IDictionary<string, object> argumentsData)
        {
            string _CachingKey = this.CachingKey;
            foreach (var argument in argumentsData)
            {
                _CachingKey += argument.Value;
            }

            return _CachingKey;
        }
    }
}
