using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using DateCore.API.Models;
using System.Linq;
using DateCore.API.Helpers;
using System;

namespace DateCore.API.Data
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

        public async Task<User> GetUser(Guid id, bool isCurrentUser)
        {
            var query = _context.Users.Include(x => x.Photos).AsQueryable();
            if(isCurrentUser)
                query = query.IgnoreQueryFilters();
            
            return await query.FirstOrDefaultAsync(x => x.Id == id);
        }


        public async Task<PagedList<User>> GetUsers(UserParams userParams)
        {
            var users =  _context.Users.Include(x => x.Photos).OrderByDescending(x => x.LastActive).AsQueryable();
            users = users.Where(x => x.Id != userParams.UserId);
            users = users.Where(x => x.Gender == userParams.Gender);

            if(userParams.Likers)
            {
                var userLikers = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikers.Contains(x.Id));
            }

            if(userParams.Likees)
            {
                var userLikees = await GetUserLikes(userParams.UserId, userParams.Likers);
                users = users.Where(x => userLikees.Contains(x.Id));
            }


            if(userParams.MinAge != 18 || userParams.MaxAge != 99)
            {
                var minDob = DateTime.Today.AddYears(-userParams.MaxAge -1);
                var maxDob = DateTime.Today.AddYears(-userParams.MinAge);
                users = users.Where(x => x.DateOfBirth >= minDob && x.DateOfBirth <= maxDob);
            }

            if(!string.IsNullOrEmpty(userParams.OrderBy))
            {
                switch(userParams.OrderBy)
                {
                    case "created":
                        users = users.OrderByDescending(x => x.Created);
                        break;
                    default:
                        users = users.OrderByDescending(x => x.LastActive);
                        break;
                    
                }
            }

            return await PagedList<User>.CreateAsync(users, userParams.PageNumber, userParams.PageSize);
        }

        private async Task<IEnumerable<Guid>> GetUserLikes(Guid userId, bool likers)
        {
            var user = await _context.Users
            .Include(x => x.Likers)
            .Include(x => x.Likees)
            .FirstOrDefaultAsync(u => u.Id == userId);

            if(likers)
            {
                return user.Likers.Where(u => u.LikeeId == userId).Select(x => x.LikerId);
            }
            return user.Likees.Where(u => u.LikerId == userId).Select(x => x.LikeeId);
        }

        public async Task<Photo> GetPhoto(Guid id)
        {
            return await _context.Photos.IgnoreQueryFilters().FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<Photo> GetMainPhotoForUser(Guid userId)
        {
            return await _context.Photos.Where(x => x.UserId == userId).FirstOrDefaultAsync(x => x.IsMain);
        }

        public async Task<bool> SaveAll()
        {
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<Like> GetLike(Guid userId, Guid recipientId)
        {
            return await _context.Likes.FirstOrDefaultAsync(x => 
                x.LikerId == userId && x.LikeeId == recipientId
            );
        }

        public async Task<Message> GetMessage(Guid id)
        {
            return await _context.Messages.FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<PagedList<Message>> GetMessagesForUser(MessageParams messageParams)
        {
            var messages = _context.Messages
                .Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                .AsQueryable();
            
            switch (messageParams.MessageContainer)
            {
                case "Inbox":
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && !x.IsRecipientDeleted);
                    break;
                case "Outbox":
                    messages = messages.Where(x => x.SenderId == messageParams.UserId && !x.IsSenderDeleted);
                    break;
                default:
                    messages = messages.Where(x => x.RecipientId == messageParams.UserId && !x.IsRecipientDeleted && !x.IsRead);
                    break;
            }

            messages = messages.OrderByDescending(x => x.MessageSent);
            return await PagedList<Message>.CreateAsync(messages, messageParams.PageNumber, messageParams.PageSize);
        }

        public async Task<IEnumerable<Message>> GetMessageThread(Guid userId, Guid recipientId)
        {
            return await _context.Messages
                .Include(x => x.Sender).ThenInclude(x => x.Photos)
                .Include(x => x.Recipient).ThenInclude(x => x.Photos)
                .Where(x => x.RecipientId == userId && !x.IsRecipientDeleted && x.SenderId == recipientId
                || x.RecipientId == recipientId && x.SenderId == userId && !x.IsSenderDeleted)
                .OrderByDescending(x => x.MessageSent)
                .ToListAsync();

        }
    }
}