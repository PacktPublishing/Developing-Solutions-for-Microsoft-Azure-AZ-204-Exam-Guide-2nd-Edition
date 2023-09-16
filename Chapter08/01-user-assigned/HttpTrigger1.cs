using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Azure.Core;

namespace Company.Function
{
    public static class HttpTrigger1
    {
        [FunctionName("HttpTrigger1")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("C# HTTP trigger function processed a request.");

            string userAssignedClientId = "Client ID of the user-assigned managed identity";
            string secretName = "Name of the secret in Key Vault";
            Uri vaultUri = new("https://<key-vault-name>.vault.azure.net/");
            TokenCredential credential = new DefaultAzureCredential(new DefaultAzureCredentialOptions { ManagedIdentityClientId = userAssignedClientId });

            try
            {
                var client = new SecretClient(vaultUri, credential);

                KeyVaultSecret secret = await client.GetSecretAsync(secretName);

                string responseMessage = $"Secret value: { secret.Value }";

                return new OkObjectResult(responseMessage);
            }
            catch (Azure.RequestFailedException)
            {
                /* DefaultAzureCredential couldn't obtain a token, try AzureCliCredential
                * This is due to a known issue at the time of writing with the
                * VisualStudioCodeCredential and certain versions of the Azure Account
                extension: https://github.com/Azure/azure-sdk-for-net/issues/27263*/

                credential = new AzureCliCredential();

                var client = new SecretClient(vaultUri, credential);

                KeyVaultSecret secret = await client.GetSecretAsync(secretName);

                string responseMessage = $"Secret value: { secret.Value }";

                return new OkObjectResult(responseMessage);
            }
        }
    }
}
