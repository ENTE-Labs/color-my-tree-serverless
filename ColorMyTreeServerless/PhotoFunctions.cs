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
    public class PhotoFunctions : ApiController
    {
        private readonly Storage _storage;
        private readonly string _publicUrlEndpoint;

        public PhotoFunctions(DatabaseContext database, Storage storage, IConfiguration config)
        {
            _storage = storage;
            _publicUrlEndpoint = config["PublicUrlEndpoint"];
        }

        [FunctionName("Upload")]
        [OpenApiOperation(operationId: "Upload", tags: new[] { "Photo" })]
        [OpenApiRequestBody(contentType: "multipart/form-data", bodyType: typeof(FormCollection))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(UploadPhotoResponseModel))]
        public async Task<IActionResult> RunUpload(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "photos")] HttpRequest req)
        {
            var form = await req.ReadFormAsync();
            var file = form.Files["file"];

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest("5MB 이하의 파일을 업로드 해주세요");
            }

            var url = await _storage.UploadAsync(file.OpenReadStream(), "$web",
                $"uploads/{GenerateFileName()}{Path.GetExtension(file.FileName)}");

            return Ok(new UploadPhotoResponseModel
            {
                Url = ToPublicUrl(url)
            });
        }

        private string GenerateFileName() => DateTime.UtcNow.ToString("yyyyMMdd-HHmmss-ffffff");

        private string ToPublicUrl(string privateUrl)
        {
            var uri = new Uri(privateUrl);
            return $"https://{_publicUrlEndpoint}{uri.PathAndQuery.Replace("$web/", "")}{uri.Fragment}";
        }
    }
}

