
﻿using backendChatApplcation.Models;

using backendChatApplcation.Requests;
using backendChatApplcation.Services;
using backendChatApplication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backendChatApplcation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IchatServices _chatService;
        public ChatController(IchatServices chatService)
        {
            _chatService = chatService;
        }
        [HttpGet("GetRoomForUsers/{userId}")]
        public ActionResult<IEnumerable<chatRoomModel>> GetRoomForUsers(int userId)
        {
            try
            {
                var chatRooms = _chatService.GetChatRoomsForUser(userId);
                if (chatRooms == null || chatRooms.Count==0)
                {
                    return NotFound(new ResponseModel
                    {
                        statusCode=404,
                        message= "No chat rooms found for the user.",
                        data="no data",
                        isSuccess=false

                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "get all your chat rooms here",
                    data = chatRooms,
                    isSuccess = true
                });
            } catch (Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = ex.InnerException?.Message ?? ex.Message,
                    data = "No data",
                    isSuccess = false
                });
            }
        }

        [HttpPost("CreateChatRoom")]
        public ActionResult<chatRoomModel> CreateChatRoom(ChatRoomRequest request)
        {
            try
            {
                var newRoom = _chatService.CreateChatRoom(request.RoomName, request.CreatorId);
                if (newRoom == null){
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 404,
                        message = "no room created",
                        data = "no data",
                        isSuccess = false

                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode=200,
                    message="room created successfully",
                    data= newRoom,
                    isSuccess=true
                });
            }catch(Exception ex)
            {
                return StatusCode(500, new ResponseModel
                {
                    statusCode = 500,
                    message = ex.InnerException?.Message ?? ex.Message,
                    data = "No data",
                    isSuccess = false
                });
            }
           
        }
      
    }
}

