FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

COPY ["Fragmenta.Api/Fragmenta.Api.csproj", "Fragmenta.Api/"]
COPY ["Fragmenta.Dal/Fragmenta.Dal.csproj", "Fragmenta.Dal/"]

RUN dotnet restore "Fragmenta.Api/Fragmenta.Api.csproj"

COPY . .

RUN dotnet publish "Fragmenta.Api/Fragmenta.Api.csproj" -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/out ./

ENV ASPNETCORE_URLS=http://0.0.0.0:5000
EXPOSE 5000

ENTRYPOINT ["dotnet", "Fragmenta.Api.dll"]
