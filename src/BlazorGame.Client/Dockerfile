FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/BlazorGame.Client/BlazorGame.Client.csproj", "src/BlazorGame.Client/"]
COPY ["src/SharedModels/SharedModels.csproj", "src/SharedModels/"]
RUN dotnet restore "src/BlazorGame.Client/BlazorGame.Client.csproj"
COPY . .
WORKDIR "/src/src/BlazorGame.Client"
RUN dotnet publish "BlazorGame.Client.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
EXPOSE 8080
# Adapter nginx à 8080
RUN sed -i 's/listen\s\+80;/listen 8080;/g' /etc/nginx/conf.d/default.conf
CMD ["nginx", "-g", "daemon off;"]