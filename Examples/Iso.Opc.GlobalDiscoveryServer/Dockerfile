#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["Iso.Opc.GlobalDiscoveryServer/Iso.Opc.GlobalDiscoveryServer.csproj", "Iso.Opc.GlobalDiscoveryServer/"]
RUN dotnet restore "Iso.Opc.GlobalDiscoveryServer/Iso.Opc.GlobalDiscoveryServer.csproj"
COPY . .
WORKDIR "/src/Iso.Opc.GlobalDiscoveryServer"
RUN dotnet build "Iso.Opc.GlobalDiscoveryServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Iso.Opc.GlobalDiscoveryServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Iso.Opc.GlobalDiscoveryServer.dll"]