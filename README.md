# 🔐 Clean Auth Template

A production-ready **ASP.NET Core** authentication template with clean architecture, full identity management, JWT + Refresh Token flow, Google OAuth, role-based access control, and cloud image storage — all wired up and ready to build on.

---

## ✨ Features

- **JWT Authentication** with short-lived access tokens
- **Refresh Token** rotation via HttpOnly Secure Cookie
- **Google OAuth 2.0** login (auto-register on first sign-in)
- **Email Confirmation** flow with Base64Url-encoded tokens
- **Password Reset** with OTP code, SHA-256 hashing & attempt limiting
- **Role-Based Authorization** with dynamic custom roles (Admin-managed)
- **Account Management** — profile, avatar, email change, password change, soft-delete
- **User Management** (Admin) — CRUD, toggle status, unlock locked accounts
- **Cloudinary** image upload with auto-thumbnail generation
- **Background Email Jobs** via Hangfire
- **Rate Limiting** — tiered policies (Auth / Sensitive / General)
- **Result Pattern** — consistent typed error responses across all services

---

## 🏗️ Project Structure

```text
Template.Api/
├── Controllers/
│   ├── AuthController.cs
│   ├── AccountController.cs
│   ├── UsersController.cs
│   └── RolesController.cs
│
├── Services/
│   ├── AuthService.cs
│   ├── UserService.cs
│   ├── RoleService.cs
│   ├── EmailService.cs
│   ├── ImageService.cs
│   └── GoogleAuthService.cs
│
├── Persistence/
│   ├── ApplicationDbContext.cs
│   └── Migrations/
│
├── Contracts/
├── Entities/
├── Helpers/
├── DependencyInjection.cs
├── GlobalUsing.cs
└── Program.cs
```

---

## 🚀 Getting Started

### Prerequisites

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- SQL Server (or any EF Core-supported database)
- A [Cloudinary](https://cloudinary.com/) account
- A [Google Cloud](https://console.cloud.google.com/) project with OAuth 2.0 credentials
- An SMTP mail server (Gmail, Mailgun, etc.)

---

### 1. Clone the Repository

```bash
git clone https://github.com/a7hmedragheb/CleanAuthTemplate.git

cd CleanAuthTemplate
```

---

### 2. Configure `appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.;Database=CleanAuthDb;Trusted_Connection=True;TrustServerCertificate=True"
  },

  "JwtSettings": {
    "Key": "YOUR_SUPER_SECRET_KEY_MIN_32_CHARS",
    "Issuer": "Template",
    "Audience": "Template Users",
    "ExpiryMinutes": 30
  },

  "MailSettings": {
    "Mail": "your@email.com",
    "DisplayName": "Template",
    "Password": "your_smtp_password",
    "Host": "smtp.gmail.com",
    "Port": 587
  },

  "GoogleSettings": {
    "ClientId": "YOUR_GOOGLE_CLIENT_ID"
  },

  "CloudinarySettings": {
    "CloudName": "your_cloud_name",
    "ApiKey": "your_api_key",
    "ApiSecret": "your_api_secret"
  },

  "AppSettings": {
    "FrontendBaseUrl": "https://your-frontend.com"
  }
}
```

---

### 3. Apply Database Migrations

```bash
dotnet ef database update
```

---

### 4. Run the API

```bash
dotnet run --project Template.Api
```

The API will be available at:

```text
https://localhost:7131
```

---

# 📡 API Reference

### 🔑 Auth — `/auth`
 
| Method | Endpoint | Auth | Description |
|--------|----------|------|-------------|
| `POST` | `/auth` | — | Login with email & password |
| `POST` | `/auth/register` | — | Register a new account |
| `POST` | `/auth/confirm-email` | — | Confirm email with token |
| `POST` | `/auth/resend-confirmation-email` | — | Resend confirmation email |
| `POST` | `/auth/forget-password` | — | Send OTP reset code to email |
| `POST` | `/auth/verify-code` | — | Verify the OTP reset code |
| `POST` | `/auth/reset-password` | — | Reset password with OTP code |
| `POST` | `/auth/refresh` | Bearer | Rotate access + refresh tokens |
| `POST` | `/auth/revoke-refresh-token` | Bearer | Logout / revoke refresh token |
| `POST` | `/auth/login-google` | — | Login or register via Google |


<details>

<summary>📋 Request / Response Examples</summary>

### Login

```http
POST /auth/login
Content-Type: application/json

{
  "email": "admin@template.com",
  "password": "P@ssword123"
}
```

### Register

```http
POST /auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "firstName": "Ahmed",
  "lastName": "Ragheb",
  "gender": 0,
  "phoneNumber": "01012345678",
  "dateOfBirth": "2000-01-01",
  "password": "P@ssword123",
  "confirmPassword": "P@ssword123"
}
```

### Forget Password → Verify → Reset

```http
POST /auth/forget-password

{
  "email": "user@example.com"
}
```

```http
POST /auth/verify-code

{
  "email": "user@example.com",
  "code": "123456"
}
```

```http
POST /auth/reset-password

{
  "email": "user@example.com",
  "code": "123456",
  "newPassword": "NewP@ss123",
  "confirmPassword": "NewP@ss123"
}
```

### Google Login

```http
POST /auth/login-google

{
  "idToken": "<Google ID Token>"
}
```

</details>


---
 
### 👤 Account — `/account` *(Requires Authentication)*
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/account/profile` | Get current user profile |
| `PUT` | `/account/update-profile` | Update name, phone, DOB, gender |
| `PUT` | `/account/update-profile-image` | Upload profile image (form-data) |
| `PUT` | `/account/change-password` | Change password |
| `POST` | `/account/change-email` | Request email change (sends confirmation) |
| `PUT` | `/account/confirm-email-change` | Confirm email change with token |
| `DELETE` | `/account/delete-account` | Soft-delete account (requires password) |
 
