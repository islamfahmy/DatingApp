using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using API.Data;
using API.Data.DTO;
using API.Entities;
using API.Extensions;
using API.Helpers;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [Authorize]
    public class UsersController : BaseApiController
    {
        private readonly IUserRepository UserRepository;
        private readonly IMapper mapper;
        private readonly IPhotoService photoService;

        public UsersController(IUserRepository UserRepository, IMapper mapper, IPhotoService photoService)
        {
            this.photoService = photoService;
            this.mapper = mapper;
            this.UserRepository = UserRepository;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<MemberDto>>> GetUsers([FromQuery] UserParams userParams)
        {
            var user = await UserRepository.GetUserByUserNameAsync(User.GetUserName());
            userParams.currentUserName = User.GetUserName();
            if (string.IsNullOrEmpty(userParams.gender))
                userParams.gender = user.Gender == "male" ? "female" : "male";
            var users = await UserRepository.GetMembersAsync(userParams);
            Response.AddPaginationHeader(users.CurrentPage, users.PageSize, users.TotalCount, users.TotalPages);

            return Ok(users);
        }
        [HttpGet("{username}", Name = "GetUser")]

        public async Task<ActionResult<MemberDto>> GetUsers(string username)
        {
            var user = await UserRepository.GetMemberAsync(username);
            return mapper.Map<MemberDto>(user);
        }
        [HttpPut]
        public async Task<ActionResult> UpdateUser(MemberUpdateDto memberUpdateDto)
        {
            var username = User.GetUserName();
            var user = await UserRepository.GetUserByUserNameAsync(username);
            mapper.Map(memberUpdateDto, user);
            UserRepository.Update(user);
            if (await UserRepository.SaveAllAsync()) return NoContent();
            return BadRequest("Failed To Update User");
        }
        [HttpPost("add-phtos")]
        public async Task<ActionResult<PhotoDTO>> AddPhoto(IFormFile file)
        {
            var user = await UserRepository.GetUserByUserNameAsync(User.GetUserName());

            var result = await photoService.AddPhotoAsync(file);

            if (result.Error != null)
            {
                return BadRequest(result.Error.Message);
            }
            var photo = new Photo
            {
                Url = result.SecureUrl.AbsoluteUri,
                PublicId = result.PublicId
            };
            if (user.Photos.Count == 0)
                photo.IsMain = true;
            user.Photos.Add(photo);
            if (await UserRepository.SaveAllAsync())
                return CreatedAtRoute("GetUser", new { username = user.UserName }, mapper.Map<PhotoDTO>(photo));
            return BadRequest("Problem adding photo");
        }
        [HttpPut("set-main-photo/{photoId}")]
        public async Task<ActionResult> SetMainPhoto(int photoId)
        {
            var user = await UserRepository.GetUserByUserNameAsync(User.GetUserName());

            var photo = user.Photos.FirstOrDefault(x => x.id == photoId);

            if (photo.IsMain) return BadRequest("This is already your main photo");

            var currentMain = user.Photos.FirstOrDefault(x => x.IsMain);
            if (currentMain != null) currentMain.IsMain = false;
            photo.IsMain = true;

            if (await UserRepository.SaveAllAsync()) return NoContent();

            return BadRequest("Failed to set main photo");
        }
        [HttpDelete("delete-photo/{photoId}")]
        public async Task<ActionResult> DeletePhoto(int photoId)
        {
            var user = await UserRepository.GetUserByUserNameAsync(User.GetUserName());
            var photo = user.Photos.FirstOrDefault(x => x.id == photoId);
            if (photo == null) return NotFound();
            if (photo.IsMain) return BadRequest("You cannot delete you main photo");
            if (photo.PublicId != null)
            {
                var res = await photoService.DeletePhtoAsync(photo.PublicId);
                if (res.Error != null)
                    return BadRequest(res.Error);
            }
            user.Photos.Remove(photo);
            if (await UserRepository.SaveAllAsync()) return Ok();
            return BadRequest("failed to delete the photo");
        }




    }

}