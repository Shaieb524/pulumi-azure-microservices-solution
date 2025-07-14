# Post-Deployment ESC Update Script - SQL Connection Strings
Write-Host "🗄️ Post-Deployment: Updating ESC with SQL Connection Strings" -ForegroundColor Green
Write-Host "=============================================================" -ForegroundColor Green

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
Write-Host "📊 Getting SQL Server outputs..." -ForegroundColor Yellow

# Get SQL Server outputs from the deployed stack (suppress warnings)
$sqlServerFqdn = (pulumi stack output SqlServerFqdn --stack $stackName 2>$null) | Select-Object -Last 1
$sqlServerName = (pulumi stack output SqlServerName --stack $stackName 2>$null) | Select-Object -Last 1

if (-not $sqlServerFqdn -or -not $sqlServerName) {
    Write-Host "❌ SQL Server outputs not found!" -ForegroundColor Red
    Write-Host "   Make sure SharedResources is deployed successfully." -ForegroundColor Yellow
    exit 1
}

Write-Host "✅ Found SQL Server outputs!" -ForegroundColor Green
Write-Host "   SQL Server FQDN: $sqlServerFqdn" -ForegroundColor White
Write-Host "   SQL Server Name: $sqlServerName" -ForegroundColor White

Write-Host ""
Write-Host "🔑 Getting SQL Admin Password..." -ForegroundColor Yellow

# Try to get SQL Admin Password - Pulumi will prompt if it's a secret
Write-Host "   Note: You may be prompted to enter the SQL password..." -ForegroundColor Cyan
$sqlPassword = (pulumi config get SharedResources:SqlAdminPassword --stack $stackName 2>$null) | Select-Object -Last 1

if (-not $sqlPassword -or $sqlPassword -eq "[secret]") {
    Write-Host "⚠️  Please enter the SQL Admin Password manually:" -ForegroundColor Yellow
    $sqlPasswordSecure = Read-Host -AsSecureString "SQL Admin Password"
    $sqlPassword = [Runtime.InteropServices.Marshal]::PtrToStringAuto([Runtime.InteropServices.Marshal]::SecureStringToBSTR($sqlPasswordSecure))
}

if (-not $sqlPassword) {
    Write-Host "❌ SQL Admin Password is required!" -ForegroundColor Red
    exit 1
}

Write-Host "✅ Found SQL Admin Password!" -ForegroundColor Green

Write-Host ""
Write-Host "📋 Getting database list from config..." -ForegroundColor Yellow

# Database names from your configuration
$databases = @(
    "App1Db",
    "App2Db"
)

Write-Host "✅ Found $($databases.Count) databases to configure" -ForegroundColor Green

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
Write-Host "⚙️  Updating ESC environment with SQL connection strings..." -ForegroundColor Yellow

# SQL connection details
$sqlUserId = "sqladmin"  # From your SQL server configuration

# Create and set connection string for each database
foreach ($dbName in $databases) {
    $connectionString = "Server=tcp:$sqlServerFqdn,1433;Database=$dbName;User ID=$sqlUserId;Password=$sqlPassword;Encrypt=true;TrustServerCertificate=false;Connection Timeout=30;"
    
    # Set connection string in ESC as secret
    $escKey = "pulumiConfig.SharedResources:ConnectionStrings.$dbName"
    Write-Host "   Setting: $dbName" -ForegroundColor Cyan
    
    esc env set --secret $fullEnvName $escKey $connectionString
}

Write-Host ""
Write-Host "✅ ESC Environment Updated with SQL Connection Strings!" -ForegroundColor Green
Write-Host "🎯 Other projects can now use SQL connection strings from ESC:" -ForegroundColor Green
Write-Host ""
Write-Host "Example usage in C#:" -ForegroundColor Yellow
Write-Host "   var config = new Config(`"SharedResources`");" -ForegroundColor White
Write-Host "   var app1DbConn = config.RequireSecret(`"ConnectionStrings:App1Db`");" -ForegroundColor White
Write-Host "   var app2DbConn= config.RequireSecret(`"ConnectionStrings:App1Db`");" -ForegroundColor White
Write-Host "   // ... etc for all databases" -ForegroundColor White