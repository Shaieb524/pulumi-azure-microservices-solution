# Setup Pulumi ESC Environment
Write-Host "🌍 Setting up Pulumi ESC Environment" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green

# Prompt for config file path
do {
    $configFilePath = Read-Host "📂 Enter the path to client-env-configs file"
    if (-not (Test-Path $configFilePath)) {
        Write-Host "❌ Config file not found at: $configFilePath" -ForegroundColor Red
        $configFilePath = $null
    }
} while (-not $configFilePath)

Write-Host "📋 Reading configuration from: $configFilePath" -ForegroundColor Cyan

# Read and parse config file
$config = @{}
try {
    Get-Content $configFilePath | ForEach-Object {
        if ($_ -match '^([^=]+)=(.*)$') {
            $key = $matches[1].Trim()
            $value = $matches[2].Trim()
            $config[$key] = $value
        }
    }
    Write-Host "✅ Configuration loaded successfully" -ForegroundColor Green
} catch {
    Write-Host "❌ Error reading config file: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Display loaded configuration
Write-Host "📄 Loaded Configuration:" -ForegroundColor Yellow
$config.GetEnumerator() | Sort-Object Name | ForEach-Object {
    Write-Host "   $($_.Key) = $($_.Value)" -ForegroundColor White
}
Write-Host ""

# Check if esc CLI is installed
if (-not (Get-Command esc -ErrorAction SilentlyContinue)) {
    Write-Host "❌ ESC CLI is not installed. Please install it first." -ForegroundColor Red
    Write-Host "   Install with: pulumi shell script" -ForegroundColor Yellow
    exit 1
}

# Build environment name using config values
$orgName = $config.PulumiOrganization
$projectName = $config.PulumiProject
$envName = $config.PulumiEnv
$fullEnvName = "$orgName/$projectName/$envName"

Write-Host "📝 Creating ESC environment: $fullEnvName..." -ForegroundColor Yellow

# Create the ESC environment
esc env init $fullEnvName

Write-Host "⚙️  Setting configuration values for $($config.ClientName) ($($config.Environment))..." -ForegroundColor Yellow

# Set basic configuration under pulumiConfig for Pulumi stacks
esc env set $fullEnvName "pulumiConfig.azure-native:location" $config.Location
esc env set $fullEnvName "pulumiConfig.SharedResources:Location" $config.Location
esc env set $fullEnvName "pulumiConfig.SharedResources:Prefix" $config.Prefix
esc env set $fullEnvName "pulumiConfig.SharedResources:Environment" $config.Environment
esc env set $fullEnvName "pulumiConfig.SharedResources:Client" $config.ClientName
esc env set $fullEnvName "pulumiConfig.SharedResources:DomainName" $config.DomainName

# Set tags under pulumiConfig using config values
esc env set $fullEnvName "pulumiConfig.SharedResources:Tags.Environment" $config.'Tags.Environment'
esc env set $fullEnvName "pulumiConfig.SharedResources:Tags.Owner" $config.'Tags.Owner'
esc env set $fullEnvName "pulumiConfig.SharedResources:Tags.Client" $config.'Tags.Client'
esc env set $fullEnvName "pulumiConfig.SharedResources:Tags.Project" $config.'Tags.Project'
esc env set $fullEnvName "pulumiConfig.SharedResources:Tags.ManagedBy" $config.'Tags.ManagedBy'

# Set app service plan SKU using config values
esc env set $fullEnvName "pulumiConfig.SharedResources:PlanSku.Capacity" $config.'PlanSku.Capacity'
esc env set $fullEnvName "pulumiConfig.SharedResources:PlanSku.Family" $config.'PlanSku.Family'
esc env set $fullEnvName "pulumiConfig.SharedResources:PlanSku.Name" $config.'PlanSku.Name'
esc env set $fullEnvName "pulumiConfig.SharedResources:PlanSku.Size" $config.'PlanSku.Size'
esc env set $fullEnvName "pulumiConfig.SharedResources:PlanSku.Tier" $config.'PlanSku.Tier'

# Also set regular values for other uses (like environment variables)
esc env set $fullEnvName "azure-native:location" $config.Location

# Set additional environment-specific values
esc env set $fullEnvName "clientName" $config.ClientName
esc env set $fullEnvName "environment" $config.Environment
esc env set $fullEnvName "prefix" $config.Prefix
esc env set $fullEnvName "domainName" $config.DomainName

Write-Host ""
Write-Host "✅ ESC Environment Setup Complete!" -ForegroundColor Green
Write-Host "=================================" -ForegroundColor Green
Write-Host ""
Write-Host "📋 What was created:" -ForegroundColor Yellow
Write-Host "   🌍 Environment: $fullEnvName" -ForegroundColor White
Write-Host "   🌐 Location: $($config.Location)" -ForegroundColor White
Write-Host "   🏢 Client: $($config.ClientName)" -ForegroundColor White
Write-Host "   🏷️  Environment: $($config.Environment)" -ForegroundColor White
Write-Host "   🔗 Domain: $($config.DomainName)" -ForegroundColor White
Write-Host "   ⚙️  pulumiConfig section for Pulumi stacks" -ForegroundColor White
Write-Host ""
Write-Host "🧪 Testing configuration access..." -ForegroundColor Yellow
$location = pulumi config get SharedResources:Location 2>$null
if ($location) {
    Write-Host "✅ Pulumi config access working! Location: $location" -ForegroundColor Green
} else {
    Write-Host "ℹ️  Run 'pulumi up' to test stack integration" -ForegroundColor Blue
}
Write-Host ""
Write-Host "🔍 View the environment:" -ForegroundColor Green
Write-Host "   esc env open $fullEnvName" -ForegroundColor White
Write-Host ""
Write-Host "🚀 Next steps:" -ForegroundColor Green
Write-Host "   1. Review the ESC environment configuration" -ForegroundColor White
Write-Host "   2. Import this environment in your Pulumi stack YAML" -ForegroundColor White
Write-Host "   3. Run 'pulumi up' to deploy with the new configuration" -ForegroundColor White