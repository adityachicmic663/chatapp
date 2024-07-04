
using backendChatApplcation.Models;
using backendChatApplcation.Services;

using backendChatApplication.Models;
using backendChatApplication.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backendChatApplcation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserServices _userService;

        public UserController(IUserServices userService)
        {
            _userService = userService;
        }

        [HttpGet("searchUser")]
        public ActionResult<List<UserResponse>> SearchUser(string searchKey)
        {
            try
            {
                var listofUsers = _userService.SearchUser(searchKey);
                if (listofUsers == null)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode=404,
                        message="user not found",
                        data="no data",
                        isSuccess=false
                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "all the users are here",
                    data=listofUsers,
                    isSuccess=true
                });
            }catch(Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode=500,
                    message=ex.Message,
                    data="no data",
                    isSuccess=false
                });
            }
        }
        [HttpGet("userWithStatus")]
        public async Task<IActionResult> getUserWithStatus(int userId)
        {
            try
            {
                var userList = _userService.GetUsersWithStatus(userId);
                if(userList == null|| userList.Count==0)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode=404,
                        message="not found",
                        data="no data",
                        isSuccess=false
                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode=200,
                    message="get your users",
                    data=userList,
                    isSuccess=true
                });
            }catch(Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = ex.Message,
                    data = "no data",
                    isSuccess = false
                });
            }
        }


       
       
    }
}
