# 📝 To-Do List Application

A full-stack task management app built with **ASP.NET Core MVC + Web API**, designed to help users create, view, edit, and delete to-dos using clean architecture principles.

This project was completed as part of the **EPAM Capstone** and demonstrates real-world backend and full-stack development skills.

---

## 🚀 Features

- Add, edit, and delete tasks  
- SQL Server persistent storage  
- RESTful Web API + MVC frontend  
- Clean services and repository architecture  
- Professional, scalable solution structure  

---

## 📦 Tech Stack

| Layer         | Technology                    |
|---------------|-------------------------------|
| Backend API   | ASP.NET Core Web API           |
| Frontend      | ASP.NET Core MVC               |
| Database      | SQL Server (LocalDB / Express)|
| ORM           | Entity Framework Core          |
| Architecture  | Services & Repository Pattern  |
| Principles    | SOLID, layered design          |

---

## 📁 Project Structure

TodoListApp/
├── TodoListApp.WebApp/ ← MVC frontend
├── TodoListApp.WebApi/ ← REST API
├── TodoListApp.Services/ ← Business logic
├── TodoListApp.Services.Database/ ← Database operations
├── images/ ← Screenshots & assets
├── .gitignore
└── TodoListApp.sln


---

## 🛠 Getting Started

### Prerequisites

- .NET 7 SDK or newer  
- SQL Server Express / LocalDB  
- Visual Studio or VS Code  

git clone https://github.com/Irakligig/TodoListApp.git
cd TodoListApp

## Running the App

  1.Open TodoListApp.sln in Visual Studio

  2.Restore NuGet packages

  3.Set WebApi and WebApp as startup projects

  4.Update SQL connection string in appsettings.json

  5.Press F5 to run

## How It Works

  1.The user interacts with the MVC frontend

  2.The frontend sends requests to the Web API

  3.The API performs CRUD operations using Entity Framework Core

  4.SQL Server stores and updates task data


## 📌 Core API Endpoints

| Action                             | Route                              | Method |
|-----------------------------------|-----------------------------------|--------|
| List all tasks                      | `/api/tasks`                      | GET    |
| Get task by ID                      | `/api/tasks/{id}`                 | GET    |
| Create new task                     | `/api/tasks`                      | POST   |
| Update existing task                | `/api/tasks/{id}`                 | PUT    |
| Delete task                         | `/api/tasks/{id}`                 | DELETE |
| List all users                      | `/api/users`                      | GET    |
| Get user by ID                      | `/api/users/{id}`                 | GET    |
| Create new user                     | `/api/users`                      | POST   |
| Update user                          | `/api/users/{id}`                 | PUT    |
| Delete user                          | `/api/users/{id}`                 | DELETE |
| Assign task to user                  | `/api/tasks/{id}/assign`          | POST   |
| Mark task as completed               | `/api/tasks/{id}/complete`        | PATCH  |
| Get tasks by status                  | `/api/tasks/status/{status}`      | GET    |
| Get tasks by assigned user           | `/api/tasks/user/{userId}`        | GET    |
| Search tasks by keyword              | `/api/tasks/search/{keyword}`     | GET    |
| List task priorities                 | `/api/tasks/priorities`           | GET    |
| Set task priority                    | `/api/tasks/{id}/priority`        | PATCH  |
| List task categories                 | `/api/tasks/categories`           | GET    |
| Set task category                    | `/api/tasks/{id}/category`        | PATCH  |
| Get user tasks statistics            | `/api/users/{id}/statistics`     | GET    |
| Bulk update tasks                    | `/api/tasks/bulk-update`          | PUT    |
| Bulk delete tasks                    | `/api/tasks/bulk-delete`          | DELETE |

---

## ❓ Why This Project Matters

This project shows that you can:

- Build **real-world full-stack apps**  
- Design and consume **REST APIs**  
- Use **SQL Server + EF Core** effectively  
- Organize a solution using **industry-standard architecture**  
- Produce **readable, maintainable, scalable code**  

This is exactly the kind of practical project recruiters look for.

---

## 📈 Future Improvements

- User authentication & JWT  
- Task priorities and due dates  
- Swagger API documentation  
- Unit & integration tests  
- Dark/light theme UI  

---

## ⭐ Support

If this project helped or impressed you, leaving a ⭐ on the repo helps a lot!
