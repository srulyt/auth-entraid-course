# Publish script for Labs.Cli
# This script publishes the CLI tool to the ./Labs.Cli/publish directory

Write-Host "Publishing Labs.Cli..." -ForegroundColor Cyan

# Navigate to the Labs.Cli directory
Set-Location -Path "$PSScriptRoot/Labs.Cli"

# Clean previous publish output
if (Test-Path "./publish") {
    Remove-Item -Path "./publish" -Recurse -Force
    Write-Host "Cleaned previous publish output" -ForegroundColor Yellow
}

# Publish the application
dotnet publish -c Release -o ./publish

$publishExitCode = $LASTEXITCODE

# Navigate back to the Labs.Cli directory
Set-Location -Path "$PSScriptRoot"

if ($publishExitCode -eq 0) {
    Write-Host "`nPublish successful!" -ForegroundColor Green
    Write-Host "`nPublished to: $PWD\publish" -ForegroundColor Yellow
    Write-Host "`nTo add the CLI to your PATH for this terminal session:" -ForegroundColor Cyan
    Write-Host "  `$env:PATH = `"`$PWD\Labs.Cli\publish;`$env:PATH`"" -ForegroundColor White
    Write-Host "`nThen you can use:" -ForegroundColor Cyan
    Write-Host "  entra-lab --help" -ForegroundColor White
    Write-Host "  entra-lab login --mode pkce" -ForegroundColor White
} else {
    Write-Host "`nPublish failed!" -ForegroundColor Red
    exit 1
}
