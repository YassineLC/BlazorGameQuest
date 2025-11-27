# Script de lancement des services BlazorGameQuest
# Demarre tous les microservices et le client Blazor en arriere-plan

Write-Host "Demarrage des services BlazorGameQuest..." -ForegroundColor Green
Write-Host "Verification des prerequis..." -ForegroundColor Cyan

# Verifier que .NET est installe
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    Write-Host "ERREUR: .NET n'est pas installe ou n'est pas dans le PATH" -ForegroundColor Red
    exit 1
}

# Arreter tous les processus dotnet existants
Write-Host "Arret des processus dotnet existants..." -ForegroundColor Yellow
Get-Process -Name "dotnet" -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue

# Fonction pour demarrer un service et verifier qu'il demarre
function Start-Service {
    param (
        [string]$ServiceName,
        [string]$Directory,
        [int]$Port
    )
    
    Write-Host "Demarrage $ServiceName sur http://localhost:$Port" -ForegroundColor Yellow
    
    # Changer vers le repertoire du service et demarrer
    $process = Start-Process powershell -ArgumentList "-WindowStyle", "Minimized", "-Command", "cd '$Directory'; dotnet run --urls http://localhost:$Port" -PassThru
    
    # Attendre un moment pour que le service demarre
    Start-Sleep -Seconds 3
    
    # Verifier si le processus est toujours actif
    if ($process.HasExited) {
        Write-Host "ERREUR: $ServiceName n'a pas pu demarrer" -ForegroundColor Red
        return $false
    }
    
    Write-Host "$ServiceName demarre avec succes (PID: $($process.Id))" -ForegroundColor Green
    return $process
}

# Demarrer les services dans l'ordre
$authProcess = Start-Service -ServiceName "AuthenticationServices" -Directory "src/AuthenticationServices" -Port 5003
if (-not $authProcess) { exit 1 }

$gameProcess = Start-Service -ServiceName "GameService" -Directory "src/GameService" -Port 5002
if (-not $gameProcess) { exit 1 }

$apiProcess = Start-Service -ServiceName "ApiGateway" -Directory "src/ApiGateway" -Port 5001
if (-not $apiProcess) { exit 1 }

# Attendre un peu plus pour l'API Gateway
Start-Sleep -Seconds 2

# Demarrer le client Blazor
Write-Host "Demarrage BlazorGame.Client sur http://localhost:5000" -ForegroundColor Yellow
$clientProcess = Start-Process powershell -ArgumentList "-WindowStyle", "Minimized", "-Command", "cd 'src/BlazorGame.Client'; dotnet run --urls http://localhost:5000" -PassThru

Start-Sleep -Seconds 3

if ($clientProcess.HasExited) {
    Write-Host "ERREUR: BlazorGame.Client n'a pas pu demarrer" -ForegroundColor Red
    exit 1
}

Write-Host "BlazorGame.Client demarre avec succes (PID: $($clientProcess.Id))" -ForegroundColor Green

# Attendre que tous les services soient prets
Write-Host "Attente que tous les services soient prets..." -ForegroundColor Cyan
Start-Sleep -Seconds 5

Write-Host ""
Write-Host "Tous les services sont en cours de demarrage..." -ForegroundColor Green
Write-Host "URLs disponibles:" -ForegroundColor Cyan
Write-Host "  - Jeu (Client): http://localhost:5000" -ForegroundColor White
Write-Host "  - API Gateway: http://localhost:5001" -ForegroundColor White
Write-Host "  - Game Service: http://localhost:5002" -ForegroundColor White
Write-Host "  - Auth Service: http://localhost:5003" -ForegroundColor White
Write-Host ""
Write-Host "Utilisez 'stop-services.ps1' pour arreter tous les services" -ForegroundColor Yellow
Write-Host "Ou utilisez Get-Process -Name 'dotnet' pour voir les processus actifs" -ForegroundColor Yellow

# Garder le script ouvert pour afficher les informations
try {
    Write-Host "Appuyez sur Ctrl+C pour fermer ce script (les services continueront)" -ForegroundColor Cyan
    while ($true) { Start-Sleep -Seconds 1 }
}
catch [System.Management.Automation.PipelineStoppedException] {
    Write-Host "Script arrete. Les services continuent de fonctionner en arriere-plan." -ForegroundColor Green
}