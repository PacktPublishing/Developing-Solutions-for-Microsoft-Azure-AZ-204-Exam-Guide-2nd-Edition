using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Cdn;
using Azure.Identity;
using System.Threading.Tasks;
using System;
using Azure.Core;
using Azure.ResourceManager.Cdn.Models;
using Azure;

namespace CDNTest
{
    class Program
    {

        static void Main(string[] args)
        {
            CreateCDN().Wait();
        }

        public async static Task CreateCDN()
        {
            ArmClient client = new ArmClient(new InteractiveBrowserCredential( 
                new InteractiveBrowserCredentialOptions()
                //{ TenantId = "" }   // if your account has more then one tenant. proved the tenant id
                ));

            //resource group should be already exists in your subscription
            string resourceGroupName = "AzureCDN-RG";
            SubscriptionResource subscription = await client.GetDefaultSubscriptionAsync();
            ResourceGroupCollection resourceGroups = subscription.GetResourceGroups();
            ResourceGroupResource resourceGroup = await resourceGroups.GetAsync(resourceGroupName);

            Console.WriteLine("Group {0} found...", resourceGroup.Data.Name);

            string profileName = "myProfile" + Guid.NewGuid().ToString().Substring(0,5);
            var profileData = new ProfileData(AzureLocation.WestUS, new CdnSku { Name = CdnSkuName.StandardMicrosoft });
            var profileOutput = await resourceGroup.GetProfiles().CreateOrUpdateAsync(WaitUntil.Completed, profileName, profileData);
            ProfileResource profile = profileOutput.Value;

            Console.WriteLine("Profile {0} Created...", profile.Data.Name);

            string endpointName = "myEndpoint" + Guid.NewGuid().ToString().Substring(0, 5);
            var endpointData = new CdnEndpointData(AzureLocation.WestUS);
            DeepCreatedOrigin deepCreatedOrigin = new DeepCreatedOrigin("myOrigin")
            {
                HostName = "test.com",   //replace with your domain name
            };
            endpointData.Origins.Add(deepCreatedOrigin);

            var endpointOutput = await profile.GetCdnEndpoints().CreateOrUpdateAsync(WaitUntil.Completed, endpointName, endpointData);
            CdnEndpointResource endpoint = endpointOutput.Value;
                       
            Console.WriteLine("Endpoint {0} Created...", endpoint.Data.Name);

            Console.ReadLine();
            
        }
    }

}



