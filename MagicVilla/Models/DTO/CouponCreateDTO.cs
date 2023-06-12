using System.ComponentModel.DataAnnotations;

namespace MagicVilla.Models.DTO
{
	public class CouponCreateDTO
	{
		[Required]
		public string Name { get; set; }
		public int Percent { get; set; }
		public bool IsActive { get; set; }
	}
}
