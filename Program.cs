using Microsoft.IdentityModel.Logging;
using Serilog;
using WebApiTraductorPMV.Extensions;
using WebApiTraductorPMV.Services;
using WebApiTraductorPMV.Utils;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.ConfigureSwagger();

//Add serilog to webapi
IConfiguration configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", false, true)
    .AddJsonFile($"appsettings.{Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"}.json", true, true)
    .AddEnvironmentVariables()
    .Build();
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(configuration)
    .CreateLogger();
builder.Host.UseSerilog();

// Injeccion de dependencias
builder.Services.AddSingleton<IConfiguration>(configuration);
builder.Services.AddScoped<IMessagesService, MessagesService>();
builder.Services.AddScoped<IAccountService, AccountService>();
builder.Services.AddScoped<IStatusService, StatusService>();
builder.Services.AddScoped<IFontsService, FontsService>();
builder.Services.AddScoped<IGraphicsService, GraphicsService>();
builder.Services.AddScoped<ISchedulesService, SchedulesService>();
builder.Services.AddSingleton<ICommonCode, CommonCode>();


//Configure Jwt Authentication
//builder.Services.ConfigureJwtAuthentication(configuration);

builder.Services.ConfigureKeycloakAuthentication(configuration);



//Configure Api Versioning
builder.Services.ConfigureApiVersioning();

//Add Cors
builder.Services.AddCors();

builder.Services.AddHealthChecks();

//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
IdentityModelEventSource.ShowPII = true;
//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls
//                                       | SecurityProtocolType.Tls11
//                                       | SecurityProtocolType.Tls12;
//ServicePointManager.Expect100Continue = true;
//ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | SecurityProtocolType.Tls13;



var app = builder.Build();

app.MapHealthChecks("/health").AllowAnonymous();

// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
//{
app.UseSwagger();
app.UseSwaggerUI();
//}

// global cors policy
app.UseCors(x => x
    .AllowAnyOrigin()
    .AllowAnyMethod()
    .AllowAnyHeader());

app.UseMiddleware<ErrorHandlerMiddleware>();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

Log.Logger.Information("init webapi...");

app.Run();
