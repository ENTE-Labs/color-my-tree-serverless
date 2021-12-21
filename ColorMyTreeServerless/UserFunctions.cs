using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColorMyTree.Controllers;
using ColorMyTree.Extensions;
using ColorMyTree.Helpers;
using ColorMyTree.Models;
using ColorMyTree.Models.Auth;
using ColorMyTree.Models.Data;
using ColorMyTree.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace ColorMyTree
{
    public class UserFunctions : ApiController
    {
        private readonly CacheService _cache;
        private readonly DatabaseContext _database;
        private readonly DateTimeOffset _christmas = new DateTimeOffset(2021, 12, 25, 0, 0, 0, TimeSpan.FromHours(9));
        private readonly ILogger<UserFunctions> _logger;

        public UserFunctions(DatabaseContext database, CacheService cache, ILogger<UserFunctions> logger)
        {
            _database = database;
            _cache = cache;
            _logger = logger;
        }

        [FunctionName("UserProfile")]
        [OpenApiOperation(operationId: "Get", tags: new[] { "User" })]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UserResponseModel))]
        public async Task<IActionResult> RunUserProfile(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "users/{userId}")] HttpRequest req, string userId)
        {
            var nowIsAfterChristmas = DateTimeOffset.UtcNow > _christmas;

            if (String.IsNullOrWhiteSpace(userId))
                return new NotFoundResult();

            _logger.LogInformation($"Profile: start get profile from cache {userId}");

            var response = await _cache.GetOrCreateAsync($"Users/{userId}", async () => await GetUserAsync(userId, nowIsAfterChristmas), expiry: TimeSpan.FromHours(1));

            _logger.LogInformation($"Profile: finish get profile from cache {userId}");

            if (response == null)
                return new NotFoundResult();

            if (!response.IsAfterChristmas && nowIsAfterChristmas)
            {
                _logger.LogInformation($"Profile: start get profile from DB {userId}");

                response = await GetUserAsync(userId, nowIsAfterChristmas);

                _logger.LogInformation($"Profile: finish get profile from DB {userId}");

                if (response == null)
                    return new NotFoundResult();

                _logger.LogInformation($"Profile: start set profile to cache {userId}");

                await _cache.SetAsync($"Users/{userId}", response, expiry: TimeSpan.FromHours(1));

                _logger.LogInformation($"Profile: finish set profile to cache {userId}");
            }

            return Ok(response);

        }

        private async Task<UserResponseModel> GetUserAsync(string userId, bool nowIsAfterChristmas)
        {
            var user = await _database.Users
                .FirstOrDefaultAsync(u => u.Id == userId);

            var gifts = await _database.Gifts
                .Where(g => g.UserId == userId)
                .OrderBy(g => g.CreatedAt)
                .ToArrayAsync();

            if (!nowIsAfterChristmas)
            {
                foreach (var g in gifts)
                {
                    g.Card.Message = null;
                }
            }

            if (user == null)
                return null;

            return new UserResponseModel(user, gifts)
            {
                IsAfterChristmas = nowIsAfterChristmas
            };
        }
    }
}

