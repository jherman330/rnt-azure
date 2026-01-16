# Development startup script
# Starts both the backend API and frontend web app, then opens the browser

Write-Host "Starting development environment..." -ForegroundColor Green

# Start the ASP.NET Core backend in a new window
Write-Host "Starting backend API (http://localhost:3100)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\api'; dotnet run --launch-profile net"

# Give the backend a moment to start
Start-Sleep -Seconds 2

# Start the Vite frontend in a new window
Write-Host "Starting frontend (http://localhost:5173)..." -ForegroundColor Yellow
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\src\web'; npm run dev"

# Wait a bit for the frontend to start, then open the browser
Start-Sleep -Seconds 3
Write-Host "Opening browser to http://localhost:5173..." -ForegroundColor Yellow
Start-Process "http://localhost:5173"

Write-Host "`nDevelopment servers are starting in separate windows." -ForegroundColor Green
Write-Host "Close those windows to stop the servers." -ForegroundColor Gray
