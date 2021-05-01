ARG BUILD=Debug
ARG DOTNET_SDK_VERSION=5.0

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS base
WORKDIR /app
EXPOSE 4001/tcp

FROM mcr.microsoft.com/dotnet/sdk:${DOTNET_SDK_VERSION} AS builder
ARG BUILD

WORKDIR /src

# copy csproj and restore as distinct layers
COPY *.sln ./
COPY Concord/Concord.csproj Concord/
COPY Concord2Mqtt/Concord2Mqtt.csproj Concord2Mqtt/
RUN dotnet restore
COPY . .
WORKDIR /src/Concord2Mqtt
RUN dotnet build -c $BUILD -o /app

FROM builder AS publish
ARG BUILD
RUN dotnet publish -c $BUILD -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Concord2Mqtt.dll"]


