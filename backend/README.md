# Employee Management - Backend

This is the backend for the Employee Management system, built with .NET 8 following Clean Architecture principles.

## Architecture

The project is structured into four main layers, as visualized in the Mermaid diagram below:

```mermaid
graph TD
    API[API Layer] --> Application[Application Layer]
    API --> Infrastructure[Infrastructure Layer]
    Infrastructure --> Application
    Infrastructure --> Domain[Domain Layer]
    Application --> Domain
    
    subgraph Layers
        API
        Infrastructure
        Application
        Domain
    end
    
    style Domain fill:#f9f,stroke:#333,stroke-width:2px
    style Application fill:#bbf,stroke:#333,stroke-width:2px
    style Infrastructure fill:#dfd,stroke:#333,stroke-width:2px
    style API fill:#fdd,stroke:#333,stroke-width:2px
```

### Layer Descriptions

-   **Domain**: Contains enteral business logic, entities, and repository interfaces. It has no dependencies on other layers.
-   **Application**: Contains application-specific business logic, DTOs, and service interfaces. It depends only on the Domain layer.
-   **Infrastructure**: Implements repository interfaces and handles external concerns like database access (Entity Framework Core) and security. It depends on Application and Domain.
-   **API**: The entry point of the application, containing Controllers and configuration. It depends on Infrastructure and Application.

## Development

To run the backend locally:

```bash
# From the backend directory
dotnet restore
dotnet run --project src/API/EmployeeManagement.API.csproj
```

Or from the root directory:

```bash
dotnet run --project backend/src/API/EmployeeManagement.API.csproj
```

## Testing

To run the backend tests:

```bash
# From the backend directory
dotnet test
```

Refer to the root [README](../README.md) for full project instructions.
