# BlazorGameQuest - Yassine LAHMAR CHERIF & Louisa MAIBECHE

Un jeu d'aventure textuel développé avec Blazor WebAssembly

> ** Note importante** : L'application utilise maintenant une **base de données en mémoire (InMemory)** 
> au lieu de PostgreSQL pour simplifier l'installation et les tests. 
> Les données sont créées automatiquement au démarrage et perdues à l'arrêt de l'application.

Depuis le répertoire racine du projet, on exécute :

```powershell
cd tests/BlazorGame.Client.Tests
dotnet test
```

## Mise en place locale et tests de la version 2

### Prérequis additionnels

- **Entity Framework Core Tools** : `dotnet tool install --global dotnet-ef` (optionnel, uniquement pour les migrations si vous repassez à une base persistante)

### Lancement de l'API

```bash
cd src/ApiGateway
dotnet run
```vers de style Metroidvania.

![Build Status](https://github.com/YassineLC/BlazorGameQuest/workflows/Build/badge.svg)
![.NET Version](https://img.shields.io/badge/.NET-9.0-blue)
![License](https://img.shields.io/badge/license-MIT-green)

## Fonctionnalités

- **Interface textuelle immersive** : Narration riche avec choix multiples
- **Design rétro** : Ambiance visuelle inspirée des jeux classiques
- **Architecture modulaire** : Séparation client/serveur avec services d'authentification
- **Responsive design** : Adapté aux mobiles et desktop
- **Déploiement automatique** : Build et déploiement via GitHub Actions

## Technologies

- **Frontend** : Blazor WebAssembly (.NET 9)
- **Backend** : ASP.NET Core Web API
- **Styling** : CSS3 avec animations et effets visuels
- **Déploiement** : GitHub Actions + GitHub Pages

## Architecture

### Microservices

- **BlazorGame.Client** - Interface utilisateur Blazor
- **ApiGateway** - Point d'entrée unique pour toutes les API
- **GameService** - Logique de jeu, génération de donjons, calcul des scores
- **AuthenticationServices** (Port 5003) - Gestion de l'authentification (sera intégré avec Keycloak)

### Projets Partagés

- **SharedModels** - Modèles de données partagés entre tous les services

## Structure du Projet

```
BlazorGameQuest/
├── src/
│   ├── BlazorGame.Client/       # Interface utilisateur Blazor (http://localhost:5000)
│   ├── ApiGateway/              # API Gateway (http://localhost:5001)
│   ├── GameService/             # Service de jeu (http://localhost:5002)
│   ├── AuthenticationServices/  # Service d'authentification (http://localhost:5003)
│   └── SharedModels/            # Modèles partagés
├── tests/
│   └── BlazorGame.Client.Tests/ # Tests unitaires
├── start-services.ps1           # Script de démarrage des services (en arrière-plan)
├── stop-services.ps1            # Script d'arrêt des services (avec confirmation)
└── BlazorGameQuest.sln          # Solution principale
```

## Démarrage Rapide

### Prérequis

- .NET 9.0 SDK
- PowerShell (pour les scripts de démarrage)

### Configuration

Avant de lancer l'application, créez un fichier `.env` à la racine du projet en vous basant sur le fichier `.env.example` fourni :

```powershell
# Copiez le fichier exemple
copy .env.example .env
```

Le fichier `.env` n'est plus nécessaire car l'application utilise maintenant une base de données en mémoire. Vous pouvez supprimer le fichier `.env` s'il existe.

### Lancement de l'Application

1. **Méthode Automatique (Recommandée)**
   ```powershell
   # Depuis le répertoire racine du projet
   .\start-services.ps1
   ```

2. **Méthode Manuelle**
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
   
   # Terminal 4 - Client Blazor
   cd src/BlazorGame.Client
   dotnet run
   ```

### URLs d'Accès

- **Jeu (Interface Joueur)** : http://localhost:5000
- **Administration** : http://localhost:5000/admin
- **API Gateway** : http://localhost:5001
- **Game Service API** : http://localhost:5002
- **Auth Service API** : http://localhost:5003

### Arrêt des Services

```powershell
.\stop-services.ps1
```

## Architecture Technique

- **Frontend** : Blazor WebAssembly (.NET 9)
- **Backend** : ASP.NET Core Web API (.NET 9)
- **Communication** : HTTP/REST via API Gateway
- **Base de données** : Entity Framework Core In-Memory (pour l'instant ?)
- **Authentification** : Keycloak (à intégrer)
- **Conteneurisation** : Docker (à implémenter)
## Tests Unitaires

BlazorGameQuest utilise **BUnit** et **xUnit** pour tester les composants Blazor

### Lancer tous les tests

Depuis le répertoire racine du projet, on exécute :

```powershell
cd BlazorGame.Client.Tests
dotnet test

## Mise en place locale et tests de la version 2

### Installation des dépendances

Depuis la racine du projet :

```bash
dotnet restore
```

### Configuration de la base de données

L'application utilise maintenant une base de données **en mémoire** (InMemory) qui ne nécessite aucune configuration. 
Les données sont automatiquement créées au démarrage de l'application et perdues à l'arrêt.

Aucune installation de PostgreSQL ou configuration de chaîne de connexion n'est requise.

### Lancement de l’API

```bash
cd ApiGateway
dotnet run
```

Ouvrir ensuite Swagger à l’adresse :
[http://localhost:5001/swagger]

### Tests des endpoints

Dans Swagger :

* **POST /api/dungeons** → créer un donjon
* **POST /api/rooms** → ajouter des salles
* **GET /api/dungeons** et **GET /api/rooms** → vérifier les données enregistrées

### Vérification dans pgAdmin

Dans la base **gamequest** :

```sql
SELECT * FROM "Dungeons";
SELECT * FROM "Rooms";
SELECT * FROM "Traps";
```
## Contributeurs

- Yassine LAHMAR CHERIF
- Louisa MAIBECHE