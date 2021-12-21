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
    public class AuthFunctions : ApiController
    {
        private readonly DatabaseContext _database;
        private readonly string _databaseName;
        private readonly ILogger<AuthFunctions> _logger;

        public AuthFunctions(DatabaseContext database, IConfiguration config, ILogger<AuthFunctions> logger)
        {
            _database = database;
            _logger = logger;
            _databaseName = config["DatabaseName"];
        }

        [FunctionName("Login")]
        [OpenApiOperation(operationId: "Login", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginRequestModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AuthResponseModel))]
        public async Task<IActionResult> RunLogin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/login")] HttpRequest req)
        {
            var model = await req.ReadBodyAsAsync<LoginRequestModel>();

            if (model == null)
                return BadRequest("��û �����Ͱ� �����ϴ�.");

            if (String.IsNullOrWhiteSpace(model.Id))
                return BadRequest("���̵� �Է����ּ���.");

            if (String.IsNullOrWhiteSpace(model.Password))
                return BadRequest("��й�ȣ�� �Է����ּ���.");

            _logger.LogInformation($"Login: start querying user for login {model.Id}");

            var user = await _database.Users.FirstOrDefaultAsync(u => u.Login == model.Id);

            _logger.LogInformation($"Login: finish querying user for login {model.Id}");

            if (user == null)
                return Unauthorized("���̵� Ȯ�����ּ���.");

            if (!CryptoHelper.VerifyHashedPassword(user.Password, model.Password))
                return Unauthorized("��й�ȣ�� Ȯ�����ּ���.");

            _logger.LogInformation($"Login: password verified {model.Id}");

            return Ok(new AuthResponseModel
            {
                NickName = user.NickName,
                UserId = user.Id
            });
        }

        [FunctionName("Join")]
        [OpenApiOperation(operationId: "Join", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginRequestModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AuthResponseModel))]
        public async Task<IActionResult> RunJoin(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "auth/join")] HttpRequest req)
        {
            var model = await req.ReadBodyAsAsync<JoinRequestModel>();

            if (model == null)
                return BadRequest("��û �����Ͱ� �����ϴ�.");

            if (String.IsNullOrWhiteSpace(model.Id))
                return BadRequest("���̵� �Է����ּ���.");

            if (model.Id.Length is < 4 or > 20)
                return BadRequest("���̵�� 4���� �̻� 20���� �̸����� �Է� ���ּ���.");

            if (!Regex.IsMatch(model.Id, "^[a-z0-9_]+$"))
                return BadRequest("���̵�� ���ҹ��ڿ� ���� �׸��� ����ٸ� ����� �� �ֽ��ϴ�.");

            if (String.IsNullOrWhiteSpace(model.NickName))
                return BadRequest("�г����� �Է����ּ���.");

            if (String.IsNullOrWhiteSpace(model.Password) || model.Password.Length < 4)
                return BadRequest("�� ���� �̻��� ��й�ȣ�� �Է����ּ���.");

            _logger.LogInformation($"Join: start querying existing user {model.Id}");

            if (await _database.Users.FirstOrDefaultAsync(u => u.Login == model.Id) != null)
                return Conflict("�̹� �����ϴ� ���̵��Դϴ�.");

            _logger.LogInformation($"Join: finish querying existing user {model.Id}");

            var user = new CmtUser
            {
                Login = model.Id,
                Password = CryptoHelper.HashPassword(model.Password),
                NickName = model.NickName,
            };

            var cosmosClient = _database.Database.GetCosmosClient();
            var container = cosmosClient.GetContainer(_databaseName, "Users");
            var trigger = await container.Scripts.ReadTriggerAsync("check_user_duplicate");

            try
            {
                _logger.LogInformation($"Join: start create new user {model.Id}");

                await container.CreateItemAsync(user, new PartitionKey(user.Id),
                    new ItemRequestOptions { PostTriggers = new List<string> { "check_user_duplicate" } });

                _logger.LogInformation($"Join: finish create new user {model.Id}");
            }
            catch (CosmosException e) when (e.Message.Contains("already two or more"))
            {
                _logger.LogInformation($"Join: error. the user already exists {model.Id}");

                return Conflict("�̹� �����ϴ� ���̵��Դϴ�.");
            }

            return Ok(new AuthResponseModel
            {
                NickName = user.NickName,
                UserId = user.Id
            });
        }

        [FunctionName("Unique")]
        [OpenApiOperation(operationId: "Unique", tags: new[] { "Auth" })]
        [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(LoginRequestModel))]
        [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(AuthResponseModel))]
        public async Task<IActionResult> RunUnique(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "auth/unique")] HttpRequest req)
        {
            var login = req.Query["login"].ToString();

            var users = await _database.Users.Where(u => u.Login == login).ToArrayAsync();
            if (users.Length < 2) return Ok("�ʿ� ����");

            int suffix = 1;
            for (var i = 1; i < users.Length; i++)
            {
                var newLogin = $"{users[0].Login}{suffix}";
                while (await _database.Users.FirstOrDefaultAsync(u => u.Login == newLogin) != null)
                {
                    suffix++;
                    newLogin = $"{users[0].Login}{suffix}";
                }

                users[i].Login = newLogin;
                suffix++;
            }

            await _database.SaveChangesAsync();

            return Ok(users.Select(u => u.Login));
        }
    }
}

