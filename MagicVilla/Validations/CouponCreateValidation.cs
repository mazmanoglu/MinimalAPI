using FluentValidation;
using MagicVilla.Models.DTO;

namespace MagicVilla.Validations
{
	public class CouponCreateValidation : AbstractValidator<CouponCreateDTO>
	{
		public CouponCreateValidation() 
		{
			RuleFor(model => model.Name).NotEmpty();
			RuleFor(model => model.Percent).InclusiveBetween(1, 100);
		}
	}
}
