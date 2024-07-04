namespace backendChatApplcation.Services
{
    public class fileServices:IFileService
    {
        private readonly string _fileDirectory;

        public fileServices()
        {
            _fileDirectory = Path.Combine(Directory.GetCurrentDirectory(), "assets/UploadedFiles");
            if (!Directory.Exists(_fileDirectory))
            {
                Directory.CreateDirectory(_fileDirectory);
            }
        }
        public async Task<string> SaveFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            var fileName = $"{Guid.NewGuid()}_{file.FileName}";
            var filePath = Path.Combine(_fileDirectory, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return filePath;

        }

        public string GetFileType(string contentType)
        {
            if (contentType.StartsWith("image/"))
                return "Image";
            if (contentType == "application/pdf")
                return "Pdf";
            if (contentType.StartsWith("text/"))
                return "Text";

            return "Other";
        }
    }
}
