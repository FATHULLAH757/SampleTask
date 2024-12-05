using Domain.Interfaces;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Persistence.Repositories
{
    public class GeneralRepositories : IGeneral
    {
        public bool CheckIsBase64(string base64String)
        {
            if (string.IsNullOrEmpty(base64String) || base64String.Length % 4 != 0
             || base64String.Contains(" ") || base64String.Contains("\t") || base64String.Contains("\r") || base64String.Contains("\n"))
                return false;

            try
            {
                Convert.FromBase64String(base64String);
                return true;
            }
            catch (Exception exception)
            {
                throw exception;

            }
        }
        public string GetFileTypeFromBytes(byte[] fileBytes)
        {
            // Read the first few bytes (magic number)
            byte[] buffer = new byte[8];
            Array.Copy(fileBytes, buffer, buffer.Length);

            // Define known file signatures
            var fileSignatures = new Dictionary<string, string>
        {
            { "25504446", ".pdf" },      // PDF
            { "89504E47", ".png" },      // PNG
        };

            // Convert the buffer to a hexadecimal string
            string hexString = BitConverter.ToString(buffer).Replace("-", "");

            // Match the hex signature to the dictionary
            foreach (var signature in fileSignatures)
            {
                if (hexString.StartsWith(signature.Key))
                {
                    return signature.Value;
                }
            }

            return null; // File type not recognized
        }

        public string SaveAttachment(IFormFile formFiles, string type, string path)
        {
          
            string uniqueFileName = null;
            Random generator = new Random();
            String uniqueNumber = generator.Next(0, 1000000).ToString("D6");
            string NameWithoutExtension = Path.GetFileNameWithoutExtension(formFiles.FileName);
            string extension = Path.GetExtension(formFiles.FileName);
            uniqueFileName = NameWithoutExtension + uniqueNumber + type;
            string filePath = Path.Combine(path, uniqueFileName);
            formFiles.CopyTo(new FileStream(filePath, FileMode.Create));
            return uniqueFileName;
        }
    }
}
