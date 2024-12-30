#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TimeZoneBot/TimeZoneBot.csproj", "TimeZoneBot/"]
COPY ["TimeZoneBot.BusinessLayer/TimeZoneBot.BusinessLayer.csproj", "TimeZoneBot.BusinessLayer/"]
COPY ["TimeZoneBot.DataLayer/TimeZoneBot.DataLayer.csproj", "TimeZoneBot.DataLayer/"]
COPY ["TimeZoneBot.Models/TimeZoneBot.Models.csproj", "TimeZoneBot.Models/"]
RUN dotnet restore "TimeZoneBot/TimeZoneBot.csproj"
COPY . .
WORKDIR "/src/TimeZoneBot"
RUN dotnet build "TimeZoneBot.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TimeZoneBot.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TimeZoneBot.dll"]