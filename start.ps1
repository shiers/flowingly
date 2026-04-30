# Start the backend API and frontend dev server.
# If Windows Terminal (wt) is available, opens each server in a new tab.
# Otherwise falls back to separate windows.

$root    = $PSScriptRoot
$apiScript      = "$root\.vscode\run-api.ps1"
$frontendScript = "$root\.vscode\run-frontend.ps1"

if (Get-Command wt -ErrorAction SilentlyContinue) {
    Write-Host "Launching in Windows Terminal tabs..."
    wt -w 0 new-tab --title "API" powershell -NoExit -File $apiScript
    Start-Sleep -Milliseconds 500
    wt -w 0 new-tab --title "Frontend" powershell -NoExit -File $frontendScript
} else {
    Write-Host "Launching in separate windows..."
    Start-Process powershell -ArgumentList "-NoExit", "-File", $apiScript
    Start-Process powershell -ArgumentList "-NoExit", "-File", $frontendScript
}

Write-Host ""
Write-Host "  API:      http://localhost:5000"
Write-Host "  Swagger:  http://localhost:5000/swagger"
Write-Host "  Frontend: http://localhost:5173"
