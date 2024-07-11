
﻿using backendChatApplcation.Models;

using backendChatApplcation.Requests;
using backendChatApplcation.Services;
using backendChatApplication.Models;
using Microsoft.AspNetCore.Authorization;
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
        public ActionResult<IEnumerable<chatRoomResponse>> GetRoomForUsers(int userId, int messageToskip,int message)
        {
            try
            {
                var chatRooms = _chatService.GetChatRoomsForUser(userId,messageToskip,message);
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
                    message ="get your chats here",
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

        [HttpPost("CreateGroup")]
        [Authorize]
        public ActionResult<chatRoomResponse> CreateChatRoom(ChatRoomRequest request)
        {
            try
            {
                var newRoom = _chatService.CreateChatRoom(request.RoomName);
                if (newRoom == null){
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
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
     
        [HttpPost("upload/group/{chatRoomId}")]

        public async Task<ActionResult<groupChatResponse>> UploadFileToGroup(int senderId,int chatRoomId, IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
                        message = "file is not selected",
                        data = "no data",
                        isSuccess = false
                    });
                }
              var response=  await _chatService.SendFileMessage(senderId, chatRoomId, file);
                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Ok your response is this",
                    data = response,
                    isSuccess = true
                });
            }
            catch(Exception ex)
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
        [HttpPost("upload/personal")]
        public async Task<ActionResult<oneToOneResponse>> UploadFileToUser(int senderId,int receiverId,IFormFile file)
        {
            try
            {
                var newMessage = await _chatService.SendDirectFileMessage(senderId, receiverId, file);
                if (newMessage == null)
                {
                    return BadRequest( new ResponseModel
                    {
                        statusCode = 400,
                        message = "Error occurred while sending direct file message.",
                        data = "No data",
                        isSuccess = false
                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "Direct file message sent successfully.",
                    data = newMessage,
                    isSuccess = true
                });
            }
            catch (Exception ex)
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
        [HttpPost("chat")]
        [Authorize]
        public async Task<IActionResult> GetChats(chatRequest request)
        {
            try
            {
                var (chat, Totalmessages) = await _chatService.GetChatAsync(request.ChatRoomId, request.pageNumber, request.pageSize);
                if(Totalmessages == 0)
                {
                    return BadRequest(new ResponseModel
                    {
                        statusCode = 400,
                        message = "no message to show or you are not a part of group",
                        data = "no data",
                        isSuccess = false
                    });
                }
                return Ok(new ResponseModel
                {
                    statusCode = 200,
                    message = "all message of these group",
                    data = chat,
                    isSuccess = true
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

