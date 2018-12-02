using System.Linq;
using AutoMapper;
using DatingApp.API.Dtos;
using DatingApp.API.Models;

namespace DatingApp.API.Helpers
{
    public class AutoMapperProfiles: Profile
    {
        public AutoMapperProfiles()
        {
            //firt parm - source, second parm - destinatiom
            CreateMap<User, UserForListDto>()
               .ForMember(dest => dest.PhotoUrl, opt => {
                   opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
               })
               .ForMember(dest => dest.Age, opt => {
                   opt.ResolveUsing(src => src.DateOfBirth.CalculateAge());
               });
            CreateMap<User, UserForDetailedDto>()
             .ForMember(dest => dest.PhotoUrl, opt => {
                   opt.MapFrom(src => src.Photos.FirstOrDefault(p => p.IsMain).Url);
               })
             .ForMember(dest => dest.Age, opt => {
                   opt.ResolveUsing(src => src.DateOfBirth.CalculateAge());
               });

            //source-destination
            CreateMap<Photo, PhotosForDetailedDto>();
            CreateMap<UserForUpdateDto, User>(); 
            CreateMap<Photo, PhotoForReturnDto>();
            CreateMap<PhotoForCreationDto, Photo>();
            CreateMap<UserForRegisterDto, User>();
            CreateMap<MessageForCreationDto, Message>().ReverseMap(); //can use mapping in both directions
            CreateMap<Message, MessageToReturnDto>()  
               .ForMember(dest => dest.SenderPhotoUrl, opt => {
                   opt.MapFrom(src => src.Sender.Photos.FirstOrDefault(p => p.IsMain).Url);
               })
               .ForMember(dest => dest.RecipientPhotoUrl, opt => {
                   opt.MapFrom(src => src.Recipient.Photos.FirstOrDefault(p => p.IsMain).Url);
               });


            
             //note that Message has Sender and REcipient as User classes, nut will map to knownAs property of dto, cool!
        }
    }
}