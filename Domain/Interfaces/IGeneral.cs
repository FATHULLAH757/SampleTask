using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Interfaces
{
    public interface IGeneral
    {
        bool CheckIsBase64(string base64String);
        string GetFileTypeFromBytes(byte[] bytes);
        public string SaveAttachment(IFormFile formFiles, string type, string path);
    }
}
