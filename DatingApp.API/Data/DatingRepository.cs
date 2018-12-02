using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DatingApp.API.Helpers;
using DatingApp.API.Models;
using Microsoft.EntityFrameworkCore;

namespace DatingApp.API.Data
{
    public class DatingRepository : IDatingRepository
    {
        private readonly DataContext _context;
        public DatingRepository(DataContext context)
        {
            _context = context;
        }
        public void Add<T>(T entity) where T : class
        {
            _context.Add(entity);
        }

        public void Delete<T>(T entity) where T : class
        {
            _context.Remove(entity);
        }

        public async Task<User> GetUser(int id)
        {
            var user = await _context.Users.Include(p => p.Photos).FirstOrDefaultAsync(u => u.Id == id);
            return user;
        }

        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            //var users = await _context.Users.Include(p => p.Photos).ToListAsync();

            // by default users are soretd bt the last active date
            var users =  _context.Users.Include(p => p.Photos).
            OrderByDescending(u => u.LastActive).AsQueryable(); //Queryable allows to execute Where 

            users = users.Where(u => u.Id != userParams.UserId); 

            users = users.Where(u => u.Gender == userParams.Gender);

            if (userParams.Likers)
            {
                //userParams.Liker is boolean
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikers.Contains(u.Id));
            }

            if (userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(u => userLikees.Contains(u.Id));
            }

            //if userparams are not default, thatmeans we have to filter by age
            if (userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge - 1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(u => u.DateOfBirth <= maxDob && u.DateOfBirth >= minDob);
            }

            if (!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch (userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(u => u.CreatedMyProperty);
                        break;
                    default:
                        users = users.OrderByDescending(u => u.LastActive);
                        break;
                }
            }
            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        //return array of ids that represnt users that either liked
        //by this user or like this user
        private async Task<IEnumerable<int>> GetUserLikes(int id, bool likers)
        {
            //if likers = true, return user ids that like currently logged in user
            var user = await _context.Users
                .Include(x => x.Likers)
                .Include(x => x.Likees)
                .FirstOrDefaultAsync(u => u.Id == id);

            //return users that like currently logged in user
            if (likers) 
            {
                return user.Likers.Where(u => u.LikeeId == id).Select(i => i.LikerId);
            }    
            else
            {
                return user.Likees.Where(u => u.LikerId == id).Select(i => i.LikeeId);
            }
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0; //return true if more than 0 changes were saved
        }

        public async Task<Photo> GetPhoto(int id)
        {
            var photo = await _context.Photos.FirstOrDefaultAsync(p => p.Id == id);
            return photo;
        }

        public async Task<Photo> GetMainPhotoForUser(int userId)
        {
            return await _context.Photos.Where(u => u.UserId == userId)
                 .FirstOrDefaultAsync(p => p.IsMain);
            
        }

        //this method will simply verify if user already liked another user to throw error in controller
        public async Task<Like> GetLike(int userId, int recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(u =>
             u.LikerId == userId && u.LikeeId == recipientId);
        }

        public async Task<Message> GetMessage(int id) 
        {
            return await _context.Messages.FirstOrDefaultAsync(m => m.id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {

           //note User objects Sender and REcipient are needed to eventually get PhotoUrl and knowsAS to sent bacl to angular 
           var messages = _context.Messages
               .Include(u => u.Sender).ThenInclude(p => p.Photos)
               .Include(u => u.Recipient).ThenInclude(p => p.Photos)
               .AsQueryable(); //note AsQuerable is used here because it is needed in PagedList.CreateAsync method

            switch (messageParams.MessageContainer)
            {
                case "Inbox": //messages sent to UserId
                  messages = messages.Where(u => u.RecipientId == messageParams.UserId  
                    && u.RecepientDeleted == false);
                  break;
                case "Outbox":
                  messages = messages.Where(u => u.SenderId == messageParams.UserId 
                    && u.SenderDeleted == false);
                  break;
                default:
                  messages = messages.Where(u => u.RecipientId == messageParams.UserId 
                    && u.IsRead == false && u.RecepientDeleted == false);
                  break;

            }
            
            messages = messages.OrderByDescending(m=>m.MessageSent);

            //PagedList is a list of message with pagination information 
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        //note that method does not retirn PagedLit of messages, but rather IEnumerable
        public async Task<IEnumerable<Message>> GetMessageThread(int userId, int recipientId)
        {
            //note: AsQuerable is not used here because messages is just a List, NOT PagedList
            //get conversation between 2 users
            var messages = await _context.Messages
               .Include(u => u.Sender).ThenInclude(p => p.Photos)
               .Include(u => u.Recipient).ThenInclude(p => p.Photos)
               .Where(m => m.RecipientId == userId && m.RecepientDeleted == false 
                 && m.SenderId == recipientId ||
                 m.RecipientId == recipientId && m.SenderId == userId 
                 && m.SenderDeleted == false)
               .OrderByDescending(m => m.MessageSent)
               .ToListAsync();
            return messages;   
        }
    }
}