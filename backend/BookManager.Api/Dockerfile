FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY BookManager.Domain/BookManager.Domain.csproj BookManager.Domain/
COPY BookManager.Application/BookManager.Application.csproj BookManager.Application/
COPY BookManager.Infrastructure/BookManager.Infrastructure.csproj BookManager.Infrastructure/
COPY BookManager.Api/BookManager.Api.csproj BookManager.Api/
RUN dotnet restore BookManager.Api/BookManager.Api.csproj

COPY . .
RUN dotnet publish BookManager.Api/BookManager.Api.csproj -c Release -o /app --no-restore


FROM build AS migrator
ARG EF_VERSION=10.0.11
RUN dotnet tool install --global dotnet-ef --version $EF_VERSION
ENV PATH="/root/.dotnet/tools:$PATH"
ENTRYPOINT ["dotnet", "ef", "database", "update", "--project", "BookManager.Infrastructure"]


FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BookManager.Api.dll"]
