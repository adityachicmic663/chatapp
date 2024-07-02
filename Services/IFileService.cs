namespace backendChatApplcation.Services
{
    public interface IFileService
    {
        Task<string> SaveFileAsync(IFormFile file);

    }
}
