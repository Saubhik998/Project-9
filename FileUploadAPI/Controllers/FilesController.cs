using Microsoft.AspNetCore.Mvc;
using FileUploadAPI.Services;
using FileUploadAPI.Models;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;

namespace FileUploadAPI.Controllers
{
    [ApiController]
    [Route("files")]
    public class FilesController : ControllerBase
    {
        private readonly FileService _fileService;
        private readonly HttpClient _httpClient;
        private readonly ILogger<FilesController> _logger;

        public FilesController(FileService fileService, HttpClient httpClient, ILogger<FilesController> logger)
        {
            _fileService = fileService;
            _httpClient = httpClient;
            _logger = logger;
        }

        // ✅ GET /files
        [HttpGet]
        public async Task<ActionResult<List<FileMetadata>>> GetAllFiles()
        {
            try
            {
                var files = await _fileService.GetAllFilesAsync();
                return Ok(files);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching files: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // ✅ POST /files/upload
        [HttpPost("upload")]
        public async Task<IActionResult> UploadFile([FromForm] FileUploadRequest request)
        {
            if (request?.File == null || request.File.Length == 0)
                return BadRequest("No file uploaded.");

            try
            {
                using var stream = request.File.OpenReadStream();
                var fileId = await _fileService.UploadFileAsync(stream, request.File.FileName, request.File.ContentType);

                // Call the FastAPI object detection service
                var detectionResults = await GetDetectionResults(request.File);

                if (detectionResults == null)
                    return BadRequest("Object detection failed.");

                // Return the file upload response along with object detection results
                return Ok(new
                {
                    message = "File uploaded successfully",
                    fileId,
                    detectedObjects = detectionResults
                });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error uploading file: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // ✅ GET /files/{id} - Serve the image file by its ID
        [HttpGet("{id}")]
        public async Task<IActionResult> GetFileById(string id)
        {
            try
            {
                var file = await _fileService.GetFileByIdAsync(id);

                if (file == null)
                    return NotFound();

                return File(file.Content, file.ContentType);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error retrieving file by ID: {ex.Message}");
                return StatusCode(500, "Internal server error");
            }
        }

        // Function to call the FastAPI object detection service
        private async Task<object> GetDetectionResults(IFormFile file)
        {
            var url = "http://localhost:8000/detect/"; // URL of the FastAPI service

            try
            {
                var content = new MultipartFormDataContent();
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.Add("Content-Type", file.ContentType);
                content.Add(fileContent, "file", file.FileName);

                var response = await _httpClient.PostAsync(url, content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject(responseString);
                }
                else
                {
                    _logger.LogError($"FastAPI request failed: {response.StatusCode}");
                    return null;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error calling FastAPI service: {ex.Message}");
                return null;
            }
        }
    }
}
