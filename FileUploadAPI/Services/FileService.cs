using FileUploadAPI.Models;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;
using MongoDB.Bson;
using FileUploadAPI.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace FileUploadAPI.Services
{
    public class FileService
    {
        private readonly MongoDBContext _context;

        public FileService()
        {
            _context = new MongoDBContext();
        }

        // ✅ Upload file to MongoDB GridFS
        public async Task<string> UploadFileAsync(Stream stream, string fileName, string contentType)
        {
            var options = new GridFSUploadOptions
            {
                Metadata = new BsonDocument
                {
                    { "contentType", contentType },
                    { "uploadedOn", DateTime.UtcNow }
                }
            };

            ObjectId id = await _context.GridFS.UploadFromStreamAsync(fileName, stream, options);
            return id.ToString();
        }

        // ✅ Fetch all files from MongoDB GridFS
        public async Task<List<FileMetadata>> GetAllFilesAsync()
        {
            var filter = Builders<GridFSFileInfo>.Filter.Empty;
            using var cursor = await _context.GridFS.FindAsync(filter);
            var files = await cursor.ToListAsync();

            return files.Select(f => new FileMetadata
            {
                Id = f.Id.ToString(),
                FileName = f.Filename,
                ContentType = f.Metadata?.GetValue("contentType", "").AsString ?? "",
                UploadedOn = f.UploadDateTime
            }).ToList();
        }

        // ✅ Fetch file by ID from MongoDB GridFS and return file content along with metadata
        public async Task<FileMetadata> GetFileByIdAsync(string id)
        {
            // Convert string ID to ObjectId
            var objectId = new ObjectId(id);

            // Use the proper filter to find by ObjectId
            var filter = Builders<GridFSFileInfo>.Filter.Eq(f => f.Id, objectId);

            var file = await _context.GridFS.Find(filter).FirstOrDefaultAsync();

            if (file == null)
            {
                return null; // Return null if file not found
            }

            // Open the stream to fetch file content
            using (var stream = await _context.GridFS.OpenDownloadStreamAsync(file.Id))
            {
                var fileContent = new byte[stream.Length];
                await stream.ReadAsync(fileContent, 0, (int)stream.Length);

                // Return the file metadata along with the file content
                return new FileMetadata
                {
                    Id = file.Id.ToString(),
                    FileName = file.Filename,
                    ContentType = file.Metadata?.GetValue("contentType", "").AsString ?? "",
                    UploadedOn = file.UploadDateTime,
                    Content = fileContent // Add file content
                };
            }
        }
    }
}
