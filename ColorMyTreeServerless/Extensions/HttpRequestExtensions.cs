using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;

namespace ColorMyTree.Extensions
{
    public static class HttpRequestExtensions
    {
        public static async Task<T> ReadBodyAsAsync<T>(this HttpRequest request)
        {
            var bodyString = await new StreamReader(request.Body).ReadToEndAsync();
            return JsonConvert.DeserializeObject<T>(bodyString);
        }
    }
}
