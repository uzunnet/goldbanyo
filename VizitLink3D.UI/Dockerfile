FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y python3
WORKDIR /app
# Cache optimizasyonu: once yalnizca csproj'ler kopyalanir (nadiren degisir).
# Boylece "dotnet restore" ve "dotnet workload restore" katmanlari cache'lenir;
# WASM toolset her commit'te sifirdan kurulmaz.
COPY VizitLink3D.UI/VizitLink3D.UI.csproj VizitLink3D.UI/
COPY VizitLink3D.Ortak/VizitLink3D.Ortak.csproj VizitLink3D.Ortak/
RUN dotnet restore VizitLink3D.UI/VizitLink3D.UI.csproj
RUN dotnet workload restore VizitLink3D.UI/VizitLink3D.UI.csproj
COPY . .
RUN dotnet publish VizitLink3D.UI/VizitLink3D.UI.csproj -c Release -o /app/publish

FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY VizitLink3D.UI/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80