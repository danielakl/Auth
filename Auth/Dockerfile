FROM mcr.microsoft.com/dotnet/aspnet:5.0-alpine3.12 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["Auth/Auth.csproj", "Auth/"]
RUN dotnet restore "Auth/Auth.csproj"
COPY . .
WORKDIR Auth
RUN dotnet build Auth.csproj -c Release -o /app

FROM build AS publish
ARG VERSION=1.0.0
RUN dotnet publish Auth.csproj -c Release -o /app /p:Version=$VERSION

FROM base AS final

# Create low privileged application user
RUN addgroup --system app
RUN adduser --system --no-create-home --ingroup app --shell=/sbin/nologin app

WORKDIR /app
RUN chown app:app /app
RUN chmod 755 /app
COPY --chown=app:app --from=publish /app .

USER app:app
ENTRYPOINT ["dotnet", "Auth.dll"]
