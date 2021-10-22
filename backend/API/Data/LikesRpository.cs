using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using API.Data.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class LikesRpository : ILikesRepository
    {
        private readonly DataContext context;
        public LikesRpository(DataContext context)
        {
            this.context = context;

        }

        public async Task<UserLike> GetUserLike(int SourceUserId, int LikedUserId)
        {
            return await context.Likes.FindAsync(SourceUserId, LikedUserId);
        }

        public async Task<PagedList<LikeDTO>> GetUserLikes(LikesParams likesParams)
        {
            var users = context.Users.OrderBy(u => u.UserName).AsQueryable();
            var likes = context.Likes.AsQueryable();
            if (likesParams.Predicate == "liked")
            {
                likes = likes.Where(like => like.SourceUserId == likesParams.UserId);
                users = likes.Select(like => like.LikedUser);
            }
            if (likesParams.Predicate == "likedby")
            {
                likes = likes.Where(like => like.LikedUserId == likesParams.UserId);
                users = likes.Select(like => like.SourceUser);
            }
            var likedUsers = users.Select(user => new LikeDTO
            {
                UserName = user.UserName,
                KnownAs = user.KnownAs,
                Age = user.DateOfBirth.CalculateAge(),
                PhotoUrl = user.Photos.FirstOrDefault(p => p.IsMain).Url,
                City = user.City,
                ID = user.Id,
            });
            return await PagedList<LikeDTO>.CreateAsync(likedUsers, likesParams.pageNumber, likesParams.PageSize);


        }

        public async Task<AppUser> GetUserWithLikes(int userId)
        {
            return await context.Users.Include(x => x.LikedUsers).FirstOrDefaultAsync(x => x.Id == userId);
        }
    }
}