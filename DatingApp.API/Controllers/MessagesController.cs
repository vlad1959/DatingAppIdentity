using System.Security.Claims;
using System.Threading.Tasks;
using AutoMapper;
using DatingApp.API.Data;
using DatingApp.API.Dtos;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System;

namespace DatingApp.API.Controllers
{   
    [ServiceFilter(typeof(LogUserActivity))] //this will use this class to update activity date whenever any method is called fom this controller
    [Authorize]
    [Route("api/users/{userId}/[controller]")]
    [ApiController]

    public class MessagesController: ControllerBase
    {
        private readonly IDatingRepository _repo;
        private readonly IMapper _mapper;
        public MessagesController(IDatingRepository repo, IMapper mapper)
        {
            _mapper = mapper;
            _repo = repo;
        }

        [HttpGet("{id}", Name="GetMessage")]
        //note userId will come from the route and id from the HTTP request
        public async Task<IActionResult> GetMessage(int userId, int id)
        {
            //make sure that user in token matches user id that was passed in the router
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messageFromRepo = await _repo.GetMessage(id);

            if (messageFromRepo == null) 
            {
                return NotFound();
            } 
            return Ok(messageFromRepo);
        }

        [HttpGet]
        // userId will come from route api/users/{userId}/messages")
        public async Task<IActionResult> GetMessagesForUser(int userId, [FromQuery]MessageParams messageParams)
        {
            //make sure that user in token matches user id that was passed in the router
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            messageParams.UserId = userId;
            var messagesFromRepo = await _repo.GetMessagesForUser(messageParams);
            var messages = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo);
        
            Response.AddPagination(messagesFromRepo.CurrentPage,
            messagesFromRepo.PageSize, messagesFromRepo.TotalCount, messagesFromRepo.TotalPages);
            

            return Ok(messages);
        }

        //note this path was added to distiguish from another HttpGet({{id}})
        //recipientId won't come from messageParms, but will be submitted in url and then received by the router
        // http://localhost:5000/api/users/1/messages/thread/11
        [HttpGet("thread/{recipientId}")]
        public async Task<IActionResult> GetMessageThread(int userId, int recipientId)
        {
            //make sure that user in token matches user id that was passed in the router
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var messagesFromRepo = await _repo.GetMessageThread(userId, recipientId);

            //send back reduced objects, not the whole message objects

            var messageThread = _mapper.Map<IEnumerable<MessageToReturnDto>>(messagesFromRepo); 

            return Ok(messageThread);
        }

        //this is used to create a new message, no parms are needed since message id will be generated
        //userId will come from the route
        //MessageForCreationDto will be sent by angular
        [HttpPost]
        public async Task<IActionResult> CreateMessage(int userId, MessageForCreationDto messageForCreationDto)
        {
         //make sure that user in token matches user id that was passed in the router
             var sender = await _repo.GetUser(userId);
            
            if (sender.Id != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            //userid is id of logged in user, will be retieved from router
            messageForCreationDto.SenderId = userId;

            //recipient id will be sent with the body of http post
            var recipient = await _repo.GetUser(messageForCreationDto.RecipientId);

            if (recipient == null)
                return BadRequest("Could not find a recipient of the message");

        
            //map dto to message object, which is a database table

            var message = Mapper.Map<Message>(messageForCreationDto);
            _repo.Add(message); //note: this is not async method, so no wait

            //map message object back to dto to return limitted data
            //note: since sender and recipient were pulled from a database in variables,
            //automapper will map knownAs and photoUrls of both to messageToReturn dto 
            
            if (await _repo.SaveAll()) //save to a database
            {
                //note that after saving to a database, message object will get generated id automatically
                //and will be mapped to MessageToReturnDto
                var messageToReturn = _mapper.Map<MessageToReturnDto>(message);
                return CreatedAtRoute("GetMessage", new { id=message.id }, messageToReturn);
            }

            throw new System.Exception("Creatng the message failed on save");
        }
        [HttpPost("{id}")]
        //id - id of the message
        //HTTPPOST is used instead of delete is because the message is not going to be deletd completely
        //it will be deleted from recipient inbox (logged in user), but not from sender outbox
        //ONLY OF BOTH SENDER AND RECIPIENT DELETD A MESSAGE, IT WILL BE DELETED FROM DATABASE
        public async Task<IActionResult> DeleteMessage(int id, int userId)
        {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();
            var messageFromRepo = await _repo.GetMessage(id);
            if (messageFromRepo.SenderId == userId) {
                messageFromRepo.SenderDeleted = true;
            }

            if (messageFromRepo.RecipientId == userId) {
                messageFromRepo.RecepientDeleted = true;
            }

            if ( messageFromRepo.SenderDeleted && messageFromRepo.RecepientDeleted) {
                _repo.Delete(messageFromRepo); //delete in memory
            }

            if (await _repo.SaveAll()) //save all updates to a database
              return NoContent();

            throw new System.Exception("Error deleting the message");  
        }
    
    // set indicator that message was read
    // id - is id of the message
    [HttpPost("{id}/read")]
    public async Task<IActionResult> MarkMessageAsRead(int userId, int id)
    {
            if (userId != int.Parse(User.FindFirst(ClaimTypes.NameIdentifier).Value))
                return Unauthorized();

            var message = await _repo.GetMessage(id);

            if (message.RecipientId != userId)
               return Unauthorized();

            message.IsRead = true;
            message.DateRead = DateTime.Now;

            await _repo.SaveAll();
            return NoContent();

    }
  }
}