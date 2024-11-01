##########################################################################################
# Following commands should be executed in PowerShell 
# You can install PowerShell locally or use Cloud Shell
# Make sure you install Azure CLI  http://aka.ms/azcli
##########################################################################################

#values from the previous run for Event Hub
$eventhub = ''  # <<<<<--- please provide the short name of eventhub from the previous run
$groupname = 'EventHubDemo-RG'

#Create a resource group
New-AzResourceGroup -Location 'UK South' -Name EventGridDemo-RG

#Enable the Event Grid resource provider
Register-AzResourceProvider -ProviderNamespace Microsoft.EventGrid

#Create a resource group to monitor
New-AzResourceGroup -Location 'UK South' -Name EventGridMonitoring

#Pull Azure subscription id
$subid = (az account show --query id -o tsv)

#Сonfigure event subscription endpoint
$endpoint = "/subscriptions/$subid/resourceGroups/$groupname/providers/Microsoft.EventHub/namespaces/$eventhub/eventhubs/$eventhub"

#Create event hub subscription destination object
$destObj = New-AzEventGridEventHubEventSubscriptionDestinationObject -ResourceId $endpoint

#Specify the scope to monitor
$scope = "/subscriptions/$subid/resourceGroups/EventGridMonitoring"

#Create a subscription for the events from the Resource group
New-AzEventGridSubscription -Name 'group-monitor-sub' -Scope $scope -Destination $destObj

###################################################################
##  START EVENT HUB CONSUMER subscriber.exe FROM PREVIOUS DEMO   ##
###################################################################

###################################################################### 
##  TO GENERATE ACTIVITY REPEAT THE FOLLOWING COMMAND several times  #
###################################################################### 

# Update tag of monitoring RG. Required about 45 to appear in the subscriber console
Set-AzResourceGroup -Name EventGridMonitoring -Tag @{ Code = (Get-Random) }

# do not delete the provision resources, it will be required for next step
