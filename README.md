# Mini Learning Management System (LMS)

A complete Learning Management System (LMS) built with:

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Angular
- JWT Authentication
- Bootstrap 5

---

# Project Overview

This project was developed as part of a Full Stack Development Assignment.

The system allows:

- Students to register and login
- Students to browse and enroll in published courses
- Students to track learning progress
- Students to complete lessons
- Administrators to manage courses and lessons
- Administrators to publish courses
- Administrators to monitor student progress

---

# Technologies Used

## Backend

- ASP.NET Core Web API
- Entity Framework Core
- SQL Server
- Repository Pattern
- Service Layer
- JWT Authentication
- AutoMapper

## Frontend

- Angular
- Bootstrap 5
- TypeScript
- Angular Route Guards
- Angular Services

---

# System Architecture

Backend follows a layered architecture:

```text
Controllers
    ↓
Services
    ↓
Repositories
    ↓
Entity Framework Core
    ↓
SQL Server
```

Project Structure:

```text
MiniLMS.API
│
├── Controllers
├── Services
├── Repositories
├── Models
├── DTOs
├── Helpers
└── Data

MiniLMS.Client
│
├── Core
│   ├── Services
│   └── Guards
│
├── Features
│   ├── Auth
│   ├── Admin
│   └── Student
│
└── Models
```

---

# Authentication & Authorization

The application uses JWT Authentication.

Roles:

- Admin
- Student

Authorization Rules:

| Feature | Admin | Student |
|----------|----------|----------|
| Manage Courses | ✅ | ❌ |
| Publish Course | ✅ | ❌ |
| Manage Lessons | ✅ | ❌ |
| View Student Progress | ✅ | ❌ |
| Browse Courses | ❌ | ✅ |
| Enroll in Course | ❌ | ✅ |
| Complete Lessons | ❌ | ✅ |
| View Own Progress | ❌ | ✅ |

Route Guards are implemented in Angular to prevent unauthorized navigation.

Backend APIs are also protected using role-based authorization.

---

# Features

## Authentication

### Student Signup

Students can register using:

- Username
- Password
- Full Name
- Email

### Login

Users login with:

- Username
- Password

After login the system redirects based on role:

- Admin → Admin Area
- Student → Student Area

---

# Admin Features

## Course Management

- Create Course
- Edit Course
- Delete Course
- Publish Course

### Publish Validation

A course cannot be published unless it contains at least one lesson.

---

## Lesson Management

Admin can:

- Add Lessons
- Remove Lessons
- Reorder Lessons

---

## Student Progress Monitoring

Admin can view:

- Enrolled Students
- Course Progress
- Completion Status

---

# Student Features

## Course Catalog

Students can:

- Browse Published Courses
- Search Courses
- Filter by Category
- Enroll in Courses

---

## Dashboard

Students can:

- View Enrolled Courses
- Track Progress
- View Course Status

Status values:

- Not Started
- In Progress
- Completed

---

## Lesson Completion

Students can:

- Open Course Details
- Mark Lessons as Completed

Progress is automatically recalculated.

---

# Business Rules

## Enrollment

A student cannot enroll in the same course twice.

---

## Progress Calculation

Progress Percentage:

```text
Completed Lessons / Total Lessons × 100
```

Calculated automatically by the server.

---

## Enrollment Status

```text
0%       => Not Started
1 - 99%  => In Progress
100%     => Completed
```

---

# API Endpoints

## Authentication

```http
POST /api/Auth/signup
POST /api/Auth/login
```

---

## Admin

```http
GET    /api/AdminCourses
GET    /api/AdminCourses/{id}
POST   /api/AdminCourses
PUT    /api/AdminCourses/{id}
DELETE /api/AdminCourses/{id}

POST   /api/AdminCourses/{courseId}/lessons
POST   /api/AdminCourses/{courseId}/lessons/reorder
POST   /api/AdminCourses/{courseId}/publish
```

---

## Student

```http
GET  /api/StudentCourses/available
POST /api/StudentCourses/enroll/{courseId}

GET  /api/StudentCourses/my-courses

GET  /api/StudentCourses/details/{courseId}

POST /api/StudentCourses/enrollments/{enrollmentId}/complete-lesson/{lessonId}
```

---

# Setup Instructions

## Backend

Navigate to:

```bash
cd MiniLMS.API
```

Restore packages:

```bash
dotnet restore
```

Apply migrations:

```bash
dotnet ef database update
```

Run API:

```bash
dotnet run
```

Swagger:

```text
https://localhost:7070/swagger
```

---

## Frontend

Navigate to:

```bash
cd MiniLMS.Client
```

Install dependencies:

```bash
npm install
```

Run Angular:

```bash
ng serve
```

Frontend URL:

```text
http://localhost:4200
```

---

# Demo Accounts

## Admin

```text
Username: admin
Password: admin123
```

## Student

Create a new account using the Signup page.

---

# Error Handling

The application provides user-friendly error messages for:

- Login failures
- Validation errors
- Unauthorized access
- Duplicate enrollments
- Missing resources

HTTP status codes are handled appropriately:

| Status | Meaning |
|----------|----------|
| 200 | Success |
| 400 | Validation Error |
| 401 | Unauthorized |
| 403 | Forbidden |
| 404 | Not Found |
| 500 | Server Error |

---

# Future Improvements

- Upload course videos
- Course certificates
- Email verification
- Password reset
- Course ratings and reviews
- Dashboard analytics

---

# Author

Gaber Anwar Abdelwahab

Full Stack Developer

ASP.NET Core Web API + Angular
