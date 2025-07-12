# SharedResources - Set Secrets Script
Write-Host "🚀 Setting up SharedResources Secrets" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green

# Check if pulumi is installed
if (-not (Get-Command pulumi -ErrorAction SilentlyContinue)) {
    Write-Host "❌ Pulumi CLI is not installed. Please install it first." -ForegroundColor Red
    exit 1
}

# Find the Pulumi project directory
$currentDir = Get-Location
$pulumiProjectDir = $null

# Check current directory first
if (Test-Path "Pulumi.yaml") {
    $pulumiProjectDir = $currentDir
    Write-Host "📁 Found Pulumi project in current directory" -ForegroundColor Cyan
}
# Check parent directory
elseif (Test-Path "../Pulumi.yaml") {
    $pulumiProjectDir = Split-Path $currentDir -Parent
    Write-Host "📁 Found Pulumi project in parent directory: $pulumiProjectDir" -ForegroundColor Cyan
}
else {
    Write-Host "❌ Not in a Pulumi project directory or its subdirectory." -ForegroundColor Red
    Write-Host "   Please run this from your project root or scripts folder." -ForegroundColor Yellow
    exit 1
}

# Change to the Pulumi project directory
Push-Location $pulumiProjectDir

Write-Host "🔐 Setting up encrypted secrets..." -ForegroundColor Yellow
Write-Host "Using format: SharedResources:SecretName" -ForegroundColor Yellow

try {
    # SQL Server Secrets
    Write-Host "Setting SQL Server secrets..." -ForegroundColor Cyan
    pulumi config set --secret SharedResources:SqlAdminPassword 'anypassword1234' --secret

    Write-Host ""
    Write-Host "✅ Secrets Setup Complete!" -ForegroundColor Green
    Write-Host "=========================" -ForegroundColor Green
    Write-Host ""
    Write-Host "📝 Remember to replace the placeholder values with your actual secrets:" -ForegroundColor Yellow
    Write-Host "   - SQL Admin Password (for your database server admin user)" -ForegroundColor White
    Write-Host ""
    Write-Host "🏗️  Ready to deploy:" -ForegroundColor Green
    Write-Host "   1. Update secret values above with your actual values" -ForegroundColor White
    Write-Host "   2. dotnet restore" -ForegroundColor White
    Write-Host "   3. pulumi up" -ForegroundColor White
    Write-Host ""
    Write-Host "🎉 Your SharedResources infrastructure is ready!" -ForegroundColor Green
}
finally {
    # Return to original directory
    Pop-Location
}