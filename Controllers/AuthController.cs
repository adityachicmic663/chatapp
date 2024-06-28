using backendChatApplication.Models;
using backendChatApplication.Requests;
using backendChatApplication.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Mvc;
using System;

namespace backendChatApplication.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        [AllowAnonymous]
        public IActionResult Register(RegisterRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    statusCode = 400,
                    message = "Invalid data",
                    data = "No data",
                    isSuccess = false
                });
            }

            try
            {
                var token = _authService.Register(request);
                if (token == null)
                {
                    return Conflict(new ResponseModel
                    {
                        statusCode = 409,
                        message = "Email already exists",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 201,
                    message = "Registered successfully",
                    data = token,
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during registration.");
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public IActionResult Login(LoginRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    statusCode = 400,
                    message = "Invalid data",
                    data = "No data",
                    isSuccess = false
                });
            }

            try
            {
                var token = _authService.Login(request);
                if (token == null)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode = 404,
                        message = "User not found or invalid credentials",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Login successful",
                    data = token,
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during login.");

                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }
        [Authorize(Roles="user,admin")]
        [HttpPost("userpasswordreset")]
        public IActionResult PasswordReset(PasswordRequest request)
        {
            try
            {
                var result = _authService.PasswordReset(request);
                if (result == null)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode = 404,
                        message = "User not found or invalid credentials",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Password updated successfully",
                    data = "No data",
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An error occurred during password reset.");
                return StatusCode(500, new ResponseModel
                {
                    
                statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }

        [Authorize(Roles = "admin")]
        [HttpPost("adminPasswordReset")]
        public IActionResult AdminPasswordReset(PasswordChangeRequest request)
        {
            try
            {
                var result = _authService.ChangePassword(request);
                if (result == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
                        message = "Invalid credentials",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Password updated successfully",
                    data = "No data",
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }

        [HttpPost("forgotpassword")]
        public IActionResult ForgotPassword(forgetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    statusCode = 400,
                    message = "Invalid data",
                    data = "No data",
                    isSuccess = false
                });
            }

            try
            {
                var result = _authService.Forgotpassword(request.email); 
                if (!result)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode = 404,
                        message = "User not found or invalid email",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Password reset email sent successfully",
                    data = "No data",
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }

        [HttpPost("resetpassword")]
     
        public IActionResult ResetPasswordWithOtp(ResetPasswordRequest request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new ResponseModel
                {
                    statusCode = 400,
                    message = "Invalid data",
                    data = "No data",
                    isSuccess = false
                });
            }

            try
            {
                var result = _authService.resetPasswordWithOtp(request);
                if (!result)
                {
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
                        message = "OTP do not match or unauthorized",
                        data = "No data",
                        isSuccess = false
                    });
                }

                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Password has been reset",
                    data = "No data",
                    isSuccess = true
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = "Internal server error",
                    data = ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false
                });
            }
        }

        [Authorize(Roles = "user,admin")]
        [HttpPost("upload-image")]
        public async Task<IActionResult> UploadImage(IFormFile file)
        {
            try
            {

                var filepath =await _authService.imageUpload(User.Identity.Name,file);
                if (filepath == null)
                {
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
                        message = "Invalid file format or upload failed",
                        data = null,
                        isSuccess = false
                    });
                }
                    

                return Ok(new ResponseModel
                { 
                    statusCode = 200,
                    message = "File uploaded successfully",
                    data=null,
                    isSuccess = true 
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                { 
                    statusCode = 500, 
                    message = "Internal server error",
                    data=ex.InnerException?.Message ?? ex.Message,
                    isSuccess = false 
                });
            }
        }
    }
}
