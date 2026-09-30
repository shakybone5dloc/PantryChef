# ---------- Stage 1: build (big SDK image, thrown away afterwards) ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy ONLY the project files first, then restore.
# Docker caches this layer, so packages are downloaded again only when a .csproj changes.
COPY global.json PantryChef.slnx ./
COPY src/PantryChef.Domain/PantryChef.Domain.csproj                 src/PantryChef.Domain/
COPY src/PantryChef.Application/PantryChef.Application.csproj       src/PantryChef.Application/
COPY src/PantryChef.Infrastructure/PantryChef.Infrastructure.csproj src/PantryChef.Infrastructure/
COPY src/PantryChef.Api/PantryChef.Api.csproj                       src/PantryChef.Api/
RUN dotnet restore src/PantryChef.Api/PantryChef.Api.csproj

# Now the source code (changes often) and publish
COPY src/ src/
RUN dotnet publish src/PantryChef.Api/PantryChef.Api.csproj -c Release -o /app/publish --no-restore

# ---------- Stage 2: EF Core migrations bundle ----------
FROM build AS migrator-build
RUN dotnet tool install --global dotnet-ef --version 10.*
ENV PATH="$PATH:/root/.dotnet/tools"
# EF runs Program.cs to discover the DbContext while bundling. Nothing connects to a
# database here, but our fail-fast guard needs *a* connection string to exist.
RUN ConnectionStrings__Pantry="Host=placeholder;Database=placeholder" \
      dotnet ef migrations bundle \
      --project src/PantryChef.Infrastructure \
      --startup-project src/PantryChef.Api \
      --configuration Release \
      --output /app/efbundle

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS migrator
WORKDIR /app
COPY --from=migrator-build /app/efbundle .
USER $APP_UID
ENTRYPOINT ["./efbundle"]

# ---------- Stage 3: the API (small runtime-only image) ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
USER $APP_UID
EXPOSE 8080
ENTRYPOINT ["dotnet", "PantryChef.Api.dll"]