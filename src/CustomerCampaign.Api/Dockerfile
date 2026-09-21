FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CustomerCampaign.Domain/*.csproj src/CustomerCampaign.Domain/
COPY src/CustomerCampaign.Application/*.csproj src/CustomerCampaign.Application/
COPY src/CustomerCampaign.Infrastructure/*.csproj src/CustomerCampaign.Infrastructure/
COPY src/CustomerCampaign.Api/*.csproj src/CustomerCampaign.Api/

RUN dotnet restore src/CustomerCampaign.Api/CustomerCampaign.Api.csproj

COPY src/ src/
RUN dotnet publish src/CustomerCampaign.Api/CustomerCampaign.Api.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

USER $APP_UID

COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CustomerCampaign.Api.dll"]