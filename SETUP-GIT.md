# Initialisation Git et GitHub Actions pour BlazorGameQuest

## 1. Initialiser le repository Git

```bash
git init
git add .
git commit -m "Initial commit: Structure microservices BlazorGameQuest"
```

## 2. Créer un repository sur GitHub

1. Aller sur GitHub.com
2. Créer un nouveau repository "BlazorGameQuest" 
3. Connecter le repository local :

```bash
git remote add origin https://github.com/[votre-username]/BlazorGameQuest.git
git branch -M main
git push -u origin main
```

## 3. Configuration GitHub Actions (à faire plus tard)

Créer le fichier `.github/workflows/ci-cd.yml` pour :
- Build automatique
- Tests automatisés  
- Déploiement Docker
- Vérification de sécurité

## 4. Structure GitHub Actions prévue

```yaml
# .github/workflows/ci-cd.yml
name: CI/CD BlazorGameQuest

on:
  push:
    branches: [ main, develop ]
  pull_request:
    branches: [ main ]

jobs:
  build-and-test:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: 9.0.x
    - name: Restore dependencies
      run: dotnet restore
    - name: Build
      run: dotnet build --no-restore
    - name: Test
      run: dotnet test --no-build --verbosity normal
```

Ce fichier sera créé lors d'un prochain livrable avec Docker.