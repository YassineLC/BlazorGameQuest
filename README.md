# BlazorGameQuest - Yassine LAHMAR CHERIF & Louisa MAIBECHE

Un jeu d'aventure textuel développé avec Blazor WebAssembly

Depuis le répertoire racine du projet, on exécute :

```powershell
cd tests/BlazorGame.Client.Tests
dotnet test
```

## Mise en place locale et tests de la version 2

### Prérequis additionnels

- **PostgreSQL** (port 5433 recommandé)
- **pgAdmin** pour la gestion de base de données
- **Entity Framework Core Tools** : `dotnet tool install --global dotnet-ef` 

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

Le fichier `.env` contient les variables d'environnement nécessaires, notamment les credentials de la base de données PostgreSQL. Un exemple de configuration est disponible dans `.env.example`.

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

Ouvrir **pgAdmin** et créer une base nommée **gamequest**.
Vérifier que PostgreSQL fonctionne (exemple :  port `5433`, utilisateur `postgres`, mot de passe `admin`).

Dans `appsettings.json`, la chaîne de connexion doit ressembler à ceci :

```json
"ConnectionStrings": {
  "Postgres": "Host=localhost;Port=5433;Database=gamequest;Username=postgres;Password=admin"
}
```

### Application des migrations

⚠️ **Important** : Exécutez cette commande depuis le répertoire racine du projet.

Installer l'outil Entity Framework Core si ce n'est pas déjà fait :

```bash
dotnet tool install --global dotnet-ef
```

Créer les tables dans la base avec :

```bash
dotnet ef database update --project src/ApiGateway --startup-project src/ApiGateway
```

**Note** : Si vous obtenez une erreur "Le fichier projet n'existe pas", vérifiez que vous êtes bien dans le répertoire racine du projet (où se trouve le fichier `BlazorGameQuest.sln`).

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