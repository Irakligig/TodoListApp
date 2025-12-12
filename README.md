📝 To-Do List Application

A full-stack task management app built with ASP.NET Core MVC + Web API, designed to help users create, view, edit, and delete to-dos using clean architecture principles.

This project was completed as part of the EPAM Capstone and demonstrates real-world backend and full-stack development skills.

🚀 Features

Add, edit, and delete tasks

SQL Server persistent storage

RESTful Web API + MVC frontend

Clean services and repository architecture

Professional, scalable solution structure

📦 Tech Stack
Layer	Technology
Backend API	ASP.NET Core Web API
Frontend	ASP.NET Core MVC
Database	SQL Server (LocalDB / Express)
ORM	Entity Framework Core
Architecture	Services & Repository Pattern
Principles	SOLID, layered design
📁 Project Structure
TodoListApp/
├── TodoListApp.WebApp/            ← MVC frontend
├── TodoListApp.WebApi/            ← REST API
├── TodoListApp.Services/          ← Business logic
├── TodoListApp.Services.Database/ ← Database operations
├── images/                        ← Screenshots & assets
├── .gitignore
└── TodoListApp.sln

🛠 Getting Started
Prerequisites

.NET 7 SDK or newer

SQL Server Express / LocalDB

Visual Studio or VS Code

Clone the Repository
git clone https://github.com/Irakligig/TodoListApp.git
cd TodoListApp

Running the App

Open TodoListApp.sln in Visual Studio

Restore NuGet packages

Set WebApi and WebApp as startup projects

Update SQL connection string in appsettings.json

Press F5 to run

🧪 How It Works

The user interacts with the MVC frontend

The frontend sends requests to the Web API

The API performs CRUD operations using Entity Framework Core

SQL Server stores and updates task data

📌 Core API Endpoints
Action	Route
List tasks	GET /api/tasks
Create task	POST /api/tasks
Update task	PUT /api/tasks/{id}
Delete task	DELETE /api/tasks/{id}
❓ Why This Project Matters

This project shows that you can:

Build real-world full-stack apps

Design and consume REST APIs

Use SQL Server + EF Core effectively

Organize a solution using industry-standard architecture

Produce readable, maintainable, scalable code

This is exactly the kind of practical project recruiters look for.

📈 Future Improvements

User authentication & JWT

Task priorities and due dates

Swagger API documentation

Unit & integration tests

Dark/light theme UI

⭐ Support

If this project helped or impressed you, leaving a ⭐ on the repo helps a lot!
