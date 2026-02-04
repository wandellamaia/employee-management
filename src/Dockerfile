FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/API/EmployeeManagement.API.csproj", "src/API/"]
COPY ["src/Application/EmployeeManagement.Application.csproj", "src/Application/"]
COPY ["src/Domain/EmployeeManagement.Domain.csproj", "src/Domain/"]
COPY ["src/Infrastructure/EmployeeManagement.Infrastructure.csproj", "src/Infrastructure/"]
RUN dotnet restore "src/API/EmployeeManagement.API.csproj"
COPY . .
WORKDIR "/src/src/API"
RUN dotnet build "EmployeeManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "EmployeeManagement.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "EmployeeManagement.API.dll"]
