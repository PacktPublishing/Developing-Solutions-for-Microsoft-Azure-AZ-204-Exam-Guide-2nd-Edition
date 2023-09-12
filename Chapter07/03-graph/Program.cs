using Microsoft.Graph;
using Azure.Identity;

// App registration variables
const string _clientId = "Put your app/client ID here";
const string _tenantId = "Put your tenant ID here";

// These scopes can be added to for demonstration purposes
string[] scopes = { "User.Read" };

// Configure the interactive browser credential options
var options = new InteractiveBrowserCredentialOptions
{
    ClientId = _clientId,
    TenantId = _tenantId,
    RedirectUri = new Uri("http://localhost")
};

// Obtain a token from the interactive authentication provider
var credential = new InteractiveBrowserCredential(options);

// Create a new Graph client with the authentication provider.
GraphServiceClient graphClient = new (credential);

// Make a Microsoft Graph API query
var user = await graphClient.Me.GetAsync();

// Customized greeting for the logged-in user
Console.WriteLine($"Hello, {user?.GivenName}! Your name was obtained from MS Graph.\n"); 