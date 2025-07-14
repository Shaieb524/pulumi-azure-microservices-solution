# Post-Deployment ESC Update Script - APIM URLs
Write-Host "🌐 Post-Deployment: Updating ESC with APIM URLs" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

# Change to the parent directory to work with Pulumi
$parentDir = Split-Path $PSScriptRoot -Parent
Set-Location $parentDir
Write-Host "📁 Working directory: $parentDir" -ForegroundColor Cyan

# Read configuration from after-deploy-configs.txt
$configFile = Join-Path $PSScriptRoot "after-deploy-configs.txt"

if (-not (Test-Path $configFile)) {
    Write-Host "❌ Configuration file not found: $configFile" -ForegroundColor Red
    exit 1
}

Write-Host "📋 Reading configuration from: $configFile" -ForegroundColor Yellow

# Parse configuration file
$config = @{}
Get-Content $configFile | ForEach-Object {
    if ($_ -match '^(.+?)=(.+)$') {
        $key = $matches[1].Trim()
        $value = $matches[2].Trim()
        $config[$key] = $value
    }
}

# Validate required configuration values
$requiredKeys = @('PulumiOrganization', 'PulumiProject', 'PulumiEnv')
foreach ($key in $requiredKeys) {
    if (-not $config.ContainsKey($key) -or [string]::IsNullOrWhiteSpace($config[$key])) {
        Write-Host "❌ Missing or empty configuration value: $key" -ForegroundColor Red
        exit 1
    }
}

# Build environment name using config values
$orgName = $config.PulumiOrganization
$projectName = $config.PulumiProject
$envName = $config.PulumiEnv
$fullEnvName = "$orgName/$projectName/$envName"

# Ask for the stack name instead of hardcoding it
Write-Host "📋 Available Pulumi stacks:" -ForegroundColor Cyan
pulumi stack ls

# Prompt for stack name
$defaultStackName = "$($config.PulumiProject)-$($config.PulumiEnv)"
$stackName = Read-Host -Prompt "Enter Pulumi stack name (default: <clientname>-<env>)"

# Use default if empty
if ([string]::IsNullOrWhiteSpace($stackName)) {
    $stackName = $defaultStackName
    Write-Host "Using default stack name: $stackName" -ForegroundColor Yellow
}

Write-Host "✅ Configuration loaded successfully!" -ForegroundColor Green
Write-Host "   Environment Name: $fullEnvName" -ForegroundColor White
Write-Host "   Stack Name: $stackName" -ForegroundColor White

Write-Host ""
Write-Host "📊 Getting APIM outputs..." -ForegroundColor Yellow

# Get APIM outputs from the deployed stack
$apimGatewayUrl = (pulumi stack output ApiManagementGatewayUrl --stack $stackName 2>$null) | Select-Object -Last 1
$apimPortalUrl = (pulumi stack output ApiManagementPortalUrl --stack $stackName 2>$null) | Select-Object -Last 1
$apimName = (pulumi stack output ApiManagementName --stack $stackName 2>$null) | Select-Object -Last 1

if (-not $apimGatewayUrl -or -not $apimPortalUrl -or -not $apimName) {
    Write-Host ""
    Write-Host "🔍 Let's try listing all outputs for this stack:" -ForegroundColor Yellow
    pulumi stack output --stack $stackName
    Write-Host ""
    Write-Host "❌ APIM outputs not found!" -ForegroundColor Red
    Write-Host "   Make sure SharedResources with APIM is deployed successfully." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Found APIM outputs!" -ForegroundColor Green
Write-Host "   Gateway URL: $apimGatewayUrl" -ForegroundColor White
Write-Host "   Portal URL: $apimPortalUrl" -ForegroundColor White
Write-Host "   APIM Name: $apimName" -ForegroundColor White

Write-Host ""
Write-Host "🌍 Setting up ESC Environment..." -ForegroundColor Yellow

# Check if ESC environment exists, create if it doesn't
Write-Host "🔍 Checking if ESC environment exists: $fullEnvName" -ForegroundColor Cyan
$envExists = esc env get $fullEnvName 2>$null
if (-not $envExists) {
    Write-Host "📝 Creating ESC environment: $fullEnvName" -ForegroundColor Yellow
    esc env init $fullEnvName
    if ($LASTEXITCODE -eq 0) {
        Write-Host "✅ ESC environment created successfully!" -ForegroundColor Green
    } else {
        Write-Host "❌ Failed to create ESC environment!" -ForegroundColor Red
        exit 1
    }
} else {
    Write-Host "✅ ESC environment already exists!" -ForegroundColor Green
}

Write-Host ""
Write-Host "⚙️  Updating ESC environment with APIM URLs..." -ForegroundColor Yellow

# Update ESC with APIM URLs
esc env set $fullEnvName "pulumiConfig.SharedResources:ApiManagement.GatewayUrl" $apimGatewayUrl
esc env set $fullEnvName "pulumiConfig.SharedResources:ApiManagement.PortalUrl" $apimPortalUrl
esc env set $fullEnvName "pulumiConfig.SharedResources:ApiManagement.ServiceName" $apimName

Write-Host ""
Write-Host "✅ ESC Environment Updated with APIM URLs!" -ForegroundColor Green
Write-Host "🎯 Other projects can now use APIM URLs from ESC:" -ForegroundColor Green
Write-Host ""
Write-Host "Example usage in C#:" -ForegroundColor Yellow
Write-Host "   var config = new Config(`"SharedResources`");" -ForegroundColor White
Write-Host "   var apimGateway = config.Require(`"ApiManagement:GatewayUrl`");" -ForegroundColor White
Write-Host "   var apimPortal = config.Require(`"ApiManagement:PortalUrl`");" -ForegroundColor White
Write-Host "   var apimName = config.Require(`"ApiManagement:ServiceName`");" -ForegroundColor White
Write-Host ""
Write-Host "Example usage in Environment Variables:" -ForegroundColor Yellow
Write-Host "   APIM_GATEWAY_URL=$apimGatewayUrl" -ForegroundColor White
Write-Host "   APIM_PORTAL_URL=$apimPortalUrl" -ForegroundColor White
Write-Host "   APIM_SERVICE_NAME=$apimName" -ForegroundColor White