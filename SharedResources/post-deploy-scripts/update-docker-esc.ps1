# Post-Deployment ESC Update Script
Write-Host "🚀 Post-Deployment: Updating ESC with Docker Settings" -ForegroundColor Green
Write-Host "====================================================" -ForegroundColor Green

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
$stackName = Read-Host -Prompt "Enter Pulumi stack name (default: $defaultStackName)"

# Use default if empty
if ([string]::IsNullOrWhiteSpace($stackName)) {
    $stackName = $defaultStackName
    Write-Host "Using default stack name: $stackName" -ForegroundColor Yellow
}

Write-Host "✅ Configuration loaded successfully!" -ForegroundColor Green
Write-Host "   Environment Name: $fullEnvName" -ForegroundColor White
Write-Host "   Stack Name: $stackName" -ForegroundColor White

Write-Host ""
Write-Host "📊 Getting stack outputs..." -ForegroundColor Yellow

# Debug: List available stacks first
Write-Host "🔍 Available stacks:" -ForegroundColor Cyan
pulumi stack ls

Write-Host ""
Write-Host "🔍 Trying to get outputs from stack: $stackName" -ForegroundColor Cyan

# Get Container Registry outputs from the deployed stack
$loginServer = pulumi stack output ContainerRegistryLoginServer --stack $stackName 2>$null
$username = pulumi stack output ContainerRegistryUsername --stack $stackName 2>$null
$password = pulumi stack output ContainerRegistryPassword --stack $stackName 2>$null

# Debug: Show what we got
Write-Host "🔍 Debug - Retrieved values:" -ForegroundColor Cyan
Write-Host "   Login Server: '$loginServer'" -ForegroundColor White
Write-Host "   Username: '$username'" -ForegroundColor White
Write-Host "   Password: $(if ($password) { '[FOUND]' } else { '[NOT FOUND]' })" -ForegroundColor White

if (-not $loginServer -or -not $username -or -not $password) {
    Write-Host ""
    Write-Host "🔍 Let's try listing all outputs for this stack:" -ForegroundColor Yellow
    pulumi stack output --stack $stackName
    Write-Host ""
    Write-Host "❌ Container Registry outputs not found!" -ForegroundColor Red
    Write-Host "   Make sure SharedResources is deployed successfully." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Found Container Registry outputs!" -ForegroundColor Green
Write-Host "   Login Server: $loginServer" -ForegroundColor White
Write-Host "   Username: $username" -ForegroundColor White

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
Write-Host "⚙️  Updating ESC environment with Docker settings..." -ForegroundColor Yellow

# Update ESC with stack references
esc env set $fullEnvName "pulumiConfig.SharedResources:DockerSettings.DockerRegistryUrl" $loginServer
esc env set $fullEnvName "pulumiConfig.SharedResources:DockerSettings.DockerRegistryUserName" $username
esc env set --secret $fullEnvName "pulumiConfig.SharedResources:DockerSettings.DockerRegistryPassword" $password

Write-Host ""
Write-Host "✅ ESC Environment Updated!" -ForegroundColor Green
Write-Host "🎯 Other projects can now use Docker settings from ESC:" -ForegroundColor Green
Write-Host "   config.Require(`"DockerSettings:DockerRegistryUrl`")" -ForegroundColor White
Write-Host "   config.Require(`"DockerSettings:DockerRegistryUserName`")" -ForegroundColor White
Write-Host "   config.RequireSecret(`"DockerSettings:DockerRegistryPassword`")" -ForegroundColor White