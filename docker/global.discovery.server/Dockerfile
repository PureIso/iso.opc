FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["Iso.Opc.Core/", "Iso.Opc.Core/"]
COPY ["Examples/Iso.Opc.GlobalDiscoveryServer/", "Examples/Iso.Opc.GlobalDiscoveryServer/"]
RUN dotnet restore "Examples/Iso.Opc.GlobalDiscoveryServer/Iso.Opc.GlobalDiscoveryServer.csproj"
WORKDIR "/src/Examples/Iso.Opc.GlobalDiscoveryServer"
RUN dotnet publish "Iso.Opc.GlobalDiscoveryServer.csproj" -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish ./