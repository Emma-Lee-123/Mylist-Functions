![C#](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp&logoColor=white&style=flat)
![Azure Functions](https://img.shields.io/badge/Backend-Azure_Functions-brightgreen)
![Azure SQL](https://img.shields.io/badge/Database-Azure_SQL-blue)

### Trip Finder – Backend (Azure Functions)

This repository contains the backend for the Trip Finder application, built using Azure Functions (Isolated .NET). It serves as a serverless API that provides transit stop suggestions, trip data, and GTFS-based schedule information to the frontend application.

### Tech Stack

- **Platform:** Azure Functions (Isolated Process)  
- **Language:** C# (.NET 8)  
- **Database:** Azure SQL  
- **Deployment:** GitHub Actions to Azure  

### Features
- **Stop Suggestions API:** Returns autocomplete data for transit stop names  
- **Trip Lookup API:** Processes trip search queries using GTFS data  
- **Secure DB Access:** Uses Azure Managed Identity to connect to Azure SQL without storing credentials  
- **CORS-enabled:** Configured for integration with a separate frontend (e.g., Azure Static Web App)  

### Configuration
Environment variables include:
- **SqlConnectionString:** connection string to Azure SQL   

### Deployment
This backend is deployed using GitHub Actions and integrated with:
- **Azure Function App (Isolated .NET)**  
- **Azure SQL Database**  
- **Azure Static Web App frontend**  
