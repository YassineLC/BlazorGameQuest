#!/bin/bash

# Wait for Keycloak to be ready
echo "Waiting for Keycloak to start..."
sleep 30

# Keycloak admin credentials
KEYCLOAK_URL="http://localhost:8080"
ADMIN_USER="admin"
ADMIN_PASSWORD="admin"
REALM_NAME="BlazorGameQuest"
CLIENT_ID="blazor-game-quest"
CLIENT_SECRET="blazor-game-quest-secret"

echo "Setting up Keycloak realm and clients..."

# Get admin token
echo "Getting admin token..."
TOKEN=$(curl -s -X POST "$KEYCLOAK_URL/realms/master/protocol/openid-connect/token" \
  -H "Content-Type: application/x-www-form-urlencoded" \
  -d "client_id=admin-cli" \
  -d "username=$ADMIN_USER" \
  -d "password=$ADMIN_PASSWORD" \
  -d "grant_type=password" | jq -r '.access_token')

echo "Token obtained: ${TOKEN:0:20}..."

# Create realm
echo "Creating realm: $REALM_NAME"
curl -s -X POST "$KEYCLOAK_URL/admin/realms" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "realm": "'$REALM_NAME'",
    "enabled": true,
    "displayName": "Blazor Game Quest",
    "displayNameHtml": "<h1>Blazor Game Quest</h1>"
  }' 

echo ""
echo "Realm created successfully!"

# Create Blazor client
echo "Creating Blazor WebAssembly client..."
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/clients" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "clientId": "'$CLIENT_ID'",
    "name": "Blazor Game Quest Client",
    "enabled": true,
    "publicClient": false,
    "secret": "'$CLIENT_SECRET'",
    "redirectUris": [
      "http://localhost:5000/authentication/login-callback",
      "http://localhost:5000/",
      "https://localhost:5001/authentication/login-callback",
      "https://localhost:5001/"
    ],
    "logoutUrl": "http://localhost:5000/authentication/logout-callback",
    "postLogoutRedirectUris": [
      "http://localhost:5000/",
      "https://localhost:5001/"
    ],
    "defaultClientScopes": [
      "openid",
      "profile",
      "email",
      "roles"
    ],
    "accessTokenLifespan": 3600,
    "standardFlowEnabled": true,
    "directAccessGrantsEnabled": true,
    "implicitFlowEnabled": true,
    "attributes": {
      "access.token.lifespan": "3600"
    }
  }'

echo ""
echo "Client created successfully!"

# Create test users
echo "Creating test users..."

# Create user1
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user1",
    "email": "user1@game.local",
    "firstName": "User",
    "lastName": "One",
    "enabled": true,
    "emailVerified": true,
    "credentials": [
      {
        "type": "password",
        "value": "1234",
        "temporary": false
      }
    ]
  }'

# Create user2
curl -s -X POST "$KEYCLOAK_URL/admin/realms/$REALM_NAME/users" \
  -H "Authorization: Bearer $TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "user2",
    "email": "user2@game.local",
    "firstName": "User",
    "lastName": "Two",
    "enabled": true,
    "emailVerified": true,
    "credentials": [
      {
        "type": "password",
        "value": "1234",
        "temporary": false
      }
    ]
  }'

echo ""
echo "Test users created successfully!"
echo ""
echo "Keycloak setup complete!"
echo "Access Keycloak Admin Console at: $KEYCLOAK_URL/admin"
echo "Realm: $REALM_NAME"
echo "Admin credentials: $ADMIN_USER / $ADMIN_PASSWORD"
