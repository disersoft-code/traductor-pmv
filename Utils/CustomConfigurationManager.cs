using Microsoft.IdentityModel.Protocols;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Tokens;
using System.Net;

namespace WebApiTraductorPMV.Utils;

public class CustomConfigurationManager : IConfigurationManager<OpenIdConnectConfiguration>
{
    private readonly string authority;
    private readonly string authorityReturnedOrigin;

    public CustomConfigurationManager(string authority, string authorityReturnedOrigin)
    {
        this.authority = authority;
        this.authorityReturnedOrigin = authorityReturnedOrigin;
    }
    public async Task<OpenIdConnectConfiguration> GetConfigurationAsync(CancellationToken cancel)
    {
        //ServicePointManager.ServerCertificateValidationCallback += (sender, cert, chain, sslPolicyErrors) => true;

        HttpClientHandler clientHandler = new HttpClientHandler();
        clientHandler.ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => { return true; };
        //clientHandler.ServerCertificateCustomValidationCallback = HttpClientHandler.DangerousAcceptAnyServerCertificateValidator;
        clientHandler.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
        var httpClient = new HttpClient(clientHandler);

        var request = new HttpRequestMessage
        {
            RequestUri = new Uri($"{authority}/.well-known/openid-configuration"),
            Method = HttpMethod.Get
        };

        var configurationResult = await httpClient.SendAsync(request, cancel);
        var resultContent = await configurationResult.Content.ReadAsStringAsync(cancel);
        if (configurationResult.IsSuccessStatusCode)
        {
            var config = OpenIdConnectConfiguration.Create(resultContent);
            var jwks = config.JwksUri.Replace(authorityReturnedOrigin, authority);
            jwks = jwks.Replace("https", "http");
            var keyRequest = new HttpRequestMessage
            {
                RequestUri = new Uri(jwks),
                Method = HttpMethod.Get
            };
            var keysResposne = await httpClient.SendAsync(keyRequest, cancel);
            var keysResultContent = await keysResposne.Content.ReadAsStringAsync(cancel);
            if (keysResposne.IsSuccessStatusCode)
            {
                config.JsonWebKeySet = new JsonWebKeySet(keysResultContent);
                var signingKeys = config.JsonWebKeySet.GetSigningKeys();
                foreach (var key in signingKeys)
                {
                    config.SigningKeys.Add(key);
                }
            }
            else
            {
                throw new Exception($"Failed to get jwks: {keysResposne.StatusCode}: {keysResultContent}");
            }

            return config;
        }
        else
        {
            throw new Exception($"Failed to get configuration: {configurationResult.StatusCode}: {resultContent}");
        }
    }

    public void RequestRefresh()
    {
        // if you are caching the configuration this is probably where you should invalidate it
    }
}