<details>
  
<summary>📋 Request examples</summary>

### Update Profile

```http
PUT /account/update-profile
Authorization: Bearer <token>
Content-Type: application/json
 
{
  "firstName": "Ahmed",
  "lastName": "Ragheb",
  "phoneNumber": "01012345678",
  "dateOfBirth": "2000-01-01",
  "gender": 1
}
```
 
### Upload Avatar

```http
PUT /account/update-profile-image
Authorization: Bearer <token>
Content-Type: multipart/form-data
 
image: <file>
```
 
### Change Password

```http
PUT /account/change-password
Authorization: Bearer <token>
 
{
  "currentPassword": "OldP@ss123",
  "newPassword": "NewP@ss123",
  "confirmPassword": "NewP@ss123"
}
```
</details>


---
 
### 🛡️ Users — `/api/users` *(Admin only)*
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/users` | Get all non-member users |
| `GET` | `/api/users/{id}` | Get user by ID |
| `POST` | `/api/users` | Create a new user with roles |
| `PUT` | `/api/users/{id}` | Update user info and roles |
| `PUT` | `/api/users/{id}/toggle-status` | Enable / disable a user |
| `PUT` | `/api/users/{id}/unlock` | Unlock a locked-out user |
 
<details>

<summary>📋 Request Examples</summary>

### Create User

```http
POST /api/users
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "email": "staff@company.com",
  "firstName": "Sara",
  "lastName": "Ali",
  "phoneNumber": "01012345678",
  "dateOfBirth": "1999-05-10",
  "gender": 1,
  "password": "P@ssword123",
  "roles": ["Manager"]
}
```

---

### Update User

```http
PUT /api/users/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "firstName": "Sara",
  "lastName": "Mohamed",
  "phoneNumber": "01111111111",
  "dateOfBirth": "1998-08-15",
  "gender": 1,
  "roles": ["Admin", "Manager"]
}
```

</details>

---
 
### 🏷️ Roles — `/api/roles` *(Admin only)*
 
| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/roles?includeDisabled=false` | Get active roles |
| `GET` | `/api/roles?includeDisabled=true` | Get all roles including disabled |
| `GET` | `/api/roles/{id}` | Get role by ID |
| `POST` | `/api/roles` | Create a new role |
| `PUT` | `/api/roles/{id}` | Rename a role |
| `PUT` | `/api/roles/{id}/toggle-status` | Enable / disable a role |

 <details>

<summary>📋 Request Examples</summary>

### Create Role

```http
POST /api/roles
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Manager"
}
```

---

### Update Role

```http
PUT /api/roles/{id}
Authorization: Bearer <admin-token>
Content-Type: application/json

{
  "name": "Supervisor"
}
```

</details>


---
 
## 🔒 Security Design
 
### Token Strategy
- **Access Token** — short-lived JWT (configurable, default 30 min), returned in response body
- **Refresh Token** — long-lived (14 days), stored in `HttpOnly + Secure + SameSite=Strict` cookie, rotated on every use
### Password Reset Flow
```
User requests reset
      ↓
OTP code generated (CSPRNG, modulo-bias-free)
      ↓
Code hashed with SHA-256 + SecurityStamp (stored in DB)
      ↓
Raw code sent to email via background job
      ↓
User submits code → verified against hash
      ↓
Max attempts enforced → auto-expire on breach
      ↓
Password reset → SecurityStamp rotated → all old codes invalidated
```
 
### Rate Limiting Tiers
 
| Policy | Applied To |
|--------|-----------|
| `AuthPolicy` | Login, Register, Google Login |
| `SensitivePolicy` | Password reset, Change password, Change email, Delete account |
| `GeneralPolicy` | Refresh token, Profile endpoints |
 
---
 
## 🧩 Built With
 
| Library | Purpose |
|---------|---------|
| `ASP.NET Core 9` | Web framework | 
| `ASP.NET Core Identity` | User & role management |
| `Entity Framework Core` | ORM & migrations |
| `System.IdentityModel.Tokens.Jwt` | JWT generation & validation |
| `Google.Apis.Auth` | Google token validation |
| `CloudinaryDotNet` | Image upload & transformation |
| `MailKit` | SMTP email sending |
| `Hangfire` | Background job processing |
| `Mapster` | Object mapping |
| `FluentValidation` | Request validation |
 
---
 
## 📦 Postman Collection
 
Import the included collection to explore and test all endpoints:
 
```
Clean_Auth_Template_postman_collection.json
```
 
Set the `{{Host}}` environment variable to your API base URL (e.g. `https://localhost:7131`).
 
---
 
## 🗺️ Default Roles
 
The template seeds two built-in roles that cannot be deleted:
 
| Role | Description |
|------|-------------|
| `Admin` | Full access — user & role management |
| `Member` | Default role assigned on registration / Google sign-in |
 
Additional roles can be created at runtime via the Roles API.
 
---
 
## 🤝 Contributing
 
Pull requests are welcome. For major changes, please open an issue first to discuss what you'd like to change.
 
1. Fork the repository
2. Create your branch: `git checkout -b feature/your-feature`
3. Commit your changes: `git commit -m 'Add some feature'`
4. Push to the branch: `git push origin feature/your-feature`
5. Open a Pull Request
---
 
## 📄 License
 
This project is licensed under the [MIT License](LICENSE).
 
---
