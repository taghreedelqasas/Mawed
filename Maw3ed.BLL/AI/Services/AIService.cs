using Maw3ed.BLL.AI.Interfaces;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace Maw3ed.BLL.AI.Services
{
    public class AIService : IAIService
    {
        private readonly HttpClient _http;
        private readonly IConfiguration _config;

        public AIService(HttpClient http, IConfiguration config)
        {
            _http = http;
            _config = config;
        }

        public async Task<string> ChatAsync(string prompt)
        {
            var apiKey = _config["StudentBedrock:ApiKey"];

            _http.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", apiKey);

            // We'll fill the request body once we know the API format.

            return "";
        }
    }
}
