using Keycloak.AuthServices.Authentication;
using Keycloak.AuthServices.Common;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Reflection;
using System.Text;

namespace WebApiTraductorPMV.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Permite configurar el JSON Web Token en el API Rest
    /// </summary>
    /// <param name="services">Clase se maneja los servicios que maneja la aplicación</param>
    /// <param name="configuration">Clase que maneja el archivo de configuración de la aplicación</param>
    public static void ConfigureJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = configuration["Jwt:Issuer"],
                    ValidAudience = configuration["Jwt:Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
                    ClockSkew = TimeSpan.Zero
                };
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        if (string.IsNullOrEmpty(accessToken) == false)
                        {
                            context.Token = accessToken;
                        }
                        return System.Threading.Tasks.Task.CompletedTask;
                    }
                };


            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireReaderRole", policy => policy.RequireRole(configuration.GetValue<string>("RolReadUser")));
            options.AddPolicy("RequireWriteRole", policy => policy.RequireRole(configuration.GetValue<string>("RolWriteUser")));
        });


        //services.AddAuthorization(options =>
        //{
        //    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        //    .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
        //    .RequireAuthenticatedUser()
        //    .Build();

        //});
    }

    /// <summary>
    /// Permite configurar el versionamiento del API Rest
    /// </summary>
    /// <param name="services">Clase se maneja los servicios que maneja la aplicación</param>
    public static void ConfigureApiVersioning(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.DefaultApiVersion = Microsoft.AspNetCore.Mvc.ApiVersion.Default;
            options.ReportApiVersions = true;

        });
    }

    /// <summary>
    /// Permite configurar la documentación de la app con el servicio de Swagger
    /// </summary>
    /// <param name="services">Clase se maneja los servicios que maneja la aplicación</param>
    public static void ConfigureSwagger(this IServiceCollection services)
    {
        services.AddSwaggerGen(option =>
        {
            option.SwaggerDoc("v1", new OpenApiInfo { Title = "Web API Traductor PMV", Version = "v1" });
            option.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                In = ParameterLocation.Header,
                Description = "Please enter a valid token",
                Name = "Authorization",
                Type = SecuritySchemeType.Http,
                BearerFormat = "JWT",
                Scheme = "Bearer"
            });
            option.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type=ReferenceType.SecurityScheme,
                                Id="Bearer"
                            }
                        },
                        new string[]{}
                    }
            });

            // Set the comments path for the XmlComments file.
            string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            option.IncludeXmlComments(xmlPath, true);
        });
    }

    public static void ConfigureKeycloakAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var aux = new KeycloakClientInstallationCredentials();

        aux.Secret = configuration.GetValue<string>("Keycloak:credentials:secret");

        var authenticationOptions = new KeycloakAuthenticationOptions
        {
            AuthServerUrl = configuration["Keycloak:auth-server-url"],
            Realm = configuration["Keycloak:realm"],
            Resource = configuration["Keycloak:resource"],
            Credentials = aux,
            SslRequired = configuration["Keycloak:ssl-required"],
            VerifyTokenAudience = configuration.GetValue<bool>("Keycloak:verify-token-audience")
        };

        services.AddKeycloakAuthentication(authenticationOptions);

        services.AddAuthorization(options =>
        {
            options.AddPolicy("RequireReaderRole", policy => policy.RequireRole(configuration.GetValue<string>("RolReadUser")));
            options.AddPolicy("RequireWriteRole", policy => policy.RequireRole(configuration.GetValue<string>("RolWriteUser")));
        });
    }

}
