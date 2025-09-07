using Microsoft.AspNetCore.Authentication.JwtBearer; 
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; 
using Microsoft.IdentityModel.Tokens; 
using Microsoft.OpenApi.Models;
using NutriTrack_Connection;
using NutriTrack_Connection.Repositories;
using NutriTrack_Domains.Interfaces.Repository;
using NutriTrack_Domains.Interfaces.UserInterfaces;
using NutriTrack_Services.UserServices;
using System.Text; 

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration; 

// 1. Configura��o do DbContext com a Connection String
builder.Services.AddDbContext<Context>(options =>
    options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"))); 

builder.Services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
builder.Services.AddScoped<IRegisterAndLoginServ, RegisterAndLoginServ>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddOptions();
builder.Services.AddControllers();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["JwtSettings:Issuer"],
        ValidAudience = configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["JwtSettings:SecretKey"]))
    };
}); 

// 4. Configura��o do Swagger para usar o Token JWT
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.SwaggerDoc("v1", new OpenApiInfo { Title = "NutriTrack.Api", Version = "v1" });
    // Adiciona o bot�o "Authorize" na UI do Swagger
    swagger.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Header,
        Description = "Por favor, insira 'Bearer' seguido de um espa�o e o seu token JWT.",
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });
    swagger.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: "_myAllowSpecificOrigins",
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                                 .AllowAnyHeader()
                                 .AllowAnyMethod();
                      });
});

var app = builder.Build();

// CONFIGURA��O DO PIPELINE HTTP 

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseCors("_myAllowSpecificOrigins");

// Middlewares de Autentica��o e Autoriza��o
app.UseAuthentication(); 
app.UseAuthorization(); 

app.MapControllers(); 

app.Run();