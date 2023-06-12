using AutoMapper;
using FluentValidation;
using MagicVilla;
using MagicVilla.Data;
using MagicVilla.Models;
using MagicVilla.Models.DTO;
using Microsoft.AspNetCore.Mvc;
using System.Net;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddAutoMapper(typeof(MappingConfig));
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
	app.UseSwagger();
	app.UseSwaggerUI();
}

app.MapGet("/helloworldget", () => "Hello World from Minimal API"); // ==> HTTP GET
app.MapGet("/helloworld", () =>
{
	return "Hello World from detailed text";
});
app.MapGet("/helloworldexception", () =>
{
	return Results.NotFound("sorry guys, we couldn't find your webpage, come tomorrow");
});

app.MapGet("/helloworldwithid/{id:int}", (int id) =>
{
	return Results.Ok("your id is " + id);
}); // id would be int only, if we write a string it gives 404 not found, but if couldn't put 'id:int' there, and then give a string value, it gives 400 bad request.

app.MapPost("/helloworldpost", () => Results.Ok("Hello World from API Post")); // ==> HTTP POST


/*
	After this line, we make a simple crud operations using real world sample
	We created coupon class and couponstore data class
 */

// app.MapGet("/api/coupon", () => Results.Ok(CouponStore.couponList)); // write this a bit longer
app.MapGet("/api/coupon", (ILogger<Program> _logger) =>
{
	//APIResponse response = new APIResponse();
	APIResponse response = new(); //new expression is simplified in .Net 7
	_logger.Log(LogLevel.Information, "Getting all coupons"); // used ILogger as dependency injection
	response.Result = CouponStore.couponList;
	response.IsSuccess = true;
	response.StatusCode = HttpStatusCode.OK;

	// return Results.Ok(CouponStore.couponList);
	return Results.Ok(response);
}).WithName("GetCoupons").Produces<APIResponse>(200);
	//.WithName("GetCoupons").Produces<IEnumerable<Coupon>>(200);

app.MapGet("/api/coupon/{id:int}", (ILogger<Program> _logger, int id) =>
{
	APIResponse response = new();
	response.Result = CouponStore.couponList.FirstOrDefault(coupon => coupon.Id == id);
	response.IsSuccess = true;
	response.StatusCode = HttpStatusCode.OK;
	return Results.Ok(response);
}).WithName("GetCoupon").Produces<APIResponse>(200);

// we will not use after we add DTO
/*app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
{
	if (coupon.Id != 0 || string.IsNullOrEmpty(coupon.Name))
		return Results.BadRequest("Invalid ID or coupon name");

	// use another (name) validation to not to create the same coupon again
	if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon.Name.ToLower()) != null)
	{
		return Results.BadRequest("Coupon Name already exists.");
	}

	coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
	CouponStore.couponList.Add(coupon);

	// when we create, the coupon ID must be 0, if it is not, then that is not a valid ID.
	// because ID is something that typically the database, all the code is responsible for adding.
	// but here we don't have/use any database. So, we need to populate the ID
	// CouponStore.couponList.OrderByDescending(u=>u.Id).FirstOrDefault().Id ==> this gives the last id number

	// return Results.Ok(coupon);
	// sometimes we use 'created' rather than returning an Ok in HTTP POST
	//return Results.Created($"/api/coupon/{coupon.Id}", coupon);
	return Results.CreatedAtRoute("GetCoupon", new {id=coupon.Id},coupon);

}).WithName("CreateCoupon").Accepts<Coupon>("application/json").Produces<Coupon>(201).Produces(400);
*/

