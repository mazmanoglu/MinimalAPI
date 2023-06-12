using AutoMapper;
using MagicVilla.Models;
using MagicVilla.Models.DTO;

namespace MagicVilla
{
	public class MappingConfig : Profile
	{
		// We installed automapper and automapper extension microsoft dependency injection

		public MappingConfig() 
		{
			CreateMap<Coupon, CouponCreateDTO>().ReverseMap();
			CreateMap<Coupon, CouponDTO>().ReverseMap();
		}

	}
}
