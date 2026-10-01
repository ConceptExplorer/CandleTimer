# Stop execution on any error
$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  CandleTimer Automated Build Script    " -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan

# 1. Clean previous build artifacts
Write-Host "`n[1/4] Cleaning solution..." -ForegroundColor Yellow
dotnet clean CandleTimer.slnx --configuration Release --verbosity quiet

# 2. Build Debug configuration
Write-Host "`n[2/4] Building Debug build..." -ForegroundColor Yellow
dotnet build CandleTimer.slnx --configuration Debug --verbosity quiet
Write-Host "  ✓ Debug build succeeded." -ForegroundColor Green

# 3. Build standard Release configuration
Write-Host "`n[3/4] Building Release build..." -ForegroundColor Yellow
dotnet build CandleTimer.slnx --configuration Release --verbosity quiet
Write-Host "  ✓ Release build succeeded." -ForegroundColor Green

# 4. Publish Standalone Single-File Executable
Write-Host "`n[4/4] Publishing Standalone Single-File (win-x64)..." -ForegroundColor Yellow
dotnet publish CandleTimer.csproj `
    --configuration Release `
    --runtime win-x64 `
    --self-contained true `
    -p:PublishSingleFile=true `
    -p:IncludeNativeLibrariesForSelfExtract=true `
    --output ./artifacts/standalone-win-x64 `
    --verbosity quiet

Write-Host "  ✓ Standalone executable published!" -ForegroundColor Green

Write-Host "`n========================================" -ForegroundColor Cyan
Write-Host "  BUILD COMPLETE!" -ForegroundColor Green
Write-Host "  Output location: ./artifacts/standalone-win-x64/CandleTimer.exe" -ForegroundColor White
Write-Host "========================================" -ForegroundColor Cyan