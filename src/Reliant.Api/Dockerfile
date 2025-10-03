# ---------- Build ----------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj first for better layer caching
COPY src/Reliant.Domain/Reliant.Domain.csproj src/Reliant.Domain/
COPY src/Reliant.Infrastructure/Reliant.Infrastructure.csproj src/Reliant.Infrastructure/
COPY src/Reliant.Api/Reliant.Api.csproj src/Reliant.Api/

# Restore the API (pulls referenced projects too)
RUN dotnet restore src/Reliant.Api/Reliant.Api.csproj

# Now copy the rest of the source
COPY src/ src/

# Publish the API (brings along referenced projects)
RUN dotnet publish src/Reliant.Api/Reliant.Api.csproj -c Release -o /out

# ---------- Runtime ----------
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /out .
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Reliant.Api.dll"]
