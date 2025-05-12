![C#](https://img.shields.io/badge/Language-C%23-239120?logo=c-sharp&logoColor=white&style=flat)
![Azure Functions](https://img.shields.io/badge/Backend-Azure_Functions-brightgreen)
![Azure SQL](https://img.shields.io/badge/Database-Azure_SQL-blue)

### My List – Backend (Azure Functions)

This repository contains the backend API for MyList, a cloud-based tasks management application. The backend is built using C# and Azure Functions to provide secure, serverless endpoints for managing user-authenticated task data.

All backend services are deployed to Azure Function App, with automated CI/CD pipelines configured using GitHub Actions to ensure smooth and reliable deployments.

### Tech Stack

- **Platform:** Azure Functions (Isolated Process)  
- **Language:** C# (.NET 8)  
- **Database:** Azure SQL  
- **Deployment:** GitHub Actions to Azure  

### Features
- **User Authentication:** Integrates with Azure Static Web Apps auth to secure endpoints and ensure each user can only access their own tasks  
- **Task Management:** Create, retrieve, update (mark complete), and delete tasks via HTTP-triggered APIs 
- **Serverless Hosting:** uns on Azure Functions for scalability, fast performance, and low maintenance overhead  

### Configuration
Environment variables include:
- **SqlConnectionString:** connection string to Azure SQL   
- **CORS-enabled:** Configured for integration with a separate frontend (e.g., Azure Static Web App)
  
### Deployment(CI/CD Pipeline)
This backend is deployed using GitHub Actions and integrated with:
- **Azure Function App (Isolated .NET)**  
- **Azure SQL Database**  
- **Azure Static Web App frontend**  
