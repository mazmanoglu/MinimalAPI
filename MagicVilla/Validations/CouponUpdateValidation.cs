﻿using FluentValidation;
using MagicVilla.Models.DTO;

namespace MagicVilla.Validations
{
	public class CouponUpdateValidation:AbstractValidator<CouponUpdateDTO>
	{
		public CouponUpdateValidation() 
		{
			RuleFor(model => model.Id).NotEmpty().GreaterThan(0);
			RuleFor(model => model.Name).NotEmpty();
			RuleFor(model => model.Percent).InclusiveBetween(1, 100);
		}
	}
}
