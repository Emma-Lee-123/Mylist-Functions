### Trip Finder – Backend (Azure Functions)

This repository contains the backend for the Trip Finder application, built using Azure Functions (Isolated .NET). It serves as a serverless API that provides transit stop suggestions, trip data, and GTFS-based schedule information to the frontend application.

### Tech Stack

- **Platform:** Azure Functions (Isolated Process)  
- **Language:** C# (.NET 8)  
- **Database:** Azure SQL  
- **Authentication:** Managed Identity (User-Assigned)  
- **Deployment:** GitHub Actions to Azure  

### Features
- **Stop Suggestions API:** Returns autocomplete data for transit stop names  
- **Trip Lookup API:** Processes trip search queries using GTFS data  
- **Secure DB Access:** Uses Azure Managed Identity to connect to Azure SQL without storing credentials  
- **CORS-enabled:** Configured for integration with a separate frontend (e.g., Azure Static Web App)  

### Configuration
Environment variables include:
- **SqlConnectionString:** connection string to Azure SQL (use managed identity authentication)  

### Deployment
This backend is deployed using GitHub Actions and integrated with:
- **Azure Function App (Isolated .NET)**  
- **Azure SQL Database**  
- **Azure Static Web App frontend**  
