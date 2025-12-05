# BlazorGameQuest - Yassine LAHMAR CHERIF & Louisa MAIBECHE

Un jeu d'aventure textuel développé avec Blazor WebAssembly

### Prérequis

- .NET 9.0 SDK
- Docker Desktop

## Architecture

### Microservices

- **BlazorGame.Client** - Interface utilisateur Blazor
- **ApiGateway** - Point d'entrée unique pour toutes les API
- **GameService** - Logique de jeu, génération de donjons, calcul des scores
- **AuthenticationServices** - Gestion de l'authentification (sera intégré avec Keycloak)
- **SharedModels** - Modèles de données partagés entre tous les services

## Structure du Projet

```
BlazorGameQuest/
├── src/
│   ├── ApiGateway/               # API Gateway (http://localhost:5001)
│   ├── AuthenticationServices/   # Service d'authentification (http://localhost:5003)
│   ├── BlazorGame.Client/        # Interface Blazor (http://localhost:5000)
│   ├── GameService/              # Service de jeu (http://localhost:5002)
│   └── SharedModels/             # Modèles partagés
├── tests/
│   ├── ApiGateway.Tests/
│   ├── BlazorGame.Client.Tests/
│   └── SharedModels.Tests/
├── docker-compose.yml            # Orchestration Docker
├── .dockerignore                 # Ignore Docker
└── appsettings.Global.json       # Configuration globale
```

## Démarrage Rapide

### Lancement avec Docker

```powershell
# Depuis la racine du projet
docker-compose up --build -d

# Vérifier l'état
docker-compose ps

# Voir les logs
docker-compose logs -f

# Arrêter
docker-compose down
```

### Lancement manuel (sans Docker)

```powershell
# Terminal 1 - AuthenticationServices
cd src/AuthenticationServices
dotnet run

# Terminal 2 - GameService
cd src/GameService
dotnet run

# Terminal 3 - ApiGateway
cd src/ApiGateway
dotnet run

# Terminal 4 - Client Blazor (WASM servi par Nginx en Docker sinon)
cd src/BlazorGame.Client
dotnet run
```

### URLs d'Accès

- **Jeu (Interface Joueur)** : http://localhost:5000
- **API Gateway** : http://localhost:5001
- **Game Service API** : http://localhost:5002
- **Auth Service API** : http://localhost:5003

### Documentation API

- **Swagger (API Gateway)** : http://localhost:5001/swagger

## Architecture Technique

- **Frontend** : Blazor WebAssembly (.NET 9)
- **Backend** : ASP.NET Core Web API (.NET 9)
- **Communication** : HTTP/REST via API Gateway
- **Base de données** : Entity Framework Core In-Memory (pour l'instant ?)
- **Authentification** : Keycloak
- **Conteneurisation** : Docker Compose

## Tests Unitaires

BlazorGameQuest utilise **BUnit** et **xUnit** pour tester les composants Blazor

### Tests

```powershell
# Depuis la racine
dotnet restore

cd tests/BlazorGame.Client.Tests
dotnet test
```

## Contributeurs

- Yassine LAHMAR CHERIF
- Louisa MAIBECHE