# PowerShell script for copying all repositories and tags between ACRs
param(
    [Parameter(Mandatory=$true)]
    [string]$sourceRegistry,
    
    [Parameter(Mandatory=$true)]
    [string]$targetRegistry,
    
    [Parameter(Mandatory=$false)]
    [switch]$whatIf = $false
)

Write-Host "=== ACR Repository Copy Script ===" -ForegroundColor Green
Write-Host "Source Registry: $sourceRegistry" -ForegroundColor Yellow
Write-Host "Target Registry: $targetRegistry" -ForegroundColor Yellow
Write-Host "What-If Mode: $whatIf" -ForegroundColor Yellow
Write-Host ""

# Get repositories from source registry
Write-Host "Getting repositories from source registry..." -ForegroundColor Cyan
try {
    $repositories = az acr repository list --name $sourceRegistry | ConvertFrom-Json
    Write-Host "Found $($repositories.Count) repositories" -ForegroundColor Green
} catch {
    Write-Error "Failed to get repositories from source registry: $_"
    exit 1
}

$totalTags = 0
$successfulCopies = 0
$failedCopies = 0

foreach ($repo in $repositories) {
    Write-Host ""
    Write-Host "Processing repository: $repo" -ForegroundColor Magenta
    
    try {
        # Get all tags for the repository
        $tags = az acr repository show-tags --name $sourceRegistry --repository $repo | ConvertFrom-Json
        Write-Host "  Found $($tags.Count) tags: $($tags -join ', ')" -ForegroundColor White
        $totalTags += $tags.Count
        
        # Copy each tag
        foreach ($tag in $tags) {
            $sourceImage = "$sourceRegistry.azurecr.io/$repo`:$tag"
            $targetImage = "$repo`:$tag"
            
            if ($whatIf) {
                Write-Host "  [WHAT-IF] Would copy: $sourceImage -> $targetImage" -ForegroundColor DarkYellow
                $successfulCopies++
            } else {
                Write-Host "  Copying: $sourceImage -> $targetImage" -ForegroundColor White
                
                try {
                    az acr import `
                        --name $targetRegistry `
                        --source $sourceImage `
                        --image $targetImage `
                        --output none
                    
                    Write-Host "    ✅ Success" -ForegroundColor Green
                    $successfulCopies++
                } catch {
                    Write-Host "    ❌ Failed: $_" -ForegroundColor Red
                    $failedCopies++
                }
            }
        }
    } catch {
        Write-Error "Failed to process repository $repo`: $_"
        $failedCopies += 1
    }
}

# Summary
Write-Host ""
Write-Host "=== Copy Summary ===" -ForegroundColor Green
Write-Host "Total repositories: $($repositories.Count)" -ForegroundColor White
Write-Host "Total tags: $totalTags" -ForegroundColor White
Write-Host "Successful copies: $successfulCopies" -ForegroundColor Green
Write-Host "Failed copies: $failedCopies" -ForegroundColor Red

if ($whatIf) {
    Write-Host ""
    Write-Host "This was a dry run. Use -whatIf:`$false to perform actual copy." -ForegroundColor Yellow
}