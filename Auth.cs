﻿using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Client;
using System;
using System.Net.Http.Headers;

namespace appsvc_fnc_dev_OrgInfoList
{
    class Auth
    {
        public GraphServiceClient graphAuth(ILogger log)
        {
            IConfiguration config = new ConfigurationBuilder()

           .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
           .AddEnvironmentVariables()
           .Build();

            log.LogInformation("C# HTTP trigger function processed a request.");
            var scopes = new string[] { "https://graph.microsoft.com/.default" };
            var keyVaultUrl = config["keyVaultUrl"];
            var clientId = config["clientId"];
            var tenantid = config["tenantid"];
            var secretName = config["secretName"];

            SecretClientOptions options = new SecretClientOptions()
            {
                Retry =
            {
                Delay= TimeSpan.FromSeconds(2),
                MaxDelay = TimeSpan.FromSeconds(16),
                MaxRetries = 5,
                Mode = RetryMode.Exponential
             }
            };
            var client = new SecretClient(new Uri(keyVaultUrl), new DefaultAzureCredential(), options);

            KeyVaultSecret secret = client.GetSecret(secretName);

            string secretValue = secret.Value;

            //IConfidentialClientApplication confidentialClientApplication = ConfidentialClientApplicationBuilder
            //.Create(clientId)
            //.WithTenantId(tenantid)
            //.WithClientSecret(secretValue)
            //.Build();

            //// Build the Microsoft Graph client. As the authentication provider, set an async lambda
            //// which uses the MSAL client to obtain an app-only access token to Microsoft Graph,
            //// and inserts this access token in the Authorization header of each API request. 
            //GraphServiceClient graphServiceClient =
            //    new GraphServiceClient(new DelegateAuthenticationProvider(async (requestMessage) =>
            //    {

            //        // Retrieve an access token for Microsoft Graph (gets a fresh token if needed).
            //        var authResult = await confidentialClientApplication
            //            .AcquireTokenForClient(scopes)
            //            .ExecuteAsync();

            //        // Add the access token in the Authorization header of the API request.
            //        requestMessage.Headers.Authorization =
            //            new AuthenticationHeaderValue("Bearer", authResult.AccessToken);
            //    })
            //    );
            //return graphServiceClient;


            // using Azure.Identity;
            var tokenOptions = new TokenCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            // https://docs.microsoft.com/dotnet/api/azure.identity.clientsecretcredential
            var clientSecretCredential = new ClientSecretCredential(tenantid, clientId, secretValue, tokenOptions);

            try
            {
                var graphClient = new GraphServiceClient(clientSecretCredential, scopes);
                return graphClient;
            }
            catch (Exception e)
            {
                log.LogInformation($"Message: {e.Message}");
                if (e.InnerException is not null)
                    log.LogInformation($"InnerException: {e.InnerException.Message}");
                return null;
            }

        }
    }
}
