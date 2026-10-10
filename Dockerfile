# syntax=docker/dockerfile:1

# =========================================================================
# Stage 1 — build
# Restores and publishes Template.Core.Api (and everything it references:
# App, Domain, Infra, IoC, CrossCutting) in the .NET 10 SDK image.
# =========================================================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy only the .csproj files first so this layer (the slow one — NuGet
# restore) is cached and skipped on every rebuild unless a dependency
# actually changed, instead of invalidating on any .cs edit.
COPY Directory.Build.props ./
COPY src/Template.Core.Api/Template.Core.Api.csproj src/Template.Core.Api/
COPY src/Template.Core.App/Template.Core.App.csproj src/Template.Core.App/
COPY src/Template.Core.Domain/Template.Core.Domain.csproj src/Template.Core.Domain/
COPY src/Template.Core.Infra/Template.Core.Infra.csproj src/Template.Core.Infra/
COPY src/Template.Core.IoC/Template.Core.IoC.csproj src/Template.Core.IoC/
COPY src/Template.Core.CrossCutting/Template.CrossCutting.csproj src/Template.Core.CrossCutting/

RUN dotnet restore src/Template.Core.Api/Template.Core.Api.csproj

# Now bring in the rest of the source and publish (Release, no re-restore).
COPY src/Template.Core.Api/ src/Template.Core.Api/
COPY src/Template.Core.App/ src/Template.Core.App/
COPY src/Template.Core.Domain/ src/Template.Core.Domain/
COPY src/Template.Core.Infra/ src/Template.Core.Infra/
COPY src/Template.Core.IoC/ src/Template.Core.IoC/
COPY src/Template.Core.CrossCutting/ src/Template.Core.CrossCutting/

RUN dotnet publish src/Template.Core.Api/Template.Core.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# =========================================================================
# Stage 2 — runtime
# Only the ASP.NET Core runtime + published output ship here — no SDK,
# no source, no NuGet cache — which is what keeps this image small.
# =========================================================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Run as a non-root user inside the container.
RUN addgroup --system --gid 1000 appgroup \
 && adduser  --system --uid 1000 --ingroup appgroup appuser

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

USER appuser

ENTRYPOINT ["dotnet", "Template.Core.Api.dll"]
