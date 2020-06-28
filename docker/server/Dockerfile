FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster-slim AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:5.0-buster AS build
WORKDIR /src
COPY ["Iso.Opc.Core/", "Iso.Opc.Core/"]
COPY ["Examples/Iso.Opc.Server/", "Examples/Iso.Opc.Server/"]
COPY ["Examples/Plugins/Iso.Opc.Plugin.InCodeNodeManager/", "Examples/Plugins/Iso.Opc.Plugin.InCodeNodeManager/"]
COPY ["Examples/Plugins/Iso.Opc.Plugin.XMLNodeManager/", "Examples/Plugins/Iso.Opc.Plugin.XMLNodeManager/"]
RUN dotnet restore "Examples/Iso.Opc.Server/Iso.Opc.Server.csproj"
RUN dotnet restore "Examples/Plugins/Iso.Opc.Plugin.InCodeNodeManager/Iso.Opc.Plugin.InCodeNodeManager.csproj"
RUN dotnet restore "Examples/Plugins/Iso.Opc.Plugin.XMLNodeManager/Iso.Opc.Plugin.XMLNodeManager.csproj"
COPY . .
WORKDIR "/src/Examples/Iso.Opc.Server"
RUN dotnet publish "Iso.Opc.Server.csproj" -c Release -o /app/publish

FROM base AS final
COPY --from=build /app/publish ./
RUN ls