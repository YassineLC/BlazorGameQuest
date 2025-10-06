# Script d'arrêt des services BlazorGameQuest
# Arrête tous les processus dotnet et ferme les fenêtres PowerShell associées

Write-Host "Arrêt des services BlazorGameQuest..." -ForegroundColor Red

# Lister d'abord les processus dotnet actifs
$dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
if ($dotnetProcesses) {
    Write-Host "Processus dotnet trouvés:" -ForegroundColor Yellow
    foreach ($process in $dotnetProcesses) {
        Write-Host "  - PID: $($process.Id) - Démarré: $($process.StartTime)" -ForegroundColor Gray
    }
    Write-Host ""
    
    Write-Host "Arrêt de $($dotnetProcesses.Count) processus dotnet..." -ForegroundColor Yellow
    $dotnetProcesses | Stop-Process -Force
    
    # Vérifier que tous les processus sont arrêtés
    Start-Sleep -Seconds 2
    $remainingProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    if ($remainingProcesses) {
        Write-Host "Attention: $($remainingProcesses.Count) processus dotnet encore actifs" -ForegroundColor Red
    } else {
        Write-Host "Tous les processus dotnet ont été arrêtés avec succès." -ForegroundColor Green
    }
} else {
    Write-Host "Aucun processus dotnet en cours d'exécution." -ForegroundColor Yellow
}

# Fermer les fenêtres PowerShell qui pourraient exécuter des services
Write-Host ""
Write-Host "Recherche et fermeture des fenêtres PowerShell des services..." -ForegroundColor Yellow

$powershellProcesses = Get-Process -Name "powershell" -ErrorAction SilentlyContinue | Where-Object { 
    $_.Id -ne $PID # Exclure le processus PowerShell actuel
}

if ($powershellProcesses) {
    Write-Host "Fenêtres PowerShell trouvées (hors script actuel):" -ForegroundColor Yellow
    foreach ($process in $powershellProcesses) {
        try {
            # Essayer d'obtenir le titre de la fenêtre ou la ligne de commande
            $processInfo = Get-WmiObject Win32_Process -Filter "ProcessId = '$($process.Id)'" -ErrorAction SilentlyContinue
            $commandLine = if ($processInfo) { $processInfo.CommandLine } else { "N/A" }
            Write-Host "  - PID: $($process.Id) - Commande: $commandLine" -ForegroundColor Gray
        }
        catch {
            Write-Host "  - PID: $($process.Id) - Détails non disponibles" -ForegroundColor Gray
        }
    }
    
    Write-Host ""
    Write-Host "Fermeture de $($powershellProcesses.Count) fenêtre(s) PowerShell..." -ForegroundColor Yellow
    $powershellProcesses | Stop-Process -Force
    Write-Host "Fenêtres PowerShell fermées." -ForegroundColor Green
} else {
    Write-Host "Aucune autre fenêtre PowerShell trouvée." -ForegroundColor Yellow
}

# Optionnel: Fermer aussi les fenêtres cmd si nécessaire
$cmdProcesses = Get-Process -Name "cmd" -ErrorAction SilentlyContinue
if ($cmdProcesses) {
    Write-Host ""
    Write-Host "Fenêtres CMD trouvées:" -ForegroundColor Yellow
    foreach ($process in $cmdProcesses) {
        Write-Host "  - PID: $($process.Id)" -ForegroundColor Gray
    }
    
    $response = Read-Host "Voulez-vous également fermer les fenêtres CMD? (o/N)"
    if ($response -eq "o" -or $response -eq "O" -or $response -eq "oui") {
        $cmdProcesses | Stop-Process -Force
        Write-Host "Fenêtres CMD fermées." -ForegroundColor Green
    }
}

Write-Host ""
Write-Host "Nettoyage terminé." -ForegroundColor Green
Write-Host "Vous pouvez maintenant relancer les services avec 'start-services.ps1'" -ForegroundColor Cyan