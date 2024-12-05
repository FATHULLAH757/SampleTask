using Domain.Entities;
using Domain.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using WebApi.Config;
using WebApi.Models;

namespace WebApi.Controllers
{
    [Route("/v1/blobs")]
    [ApiController]
    [Authorize]
    public class UploadAndGetFileController : ControllerBase
    {
        private readonly IConfiguration _config;
        private readonly IGeneral _general;
        private readonly IApplicationWriteDbConnection _applicationWriteDbConnection;
        private readonly IApplicationReadDbConnection _readDbConnection;
        public UploadAndGetFileController(IConfiguration configuration, IGeneral general, IApplicationWriteDbConnection applicationWriteDbConnection, IApplicationReadDbConnection readDbConnection)
        {
            _config = configuration;
            _general = general;
            _applicationWriteDbConnection = applicationWriteDbConnection;
            _readDbConnection = readDbConnection;
        }
        
        [HttpPost]
        public async Task<APIResponce> UploadFile(Models.UploadFile uploadFile)
        {
            var result = _general.CheckIsBase64(uploadFile.data);
            if (!string.IsNullOrEmpty(uploadFile.data) && result is true)
            {
                byte[] bytes = Convert.FromBase64String(uploadFile.data);
                var type = _general.GetFileTypeFromBytes(bytes);
                using (var memoryStream = new MemoryStream(bytes))
                {
                    IFormFile file = new FormFile(memoryStream, 0, bytes.Length, "File", "general.png");
                    var path = _config["FilePath:AttachmentFilePath"].ToString();
                    long fileSize = file.Length;
                    var result1 = _general.SaveAttachment(file, type, path);
                    if (!string.IsNullOrEmpty(result1))
                    {

                        string sql = "INSERT INTO UploadFile (Id, Name, CreatedOn, Size, FilePath) VALUES (@Id, @Name, GetDate(), @Size,@FilePath)";
                        var parameters = new { Id = uploadFile.Id, Name = result1, Size = fileSize , FilePath  = result1 };

                        var saveRecord = await _applicationWriteDbConnection.ExecuteAsync(sql, parameters);
                        if (saveRecord > 0)
                        {
                            return new APIResponce((int)HttpStatusCode.OK, "File Saved Successfully");
                        }
                    }


                }
            }
            else
            {
                return new APIResponce((int)HttpStatusCode.InternalServerError, "Please Upload Valid String");
            }
            return new APIResponce((int)HttpStatusCode.InternalServerError);
        }
        [AllowAnonymous]
        [HttpGet("generatetoken")]
        public string GenerateToken()
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_config["SecurityKey:JWTKey"].ToString()); // Replace with your secret key
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Expires = DateTime.UtcNow.AddHours(1),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> RetriveFile(string id)
        {
            var query = $"Select * from UploadFile where Id = {id}";
            var fileResult = await _readDbConnection.QueryFirstOrDefaultAsync<Domain.Entities.UploadFile>(query);

            string path = Path.Combine(_config["FilePath:AttachmentFilePath"].ToString())+"/" + fileResult.FilePath;

            byte[] bytes = System.IO.File.ReadAllBytes(path);
            string base64Content = Convert.ToBase64String(bytes);

            return Ok(new { id = fileResult.Id, data = base64Content, size = fileResult.Size, created_at = fileResult.CreatedOn});
        }
    }

}



