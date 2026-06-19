FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
RUN apt-get update && apt-get install -y python3
WORKDIR /app
COPY . .
RUN dotnet restore Desadoor.UI/Desadoor.UI.csproj
RUN dotnet workload restore Desadoor.UI/Desadoor.UI.csproj
RUN dotnet publish Desadoor.UI/Desadoor.UI.csproj -c Release -o /app/publish

FROM nginx:alpine AS runtime
WORKDIR /usr/share/nginx/html
COPY --from=build /app/publish/wwwroot .
COPY Desadoor.UI/nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
