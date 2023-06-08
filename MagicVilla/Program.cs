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
app.MapPost("/helloworldpost",()=> "Hello World from API Post"); // ==> HTTP POST

app.UseHttpsRedirection();
app.Run();
