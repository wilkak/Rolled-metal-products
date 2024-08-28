#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Базовый образ
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080

# Этап сборки
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Rolled-metal-products.csproj", "."]
RUN dotnet restore "./Rolled-metal-products.csproj"
COPY . .
WORKDIR "/src/."
RUN dotnet build "./Rolled-metal-products.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Этап публикации
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Rolled-metal-products.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Финальный этап
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Назначаем права на директории и файлы для пользователя app
USER root
RUN chown -R app:app /app
USER app

ENTRYPOINT ["dotnet", "Rolled-metal-products.dll"]