#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/core/runtime:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["Iso.OPC.Server/Iso.OPC.Server.csproj", "Iso.OPC.Server/"]
RUN dotnet restore "Iso.OPC.Server/Iso.OPC.Server.csproj"
COPY . .
WORKDIR "/src/Iso.OPC.Server"
RUN dotnet build "Iso.OPC.Server.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Iso.OPC.Server.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Iso.OPC.Server.dll"]