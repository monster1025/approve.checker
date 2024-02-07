FROM repo.dev002.local/mcr.microsoft.com/dotnet/sdk:8.0 AS build

# copy csproj and restore as distinct layers
COPY ./src /solution/
WORKDIR /solution/
RUN dotnet restore

FROM build AS publish
RUN dotnet publish -c release --property PublishDir=/app --no-restore /p:TreatWarningsAsErrors=true /warnaserror -warnaserror

# final stage/image
FROM repo.dev002.local/mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app ./
ENTRYPOINT ["dotnet", "Approve.Checker.dll"]
