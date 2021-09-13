#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

ENV hostname=172.17.0.2

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ReceiveLogs/ReceiveLogs.csproj", "ReceiveLogs/"]
RUN dotnet restore "ReceiveLogs/ReceiveLogs.csproj"
COPY . .
WORKDIR "/src/ReceiveLogs"
RUN dotnet build "ReceiveLogs.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReceiveLogs.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReceiveLogs.dll"]