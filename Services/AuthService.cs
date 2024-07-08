using backendChatApplication.Models;
using backendChatApplication.Requests;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.IO;
using backendChatApplcation.Models;

namespace backendChatApplication.Services
{
    public class AuthService : IAuthService
    {
        private readonly chatDataContext _context;
        private readonly string _secretkey;
        private readonly IEmailSender _emailSender;
        private readonly IHttpContextAccessor _httpcontextAccessor;
        private readonly string _uploadPath;
        private readonly string[] _permittedExtensions = { ".jpg", ".jpeg", ".png", ".gif" };
        private readonly string[] _permittedMimeTypes = { "image/jpeg", "image/png", "image/gif" };

        public AuthService(chatDataContext context, IOptions<JwtSettings> jwtsettings, IEmailSender emailSender, IConfiguration configuration, IHttpContextAccessor httpcontextAccessor)
        {
            _context = context;
            _secretkey = jwtsettings.Value.SecretKey;
            _emailSender = emailSender;
            _uploadPath = configuration.GetValue<string>("UploadPath");
            _httpcontextAccessor = httpcontextAccessor;
        }

        public UserResponse Register(RegisterRequest request)
        {
            if (_context.users.Any(x => x.email == request.email))
            {
                return null;
            }

            var newUser = new UserModel
            {
                userName = request.userName,
                email = request.email,
                password = BCrypt.Net.BCrypt.HashPassword(request.password),
                role = "user",
                FirstLanguage="English",
                profilePicturePath=request.profilePicturePath,
                age= request.age,
                gender= request.gender,
                address= request.address
            };

            _context.users.Add(newUser);
            _context.SaveChanges();

            var response = new UserResponse
            {
                userName = request.userName,
                email = request.email,
                phoneNumber = request.phoneNumber,
                age = request.age,
                address = request.address,
                gender = request.gender
            };

            return response;
        }

        public string Login(LoginRequest request)
        {
            var user = _context.users.SingleOrDefault(x => x.email == request.email);

            if (user == null)
            {
                return null;
            }
            if(!BCrypt.Net.BCrypt.Verify( request.password,user.password))
            {
                return null;
            }

            return GenerateJwtToken(user);
        }

        public string PasswordReset(PasswordRequest request)
        {
            var UserEmailClaim = _httpcontextAccessor.HttpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Email)?.Value;
            var user = _context.users.SingleOrDefault(x => x.email == UserEmailClaim);
            if (user == null)
            {
                return null;
            }

            user.password =BCrypt.Net.BCrypt.HashPassword(request.newPassword);
            _context.SaveChanges();
            return "Password updated successfully";
        }

        public string ChangePassword(PasswordChangeRequest request)
        {
            var user = _context.users.SingleOrDefault(x => x.email == request.email);
            if (user == null)
            {
                return null;
            }

            user.password = BCrypt.Net.BCrypt.HashPassword(request.newPassword);
            _context.SaveChanges();
            return "Password updated successfully";
        }

        public bool Forgotpassword(string email)
        {
            var user = _context.users.SingleOrDefault(x => x.email == email);

            if (user == null)
            {
                return false;
            }

            var token = GeneratePasswordResetToken();
            user.otpToken = token;
            user.OtpTokenExpiry = DateTime.UtcNow.AddMinutes(15);
            _context.SaveChanges();

            _emailSender.SendEmailAsync(user.email, "Reset password", $"your otp is{token}").Wait();

            return true;
        }

        public bool ValidateOtpToken(string email, string token)
        {
            var user = _context.users.SingleOrDefault(x => x.email == email && x.otpToken == token);
            if (user == null || user.OtpTokenExpiry < DateTime.UtcNow)
            {
                return false;
            }

            return true;
        }

        public bool resetPasswordWithOtp(ResetPasswordRequest request)
        {
            if (!ValidateOtpToken(request.email, request.token))
            {
                return false;
            }

            var user = _context.users.SingleOrDefault(x => x.email == request.email);
            if (user == null)
            {
                return false;
            }

            user.password = request.newPassword;
            user.otpToken = null;
            user.OtpTokenExpiry = null;
            _context.SaveChanges();
            return true;
        }

        private string GeneratePasswordResetToken()
        {
            return Guid.NewGuid().ToString();
        }

        private string GenerateJwtToken(UserModel user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.ASCII.GetBytes(_secretkey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.userId.ToString()),
                    new Claim(ClaimTypes.Email, user.email),
                    new Claim(ClaimTypes.Role, user.role)
                }),
                Expires = DateTime.UtcNow.AddHours(5),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        
        public async Task<string> imageUpload(string email, IFormFile file)
        {
            try
            {
                var user = _context.users.SingleOrDefault(x => x.email == email);
                if (user == null)
                {
                    Console.WriteLine("User not found.");
                    return null;
                }
                if (file.Length > 0 && IsImageFile(file))
                {
                    if (!Directory.Exists(_uploadPath))
                    {
                        Directory.CreateDirectory(_uploadPath);
                    }
                    var filePath = Path.Combine(_uploadPath, file.FileName);

                    using (var filestream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(filestream);
                    }
                    user.profilePicturePath = filePath;
                    _context.SaveChanges();
                    return filePath;
                }
                Console.WriteLine("Invalid file format or upload failed.");
                return null;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                return null;
            }
        }

        private bool IsImageFile(IFormFile file)
        {
            var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var mimeType = file.ContentType.ToLowerInvariant();

            Console.WriteLine($"File Extension: {fileExtension}");
            Console.WriteLine($"MIME Type: {mimeType}");

            return _permittedExtensions.Contains(fileExtension) && _permittedMimeTypes.Contains(mimeType);
        }
    }
}
