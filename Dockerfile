FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["SporiumAPI.csproj", "."]
RUN dotnet restore "./SporiumAPI.csproj"
COPY . .
RUN dotnet build "SporiumAPI.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SporiumAPI.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SporiumAPI.dll"]