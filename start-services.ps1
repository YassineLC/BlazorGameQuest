# Script de lancement des services BlazorGameQuest
# Démarre tous les microservices et le client Blazor en arrière-plan

Write-Host "Démarrage des services BlazorGameQuest..." -ForegroundColor Green

# Démarrer AuthenticationServices (port 5003)
Write-Host "Démarrage AuthenticationServices sur http://localhost:5003" -ForegroundColor Yellow
$authProcess = Start-Process powershell -ArgumentList "-WindowStyle", "Hidden", "-Command", "cd 'AuthenticationServices'; dotnet run" -PassThru

# Attendre 2 secondes
Start-Sleep -Seconds 2

# Démarrer GameService (port 5002)
Write-Host "Démarrage GameService sur http://localhost:5002" -ForegroundColor Yellow
$gameProcess = Start-Process powershell -ArgumentList "-WindowStyle", "Hidden", "-Command", "cd 'GameService'; dotnet run" -PassThru

# Attendre 2 secondes
Start-Sleep -Seconds 2

# Démarrer ApiGateway (port 5001)
Write-Host "Démarrage ApiGateway sur http://localhost:5001" -ForegroundColor Yellow
$apiProcess = Start-Process powershell -ArgumentList "-WindowStyle", "Hidden", "-Command", "cd 'ApiGateway'; dotnet run" -PassThru

# Attendre 3 secondes
Start-Sleep -Seconds 3

# Démarrer BlazorGame.Client (port 5000)
Write-Host "Démarrage BlazorGame.Client sur http://localhost:5000" -ForegroundColor Yellow
$clientProcess = Start-Process powershell -ArgumentList "-WindowStyle", "Hidden", "-Command", "cd 'BlazorGame.Client'; dotnet run" -PassThru

Write-Host ""
Write-Host "Tous les services sont en cours de démarrage..." -ForegroundColor Green
Write-Host "URLs disponibles:" -ForegroundColor Cyan
Write-Host "  - Jeu (Client): http://localhost:5000" -ForegroundColor White
Write-Host "  - API Gateway: http://localhost:5001" -ForegroundColor White
Write-Host "  - Game Service: http://localhost:5002" -ForegroundColor White
Write-Host "  - Auth Service: http://localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "Utilisez 'stop-services.ps1' pour arrêter tous les services" -ForegroundColor Yellow
Write-Host "Ou utilisez Get-Process -Name 'dotnet' pour voir les processus actifs" -ForegroundColor Yellow

# Garder le script ouvert pour afficher les informations
try {
    Write-Host "Appuyez sur Ctrl+C pour fermer ce script (les services continueront)" -ForegroundColor Cyan
    while ($true) { Start-Sleep -Seconds 1 }
}
catch [System.Management.Automation.PipelineStoppedException] {
    Write-Host "Script arrêté. Les services continuent de fonctionner en arrière-plan." -ForegroundColor Green
}