using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using ColorMyTree.Controllers;
using ColorMyTree.Extensions;
using ColorMyTree.Helpers;
using ColorMyTree.Models;
using ColorMyTree.Models.Auth;
using ColorMyTree.Models.Data;
using ColorMyTree.Models.Gift;
using ColorMyTree.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;

namespace ColorMyTree
{
    public class GiftFunctions : ApiController
    {
        private readonly CacheService _cache;
        private readonly DatabaseContext _database;

        public GiftFunctions(DatabaseContext database, CacheService cache)
        {
            _database = database;
            _cache = cache;
        }

        [FunctionName("Give")]
        [OpenApiOperation(operationId: "Post", tags: new[] { "Gift" })]
        [OpenApiParameter(name: "userId", In = ParameterLocation.Path, Required = true, Type = typeof(string))]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(PostGiftRequestModel))]
        [OpenApiResponseWithoutBody(statusCode: HttpStatusCode.OK)]
        public async Task<IActionResult> RunGive(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "users/{userId}/gifts")] HttpRequest req, string userId)
        {
            var model = await req.ReadBodyAsAsync<PostGiftRequestModel>();

            if (model is not { Ornament: not null, Card: not null })
                return BadRequest("요청 데이터가 올바르지 않습니다.");

            if (String.IsNullOrWhiteSpace(model.Ornament.ImageUrl))
                return BadRequest("오너먼트 이미지가 올바르지 않습니다.");

            if (String.IsNullOrWhiteSpace(model.Ornament.PrimaryThemeColor))
                return BadRequest("테마 색상을 지정해주세요.");

            if (String.IsNullOrWhiteSpace(model.Card.NickName))
                return BadRequest("작성자 닉네임을 입력해주세요.");

            var gift = new Gift
            {
                UserId = userId,
                Card = new Card
                {
                    NickName = model.Card.NickName,
                    ImageUrl = model.Card.ImageUrl,
                    Message = model.Card.Message
                },
                Ornament = new Ornament
                {
                    ImageUrl = model.Ornament.ImageUrl,
                    PrimaryThemeColor = model.Ornament.PrimaryThemeColor,
                    SecondaryThemeColor = model.Ornament.SecondaryThemeColor
                },
                IpAddress = req.HttpContext.Connection.RemoteIpAddress?.ToString() ?? ""
            };

            _database.Gifts.Add(gift);
            await _database.SaveChangesAsync();

            await _cache.DeleteAsync($"Users/{userId}");

            return Ok(null);
        }
    }
}

