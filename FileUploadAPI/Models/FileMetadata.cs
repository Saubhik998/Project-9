namespace FileUploadAPI.Models
{
    public class FileMetadata
    {
        public string Id { get; set; } = string.Empty;
        public string FileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public DateTime UploadedOn { get; set; }
        public byte[] Content { get; set; } // Add content to hold the file's byte data
    }
}
