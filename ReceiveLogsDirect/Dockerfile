#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/runtime:3.1 AS base
WORKDIR /app

ENV hostname=172.17.0.2

FROM mcr.microsoft.com/dotnet/sdk:3.1 AS build
WORKDIR /src
COPY ["ReceiveLogsDirect/ReceiveLogsDirect.csproj", "ReceiveLogsDirect/"]
RUN dotnet restore "ReceiveLogsDirect/ReceiveLogsDirect.csproj"
COPY . .
WORKDIR "/src/ReceiveLogsDirect"
RUN dotnet build "ReceiveLogsDirect.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ReceiveLogsDirect.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ReceiveLogsDirect.dll"]

#Run this created image after with: run -i --name="..." receivelogsdirect [args]