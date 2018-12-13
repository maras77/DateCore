using System.Linq;
using AutoMapper;
using DateCore.API.DTOs;
using DateCore.API.Models;

namespace DateCore.API.Helpers
{
    public class AutoMapperProfiles : Profile
    {
        public AutoMapperProfiles()
        {
            CreateMap<User,UserForListDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => {
                opt.MapFrom(src => src.DateOfBirth.CalculateAge());
            });

            CreateMap<User,UserForDetailedDTO>()
            .ForMember(dest => dest.PhotoUrl, opt => {
                opt.MapFrom(src => src.Photos.FirstOrDefault(x => x.IsMain).Url);
            })
            .ForMember(dest => dest.Age, opt => {
                opt.MapFrom(src => src.DateOfBirth.CalculateAge());
            });

            CreateMap<UserForUpdateDTO, User>();
            CreateMap<Photo, PhotosForDetailedDTO>();
            CreateMap<PhotoForCreationDTO, Photo>();
            CreateMap<Photo, PhotoForReturnDTO>();
            CreateMap<UserRegisterDTO, User>();
        }
    }
}