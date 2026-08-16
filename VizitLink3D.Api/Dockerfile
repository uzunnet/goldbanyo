FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app
# Cache optimizasyonu: once yalnizca csproj'ler kopyalanir (nadiren degisir).
# Boylece "dotnet restore" katmani Docker layer cache'inden gelir,
# her commit'te NuGet paketleri sifirdan indirilmez.
COPY VizitLink3D.Api/VizitLink3D.Api.csproj VizitLink3D.Api/
COPY VizitLink3D.Ortak/VizitLink3D.Ortak.csproj VizitLink3D.Ortak/
RUN dotnet restore VizitLink3D.Api/VizitLink3D.Api.csproj
COPY . .
RUN dotnet publish VizitLink3D.Api/VizitLink3D.Api.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production
COPY --from=build /app/publish .
RUN mkdir -p /app/Veri
RUN cp -r /app/wwwroot/medya /app/medya-init 2>/dev/null || true
COPY VizitLink3D.Api/entrypoint.sh /app/entrypoint.sh
RUN chmod +x /app/entrypoint.sh
ENTRYPOINT ["/app/entrypoint.sh"]