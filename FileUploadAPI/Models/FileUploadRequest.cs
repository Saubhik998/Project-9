using Microsoft.AspNetCore.Http;

namespace FileUploadAPI.Models
{
    public class FileUploadRequest
    {
        public IFormFile File { get; set; }
    }
}
