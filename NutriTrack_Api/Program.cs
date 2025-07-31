using Microsoft.OpenApi.Models;
using NutriTrack_Connection;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration
       .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
       .AddEnvironmentVariables();

builder.Services.AddDbContext<Context>();

var myAllowSpecificOrigins = "_myAllowSpecificOrigins";
var configuration = builder.Configuration;
var environment = builder.Environment;

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy(name: myAllowSpecificOrigins,
                      builder =>
                      {
                          builder.AllowAnyOrigin()
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                      });
});

// Get Environment Name
var envName = configuration.GetSection("EnvironmentName").Value;
var versionName = configuration.GetSection("VersionName").Value;

// Swagger
builder.Services.AddSwaggerGen(swagger =>
{
    swagger.CustomSchemaIds(type => type.ToString());
    swagger.SwaggerDoc("v1", new OpenApiInfo
    {
        Version = versionName,
        Title = $"NutriTrack.Api - {envName}"
    });
});

builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;

}).ConfigureApiBehaviorOptions(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});
//}).AddNewtonsoftJson(options =>
//{
//    options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Include;
//    options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore;
//});

//builder.Services.AddScoped(typeof(IGeneralRepository<>), typeof(GeneralRepository<>));
//builder.Services.AddScoped<IUserSettingsAppService, UserSettingsAppService>();


builder.Services.AddOptions();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "NutriTrack.Api"));
    app.UseHsts();
}

app.UseRouting();

app.UseCors(myAllowSpecificOrigins);

app.UseAuthentication();
app.UseAuthorization();

app.UseEndpoints(endpoints => endpoints.MapControllers());

app.Run();