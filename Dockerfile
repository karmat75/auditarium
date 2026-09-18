# SPDX-License-Identifier: MIT
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS development
WORKDIR /workspace
RUN dotnet --info

FROM development AS build
COPY . .
RUN dotnet restore Auditarium.sln --locked-mode
RUN dotnet publish UI/Auditarium.Api/Auditarium.Api.csproj --no-restore -c Release -o /out/api
RUN dotnet publish UI/Auditarium.Web/Auditarium.Web.csproj --no-restore -c Release -o /out/web

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS api
WORKDIR /app
COPY --from=build /out/api .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Auditarium.Api.dll"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS web
WORKDIR /app
COPY --from=build /out/web .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "Auditarium.Web.dll"]
