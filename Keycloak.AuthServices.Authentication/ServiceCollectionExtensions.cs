namespace Keycloak.AuthServices.Authentication;

using Claims;
using Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;

/// <summary>
/// Configures Authentication via Keycloak
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds keycloak authentication services.
    /// </summary>
    public static AuthenticationBuilder AddKeycloakAuthentication(
        this IServiceCollection services,
        KeycloakAuthenticationOptions keycloakOptions,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        const string roleClaimType = "role";
        var validationParameters = new TokenValidationParameters
        {
            ClockSkew = keycloakOptions.TokenClockSkew,
            ValidateAudience = false,
            ValidateIssuer = true,
            NameClaimType = "preferred_username",
            RoleClaimType = roleClaimType, // TODO: clarify how keycloak writes roles
            ValidateLifetime = true
        };

        // options.Resource == Audience
        services.AddTransient<IClaimsTransformation>(_ =>
            new KeycloakRolesClaimsTransformation(roleClaimType, keycloakOptions.Resource));

        return services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opts =>
            {
                var sslRequired = string.IsNullOrWhiteSpace(keycloakOptions.SslRequired)
                    || keycloakOptions.SslRequired
                        .Equals("external", StringComparison.OrdinalIgnoreCase);

                opts.Authority = keycloakOptions.KeycloakUrlRealm;
                opts.Audience = keycloakOptions.Resource;
                opts.TokenValidationParameters = validationParameters;
                opts.RequireHttpsMetadata = sslRequired;
                opts.SaveToken = true;
                opts.IncludeErrorDetails = true;
                opts.BackchannelHttpHandler = GetHandler();
                //opts.MetadataAddress = "http://qa.movilidad-manizales.com/keycloak/realms/smm-qa-env/.well-known/openid-configuration";
                opts.MetadataAddress = $"{keycloakOptions.AuthServerUrl}/realms/{keycloakOptions.Realm}/.well-known/openid-configuration";
                opts.RequireHttpsMetadata = false;
                opts.ConfigurationManager = new CustomConfigurationManager($"{keycloakOptions.AuthServerUrl}/realms/{keycloakOptions.Realm}", $"{keycloakOptions.AuthServerUrl}");
                configureOptions?.Invoke(opts);
            });
    }


    /// <summary>
    /// Adds keycloak authentication services from configuration located in specified default section.
    /// </summary>
    /// <param name="services">Source service collection</param>
    /// <param name="configuration">Configuration source</param>
    /// <param name="configureOptions">Configure overrides</param>
    /// <returns></returns>
    public static AuthenticationBuilder AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        KeycloakAuthenticationOptions options = new();

        configuration
            .GetSection(KeycloakAuthenticationOptions.Section)
            .Bind(options, opt => opt.BindNonPublicProperties = true);

        return services.AddKeycloakAuthentication(options, configureOptions);
    }

    /// <summary>
    /// Adds keycloak authentication services from section
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <param name="keycloakClientSectionName"></param>
    /// <param name="configureOptions"></param>
    /// <returns></returns>
    public static AuthenticationBuilder AddKeycloakAuthentication(
        this IServiceCollection services,
        IConfiguration configuration,
        string? keycloakClientSectionName,
        Action<JwtBearerOptions>? configureOptions = default)
    {
        KeycloakAuthenticationOptions options = new();

        configuration
            .GetSection(keycloakClientSectionName ?? KeycloakAuthenticationOptions.Section)
            .Bind(options, opt => opt.BindNonPublicProperties = true);

        return services.AddKeycloakAuthentication(options, configureOptions);
    }

    /// <summary>
    /// Adds configuration source based on adapter config.
    /// </summary>
    /// <param name="hostBuilder"></param>
    /// <param name="fileName"></param>
    /// <returns></returns>
    public static IHostBuilder ConfigureKeycloakConfigurationSource(
        this IHostBuilder hostBuilder, string fileName = "keycloak.json") =>
        hostBuilder.ConfigureAppConfiguration((_, builder) =>
        {
            var source = new KeycloakConfigurationSource { Path = fileName, Optional = false };
            builder.Sources.Insert(0, source);
        });

    private static HttpClientHandler GetHandler()
    {
        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            SslProtocols = System.Security.Authentication.SslProtocols.Tls12,
            ServerCertificateCustomValidationCallback = (message, cert, chain, errors) => true
        };
        return handler;
    }
}
