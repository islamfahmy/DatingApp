using System.Linq;
using API.Data.DTO;
using API.Entities;
using API.Extensions;
using AutoMapper;

namespace API.Helpers
{
    public class AutoMaperProfiles : Profile
    {
        public AutoMaperProfiles()
        {
            CreateMap<AppUser, MemberDto>().ForMember(
                dest => dest.PhotoUrl
                , opt => opt.MapFrom(
                    src => src.Photos.FirstOrDefault(
                        x => x.IsMain
                        ).Url))
                .ForMember(dest => dest.Age, opt => opt.MapFrom(src => src.DateOfBirth.CalculateAge()));
            CreateMap<MemberUpdateDto, AppUser>();
            CreateMap<Photo, PhotoDTO>();
            CreateMap<RegisterDto, AppUser>();
        }
    }
}