app.MapPost("/api/coupon", async (IMapper _mapper, 
	IValidator<CouponCreateDTO> _validation, [FromBody] CouponCreateDTO coupon_C_DTO) =>
{
	APIResponse response = new() { IsSuccess=false,StatusCode=HttpStatusCode.BadRequest};
	


	//var validationResult = _validation.ValidateAsync(coupon_C_DTO).GetAwaiter().GetResult(); //use async instead of GetAwaiter().GetResult()
	var validationResult = await _validation.ValidateAsync(coupon_C_DTO);


	/*
	if (string.IsNullOrEmpty(coupon_C_DTO.Name))
		return Results.BadRequest("Invalid ID or coupon name");
	*/
	// after validation
	if (!validationResult.IsValid)
	{
		response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
		return Results.BadRequest(response);
	}

	// use another (name) validation to not to create the same coupon again
	if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon_C_DTO.Name.ToLower()) != null)
	{
		response.ErrorMessages.Add("Coupon Name already exists.");
		return Results.BadRequest(response);
	}

	/*
	Coupon coupon = new()
	{
		Name = coupon_C_DTO.Name,
		IsActive = coupon_C_DTO.IsActive,
		Percent = coupon_C_DTO.Percent
	};
	*/
	// after mapper
	Coupon coupon = _mapper.Map<Coupon>(coupon_C_DTO);


	coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
	CouponStore.couponList.Add(coupon);

	/*
	CouponDTO couponDTO = new()
	{
		Id = coupon.Id,
		Name = coupon.Name,
		IsActive = coupon.IsActive,
		Percent = coupon.Percent,
		Created = coupon.Created
	};
	*/
	// after automapper
	CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);


	response.Result = couponDTO;
	response.IsSuccess = true;
	response.StatusCode = HttpStatusCode.Created;
	return Results.Ok(response);

	/* 
		when we create, the coupon ID must be 0, if it is not, then that is not a valid ID.
		because ID is something that typically the database, all the code is responsible for adding.
		but here we don't have/use any database. So, we need to populate the ID
		CouponStore.couponList.OrderByDescending(u=>u.Id).FirstOrDefault().Id ==> this gives the last id number

		return Results.Ok(coupon);
		sometimes we use 'created' rather than returning an Ok in HTTP POST


		return Results.CreatedAtRoute("GetCoupon", new { id = coupon.Id }, couponDTO);
		return Results.Created($"/api/coupon/{coupon.Id}", coupon);
	*/

}).WithName("CreateCoupon").Accepts<CouponCreateDTO>("application/json").Produces<APIResponse>(201).Produces(400);




app.MapPut("/api/coupon", async (IMapper _mapper,
	IValidator<CouponUpdateDTO> _validation, [FromBody] CouponUpdateDTO coupon_Update_DTO) =>
{
	APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

	var validationResult = await _validation.ValidateAsync(coupon_Update_DTO);

	if (!validationResult.IsValid)
	{
		response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
		return Results.BadRequest(response);
	}

	/* no need for now
	 
	if (CouponStore.couponList.FirstOrDefault(u => u.Name.ToLower() == coupon_Update_DTO.Name.ToLower()) != null)
	{
		response.ErrorMessages.Add("Coupon Name already exists.");
		return Results.BadRequest(response);
	}
	*/

	Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == coupon_Update_DTO.Id);
	couponFromStore.IsActive = coupon_Update_DTO.IsActive;
	couponFromStore.Name = coupon_Update_DTO.Name;
	couponFromStore.Percent = coupon_Update_DTO.Percent;
	couponFromStore.LastUpdated = DateTime.Now;

	/*
	Coupon coupon = _mapper.Map<Coupon>(coupon_Update_DTO);
	coupon.Id = CouponStore.couponList.OrderByDescending(u => u.Id).FirstOrDefault().Id + 1;
	CouponStore.couponList.Add(coupon);
	CouponDTO couponDTO = _mapper.Map<CouponDTO>(coupon);
	*/

	response.Result = _mapper.Map<CouponDTO>(couponFromStore);
	response.IsSuccess = true;
	response.StatusCode = HttpStatusCode.OK;
	return Results.Ok(response);
}).WithName("UpdateCoupon").Accepts<CouponUpdateDTO>("application/json").Produces<APIResponse>(200).Produces(400);






app.MapDelete("/api/coupon/{id:int}", (int id) =>
{
	APIResponse response = new() { IsSuccess = false, StatusCode = HttpStatusCode.BadRequest };

	/*
	No need a validation, for now 
	
	var validationResult = await _validation.ValidateAsync(coupon_Update_DTO); 

	if (!validationResult.IsValid)
	{
		response.ErrorMessages.Add(validationResult.Errors.FirstOrDefault().ToString());
		return Results.BadRequest(response);
	}
	*/


	Coupon couponFromStore = CouponStore.couponList.FirstOrDefault(u => u.Id == id);
	if (couponFromStore != null)
	{
		CouponStore.couponList.Remove(couponFromStore);
		response.IsSuccess = true;
		response.StatusCode = HttpStatusCode.NoContent;
		return Results.Ok(response);
	}
	else
	{
		response.ErrorMessages.Add("Invalid ID");
		return Results.BadRequest(response);
	}

});

app.UseHttpsRedirection();
app.Run();
