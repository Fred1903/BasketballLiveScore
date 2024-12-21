using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.Services;
using System.Text.Json.Serialization;
//using Basketball_LiveScore.Server.Controllers;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BasketballDBContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("BasketballDBContext") ?? throw new InvalidOperationException("Connection string 'BasketballDBContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
//Scoped car va génerer une nvl instance pr chaque requête HTTP
builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IAuthService, AuthService>();



builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("https://localhost:4200") 
              .AllowAnyHeader()
              .AllowAnyMethod()
              .SetIsOriginAllowed(origin => true)
              .AllowCredentials();
    });
});

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });



// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("AllowFrontend");


app.UseDefaultFiles();
app.UseStaticFiles();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");


app.Run();
