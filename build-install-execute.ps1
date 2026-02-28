$AndroidSdk = "C:\Program Files (x86)\Android\android-sdk"
$Emulator = "$AndroidSdk\emulator\emulator.exe"
$Adb = "$AndroidSdk\platform-tools\adb.exe"
$Avd = "pixel_7_-_api_34"
$Project = "FamilyFinance\FamilyFinance.csproj"
$Framework = "net8.0-android"

# Start emulator if not already running
$emulatorRunning = & $Adb devices 2>$null | Select-String "emulator"
if (-not $emulatorRunning) {
    Write-Host "Starting emulator '$Avd'..." -ForegroundColor Cyan
    Start-Process -FilePath $Emulator -ArgumentList "-avd", $Avd -WindowStyle Minimized

    Write-Host "Waiting for emulator to boot..." -ForegroundColor Yellow
    do {
        Start-Sleep -Seconds 2
        $bootAnim = & $Adb shell getprop sys.boot_completed 2>$null
    } while ($bootAnim -ne "1")

    Write-Host "Emulator ready." -ForegroundColor Green
} else {
    Write-Host "Emulator already running." -ForegroundColor Green
}

# Build, install and run
Write-Host "Building and deploying app..." -ForegroundColor Cyan
dotnet build $Project -f $Framework -t:Run -p:AndroidSdkDirectory="$AndroidSdk"

if ($LASTEXITCODE -eq 0) {
    Write-Host "App launched successfully." -ForegroundColor Green
} else {
    Write-Host "Build failed with exit code $LASTEXITCODE." -ForegroundColor Red
    exit $LASTEXITCODE
}
