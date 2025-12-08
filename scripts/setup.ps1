# Entra ID Auth Labs - Setup Script
# This script helps configure the application with your Azure AD settings

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Entra ID Auth Labs - Setup Script" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if .NET 8 SDK is installed
Write-Host "Checking prerequisites..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK found: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ .NET 8 SDK not found. Please install from: https://dotnet.microsoft.com/download/dotnet/8.0" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 1: Azure App Registration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Before proceeding, create an app registration in Azure Portal:" -ForegroundColor Yellow
Write-Host "1. Go to https://portal.azure.com" -ForegroundColor White
Write-Host "2. Navigate to Microsoft Entra ID → App registrations" -ForegroundColor White
Write-Host "3. Click 'New registration'" -ForegroundColor White
Write-Host "4. Name: 'Entra ID Auth Labs' (or your choice)" -ForegroundColor White
Write-Host "5. Supported account types: 'Accounts in this organizational directory only'" -ForegroundColor White
Write-Host "6. Redirect URI:" -ForegroundColor White
Write-Host "   - Platform: Web" -ForegroundColor White
Write-Host "   - URI: https://localhost:7001/signin-oidc" -ForegroundColor Green
Write-Host "7. Click 'Register'" -ForegroundColor White
Write-Host ""

$continue = Read-Host "Have you created the app registration? (y/n)"
if ($continue -ne 'y') {
    Write-Host "Please create the app registration first, then run this script again." -ForegroundColor Yellow
    exit 0
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 2: Collect Configuration Values" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Collect Tenant ID
Write-Host "From your app's Overview page, copy the following:" -ForegroundColor Yellow
Write-Host ""
$tenantId = Read-Host "Directory (tenant) ID"
if ([string]::IsNullOrWhiteSpace($tenantId)) {
    Write-Host "✗ Tenant ID is required" -ForegroundColor Red
    exit 1
}

# Collect Client ID
$clientId = Read-Host "Application (client) ID"
if ([string]::IsNullOrWhiteSpace($clientId)) {
    Write-Host "✗ Client ID is required" -ForegroundColor Red
    exit 1
}

# Collect Domain (optional, can be derived)
Write-Host ""
Write-Host "Your tenant domain (e.g., contoso.onmicrosoft.com):" -ForegroundColor Yellow
$domain = Read-Host "Domain (press Enter to auto-detect)"

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 3: Update Configuration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Path to appsettings.json
$appSettingsPath = Join-Path $PSScriptRoot "..\src\WebAuthzDemo\appsettings.json"

if (-not (Test-Path $appSettingsPath)) {
    Write-Host "✗ appsettings.json not found at: $appSettingsPath" -ForegroundColor Red
    exit 1
}

# Read existing appsettings.json
$appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json

# Update values
$appSettings.AzureAd.TenantId = $tenantId
$appSettings.AzureAd.ClientId = $clientId

if (-not [string]::IsNullOrWhiteSpace($domain)) {
    $appSettings.AzureAd.Domain = $domain
}

# Write updated appsettings.json
$appSettings | ConvertTo-Json -Depth 10 | Set-Content $appSettingsPath

Write-Host "✓ Configuration updated successfully!" -ForegroundColor Green
Write-Host ""

# Display configuration summary
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Configuration Summary" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Tenant ID:        $tenantId" -ForegroundColor White
Write-Host "Client ID:        $clientId" -ForegroundColor White
Write-Host "Domain:           $($appSettings.AzureAd.Domain)" -ForegroundColor White
Write-Host "Redirect URI:     https://localhost:7001/signin-oidc" -ForegroundColor Green
Write-Host ""

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 4: Verify Redirect URI" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "IMPORTANT: Verify the following redirect URI is configured in Azure:" -ForegroundColor Yellow
Write-Host ""
Write-Host "  https://localhost:7001/signin-oidc" -ForegroundColor Green
Write-Host ""
Write-Host "If not configured:" -ForegroundColor Yellow
Write-Host "1. Go to Azure Portal → Your app → Authentication" -ForegroundColor White
Write-Host "2. Under 'Web' platform, add the above URI" -ForegroundColor White
Write-Host "3. Click 'Save'" -ForegroundColor White
Write-Host ""

$redirectOk = Read-Host "Is the redirect URI configured? (y/n)"
if ($redirectOk -ne 'y') {
    Write-Host ""
    Write-Host "⚠ Please configure the redirect URI before running the application." -ForegroundColor Yellow
    Write-Host ""
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Step 5: Build and Run" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$buildAndRun = Read-Host "Would you like to build and run the application now? (y/n)"

if ($buildAndRun -eq 'y') {
    Write-Host ""
    Write-Host "Building solution..." -ForegroundColor Yellow
    
    $solutionPath = Join-Path $PSScriptRoot ".."
    Push-Location $solutionPath
    
    try {
        dotnet restore
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Restore failed" -ForegroundColor Red
            Pop-Location
            exit 1
        }
        
        dotnet build
        if ($LASTEXITCODE -ne 0) {
            Write-Host "✗ Build failed" -ForegroundColor Red
            Pop-Location
            exit 1
        }
        
        Write-Host ""
        Write-Host "✓ Build successful!" -ForegroundColor Green
        Write-Host ""
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host "Starting Application..." -ForegroundColor Cyan
        Write-Host "========================================" -ForegroundColor Cyan
        Write-Host ""
        Write-Host "The application will start on: https://localhost:7001" -ForegroundColor Green
        Write-Host ""
        Write-Host "Press Ctrl+C to stop the application" -ForegroundColor Yellow
        Write-Host ""
        
        $projectPath = Join-Path $solutionPath "src\WebAuthzDemo"
        Set-Location $projectPath
        
        # Run the application
        dotnet run
        
    } finally {
        Pop-Location
    }
} else {
    Write-Host ""
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host "Next Steps" -ForegroundColor Cyan
    Write-Host "========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To run the application manually:" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  cd src\WebAuthzDemo" -ForegroundColor White
    Write-Host "  dotnet run" -ForegroundColor White
    Write-Host ""
    Write-Host "Then open your browser to: https://localhost:7001" -ForegroundColor Green
    Write-Host ""
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Additional Resources" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "📖 Main README:     docs\README.md" -ForegroundColor White
Write-Host "📚 Lab 1 (Auth):    docs\Lab1_Authentication.md" -ForegroundColor White
Write-Host "📚 Lab 2 (Authz):   docs\Lab2_SimpleAuthorization.md" -ForegroundColor White
Write-Host ""
Write-Host "Happy learning! 🎉" -ForegroundColor Cyan
Write-Host ""
