using backendChatApplcation.Models;
using backendChatApplication.Requests;

namespace backendChatApplication.Services
{
    public interface IAuthService
    {
        UserResponse Register(RegisterRequest request);

        string Login(LoginRequest request);

        string PasswordReset(PasswordRequest request);

        string ChangePassword(PasswordChangeRequest request);

        bool Forgotpassword(string email);

        bool resetPasswordWithOtp(ResetPasswordRequest request);

        Task<string> imageUpload(string email,IFormFile file);

    }
}
