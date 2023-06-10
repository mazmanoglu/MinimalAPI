using MagicVilla.Data;
using MagicVilla.Models;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

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
	_logger.Log(LogLevel.Information, "Getting all coupons"); // used ILogger as dependency injection
	return Results.Ok(CouponStore.couponList);
}).WithName("GetCoupons").Produces<IEnumerable<Coupon>>(200);

app.MapGet("/api/coupon/{id:int}", (int id) =>
{
	return Results.Ok(CouponStore.couponList.FirstOrDefault(coupon => coupon.Id == id));
}).WithName("GetCoupon").Produces<Coupon>(200);

app.MapPost("/api/coupon", ([FromBody] Coupon coupon) =>
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

app.MapPut("/api/coupon", () =>
{

});

app.MapDelete("/api/coupon/{id:int}", (int id) =>
{

});

app.UseHttpsRedirection();
app.Run();
