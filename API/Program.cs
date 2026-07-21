using API;
using API.Data;
using API.Models;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Linq;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddDbContext<Context>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
});

builder.Services.AddIdentityCore<User>(options =>
{
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
    options.Password.RequireNonAlphanumeric= false;
    options.SignIn.RequireConfirmedEmail = true;
}).AddRoles<IdentityRole>()// be able to add role
    .AddRoleManager<RoleManager<IdentityRole>>() // be able to make use of RoleManager
    .AddEntityFrameworkStores<Context>() // providing our context
    .AddSignInManager<SignInManager<User>>() // make use of signin manager
    .AddUserManager<UserManager<User>>() // make use of usermanager to create user
  .AddDefaultTokenProviders(); // be able to create tokens for email confirmation

// able to use jwt to sign in
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidateIssuer = true,
            ValidateAudience = false 
        };
    });

builder.Services.AddScoped<JWTService>();
builder.Services.AddScoped<EmailService>();
builder.Services.AddScoped<ContextSeedService>();

builder.Services.AddCors();

//ValidationFailure message setup
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = actionContext =>
    {
        var erros = actionContext.ModelState
        .Where(x => x.Value.Errors.Count > 0)
        .SelectMany(x => x.Value.Errors)
        .Select(x => x.ErrorMessage).ToArray();

        var toReturn =
        new { Errors = erros };
        return new BadRequestObjectResult(toReturn);
    };
});


builder.Services.AddAuthorization(opt =>
{
    opt.AddPolicy("AdminPolicy", policy => policy.RequireRole("Admin"));
    opt.AddPolicy("ManagerPolicy", policy => policy.RequireRole("Manager"));
    opt.AddPolicy("PlayerPolicy", policy => policy.RequireRole("Player"));



    opt.AddPolicy("AdminOrManagerPolicy", policy => policy.RequireRole("Admin","Manager"));
    opt.AddPolicy("AdminAndManagerPolicy", policy => policy.RequireRole("Admin").RequireRole("Manager"));
    opt.AddPolicy("AllPolicy", policy => policy.RequireRole("Admin","Manager","Player"));

    opt.AddPolicy("AdminEmailPolicy", policy => policy.RequireClaim(ClaimTypes.Email, "admin@gmail.com"));
    opt.AddPolicy("MillerSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Surname, "miller"));
    

    opt.AddPolicy("ManagerEmailAndplayerSurnamePolicy", policy => policy.RequireClaim(ClaimTypes.Email, "manager@gmail.com").RequireClaim(ClaimTypes.Surname,"player"));
    opt.AddPolicy("VIPPolicy", policy => policy.RequireAssertion(context=>SD.VIPPolicy(context)));
});
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors(opt =>
{
    opt.AllowAnyHeader().AllowAnyMethod().AllowCredentials().WithOrigins(builder.Configuration["Jwt:ClientUrl"]);
});
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();


#region
using var scope = app.Services.CreateScope();
try
{
    var contextSeedService = scope.ServiceProvider.GetService<ContextSeedService>();
    await contextSeedService.initializeContextAsync();

}
catch(Exception ex)
{
    var logger = scope.ServiceProvider.GetService<ILogger<Program>>();
    logger.LogError(ex.Message,"Failed to initialize and data seed");
}
#endregion
app.Run();
