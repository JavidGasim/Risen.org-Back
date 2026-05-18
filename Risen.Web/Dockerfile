FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app

EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

COPY ["Risen.Web/Risen.Web.csproj", "Risen.Web/"]
COPY ["Risen.Business/Risen.Business.csproj", "Risen.Business/"]
COPY ["Risen.Contracts/Risen.Contracts.csproj", "Risen.Contracts/"]
COPY ["Risen.Entities/Risen.Entities.csproj", "Risen.Entities/"]
COPY ["Risen.DataAccess/Risen.DataAccess.csproj", "Risen.DataAccess/"]

RUN dotnet restore "Risen.Web/Risen.Web.csproj"

COPY . .
WORKDIR "/src/Risen.Web"

RUN dotnet build "Risen.Web.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Risen.Web.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app

COPY --from=publish /app/publish .

ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "Risen.Web.dll"]