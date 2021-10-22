using System.Collections.Generic;
using System.Threading.Tasks;
using API.Data.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    [Authorize]
    public class LikesController : BaseApiController
    {
        private readonly ILikesRepository likesRepository;
        private readonly IUserRepository userRepository;
        public LikesController(IUserRepository userRepository, ILikesRepository likesRepository)
        {
            this.userRepository = userRepository;
            this.likesRepository = likesRepository;

        }
        [HttpPost("{username}")]
        public async Task<ActionResult> AddLike(string username)
        {
            var SourceUserId = User.GetUserId();
            var likedUser = await userRepository.GetUserByUserNameAsync(username);
            var sourceUser = await likesRepository.GetUserWithLikes(SourceUserId);

            if (likedUser == null) return NotFound();

            if (sourceUser.UserName == username) return BadRequest("You can't like yourself");

            var userLike = await likesRepository.GetUserLike(SourceUserId, likedUser.Id);

            if (userLike != null) return BadRequest("You already Like this user");

            userLike = new UserLike
            {
                SourceUserId = SourceUserId,
                LikedUserId = likedUser.Id,

            };

            sourceUser.LikedUsers.Add(userLike);

            if (await userRepository.SaveAllAsync()) return Ok();

            return BadRequest("Failed to Like user");

        }
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LikeDTO>>> GetUserLikes([FromQuery] LikesParams likesParams)
        {
            likesParams.UserId = User.GetUserId();
            var users = await likesRepository.GetUserLikes(likesParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);
            return Ok(users);
        }
    }
}