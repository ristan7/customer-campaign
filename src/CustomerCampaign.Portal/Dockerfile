FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY src/CustomerCampaign.Portal/*.csproj src/CustomerCampaign.Portal/
RUN dotnet restore src/CustomerCampaign.Portal/CustomerCampaign.Portal.csproj

COPY src/CustomerCampaign.Portal/ src/CustomerCampaign.Portal/
RUN dotnet publish src/CustomerCampaign.Portal/CustomerCampaign.Portal.csproj \
    -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
USER $APP_UID
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "CustomerCampaign.Portal.dll"]