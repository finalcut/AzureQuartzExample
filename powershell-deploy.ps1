[CmdletBinding()]
Param(
  [Parameter(Mandatory=$True,Position=1)]
   [string]$subscription,

   [Parameter(Mandatory=$True,Position=2)]
   [string]$service,

   [Parameter(Mandatory=$True,Position=3)]
   [string]$projectName,

   [Parameter(Mandatory=$True,Position=4)]
   [string]$storageAccountName,

   [Parameter(Mandatory=$True,Position=5)]
   [string]$pathToPublishSettingsFile,


   [Parameter(Mandatory=$False,Position=6)]
   [string]$slot="staging",

   [Parameter(Mandatory=$False,Position=7)]
   [string]$buildConfigName="release",


   [Parameter(Mandatory=$False,Position=8)]
   [string]$timeStampFormat="g",


   [Parameter(Mandatory=$False,Position=9)]
   [string]$deploymentLabel="Continuous Deploy"

)


$package = "$($projectName)\bin\$($buildConfigName)\app.publish\$($projectName).cspkg"
$configuration =  "$($projectName)\bin\$($buildConfigName)\app.publish\ServiceConfiguration.Cloud.cscfg"
$errorKey = "POWERSHELL ERROR"

Write-Host "Running Azure Imports"
try {
  Import-Module "C:\Program Files (x86)\Microsoft SDKs\Azure\PowerShell\ResourceManager\AzureResourceManager\AzureRM.Profile\AzureRM.Profile.psd1"
  Import-Module "C:\Program Files (x86)\Microsoft SDKs\Azure\PowerShell\Storage\Azure.Storage\Azure.Storage.psd1"
  Import-Module "C:\Program Files (x86)\Microsoft SDKs\Azure\PowerShell\ServiceManagement\Azure\Azure.psd1"
} Catch {
  $ErrorMessage = $_.Exception.Message
  Write-Host $errorKey
  Write-Host "Error Importing Modules"
  Write-Host $ErrorMessage
  exit(1)
}


# import settings
Write-Host "Import Settings File"
try {
  Import-AzurePublishSettingsFile $pathToPublishSettingsFile
} Catch {
  $ErrorMessage = $_.Exception.Message
  Write-Host $errorKey
  Write-Host "Error Importing Publish Settings File"
  Write-Host $ErrorMessage
  exit(1)
}

#setup azure information
Write-Host "Setup Azure Subscription Information"
try {
  Set-AzureSubscription -CurrentStorageAccount $storageAccountName -SubscriptionName $subscription
} Catch {
  $ErrorMessage = $_.Exception.Message
  Write-Host $errorKey
  Write-Host "Error Setting Azure Subscription"
  Write-Host $ErrorMessage
  exit(1)
}


# DEFINE FUNCTIONS

function CreateNewDeployment()
{
    Write-Host -id 3 -activity "Creating New Deployment" -Status "In progress"
    Write-Host "$(Get-Date -f $timeStampFormat) - Creating New Deployment: In progress"

    $opstat = New-AzureDeployment -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service

    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid

    Write-Host -id 3 -activity "Creating New Deployment" -completed -Status "Complete"
    Write-Host "$(Get-Date -f $timeStampFormat) - Creating New Deployment: Complete, Deployment ID: $completeDeploymentID"
}

function UpgradeDeployment()
{
    Write-Host -id 3 -activity "Upgrading Deployment" -Status "In progress"
    Write-Host "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: In progress"

    # perform Update-Deployment
    $setdeployment = Set-AzureDeployment -Upgrade -Slot $slot -Package $package -Configuration $configuration -label $deploymentLabel -ServiceName $service -Force

    $completeDeployment = Get-AzureDeployment -ServiceName $service -Slot $slot
    $completeDeploymentID = $completeDeployment.deploymentid

    Write-Host -id 3 -activity "Upgrading Deployment" -completed -Status "Complete"
    Write-Host "$(Get-Date -f $timeStampFormat) - Upgrading Deployment: Complete, Deployment ID: $completeDeploymentID"
}


function Publish(){
 $deployment = Get-AzureDeployment -ServiceName $service -Slot $slot -ErrorVariable a -ErrorAction silentlycontinue
 Write-Host $deployment

 if ($a[0] -ne $null) {
    Write-Host "$(Get-Date -f $timeStampFormat) - No deployment is detected. Creating a new deployment. "
 }

 if ($deployment.Name -ne $null) {
    #Update deployment inplace (usually faster, cheaper, won't destroy VIP)
    Write-Host "$(Get-Date -f $timeStampFormat) - Deployment exists in $servicename.  Upgrading deployment."

    try {
      UpgradeDeployment
    } catch {
      $ErrorMessage = $_.Exception.Message
      Write-Host $errorKey
      Write-Host "Error Upgrading Deployment"
      Write-Host $ErrorMessage
      exit(1)
    }
 } else {
    try {
      CreateNewDeployment
    } catch {
      $ErrorMessage = $_.Exception.Message
      Write-Host $errorKey
      Write-Host "Error Upgrading Deployment"
      Write-Host $ErrorMessage
      exit(1)
    }
 }
}

Publish

Exit(0)
