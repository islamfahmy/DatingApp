using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using API.Data;
using API.Data.DTO;
using API.Entities;
using API.Interfaces;
using AutoMapper;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    public class AccountController : BaseApiController
    {
        private readonly DataContext context;
        private readonly ITokenService tokenService;
        private readonly IMapper mapper;

        public AccountController(DataContext context, ITokenService tokenService, IMapper mapper)
        {
            this.mapper = mapper;
            this.context = context;
            this.tokenService = tokenService;
        }
        [HttpPost("register")]
        public async Task<ActionResult<UserDto>> Register(RegisterDto data)
        {
            if (await UserExists(data.UserName))
                return BadRequest("Username is taken");
            var user = mapper.Map<AppUser>(data);
            using var hmac = new HMACSHA512();
            user.UserName = data.UserName.ToLower();
            user.PasswordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            user.PasswordSalt = hmac.Key;
            context.Users.Add(user);
            await context.SaveChangesAsync();
            return new UserDto
            {
                UserName = data.UserName,
                Token = tokenService.CreateToken(user),
                KnownAs = user.KnownAs
            };
        }
        [HttpPost("login")]
        public async Task<ActionResult<UserDto>> Login(LoginDto data)
        {
            var user = await context.Users.Include(p => p.Photos).SingleOrDefaultAsync(x => x.UserName == data.UserName.ToLower());
            if (user == null) return Unauthorized("Invalid username");
            using var hmac = new HMACSHA512(user.PasswordSalt);
            var ComputedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data.Password));
            for (int i = 0; i < ComputedHash.Length; i++)
            {
                if (i >= user.PasswordHash.Length || ComputedHash[i] != user.PasswordHash[i])
                    return Unauthorized("Invalid Password");

            }
            return new UserDto
            {
                UserName = data.UserName,
                Token = tokenService.CreateToken(user),
                PhotoUrl = user.Photos.FirstOrDefault(x => x.IsMain)?.Url,
                KnownAs = user.KnownAs,
                Gender = user.Gender
            };
        }
        private async Task<bool> UserExists(string UserName)
        {
            return await context.Users.AnyAsync(x => x.UserName == UserName.ToLower());
        }
    }
}