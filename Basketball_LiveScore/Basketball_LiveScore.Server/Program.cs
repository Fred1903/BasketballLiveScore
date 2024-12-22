using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Basketball_LiveScore.Server.Data;
using Basketball_LiveScore.Server.Services;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
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
builder.Services.AddScoped<IMatchService, MatchService>();




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
    {//On ajoute des serializer pour les enum, refHandler pour relation circulaire
        options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });


//Auth JWT
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters()
    {
        ValidateActor = true, //identifie les parties
        ValidateAudience = true,//empeche les attaques par rejeu (un site ne peut pas rejouer le tokeen sur un autre site)
        ValidateLifetime = true, //limiter la durée de vie du token
        ValidateIssuerSigningKey = true, //demande utilisatíon de signature
        ValidIssuer = builder.Configuration["Jwt:Issuer"], //emetteur du token
        ValidAudience = builder.Configuration["Jwt:Audience"],//recepteur du token
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),//Clé de chiffrement
        ClockSkew = TimeSpan.Zero, //par defaut 5min
    };

});


builder.Services.AddAuthorization(options =>
{
    // Admins seulement
    options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));

    // Utilisateurs seulement
    options.AddPolicy("UserOnly", policy => policy.RequireRole("User"));

    // Tous les utilisateurs connectés (Admin ou User)
    options.AddPolicy("AuthenticatedUsers", policy => policy.RequireAuthenticatedUser());

    // Utilisateurs non connectés (anonymes)
    options.AddPolicy("AnonymousOnly", policy =>
    {
        policy.RequireAssertion(context => !context.User.Identity.IsAuthenticated);
    });
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
app.UseAuthentication(); // Doit être avant UseAuthorization
app.UseAuthorization();

app.MapControllers();

app.MapFallbackToFile("/index.html");


app.Run();
