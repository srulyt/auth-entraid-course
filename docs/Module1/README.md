# Module 1: Entra ID Authentication & Authorization Basics

Complete ASP.NET Core 8.0 lab application demonstrating basic authentication and authorization with Microsoft Entra ID (formerly Azure Active Directory).

## 🎯 Overview

Module 1 contains two hands-on labs designed to teach authentication and authorization concepts through configuration and exploration—**no coding required**.

### Lab 1: Authentication with Microsoft Entra ID (~25-30 minutes)
**Learning Objectives:**
- Create and configure an app registration in Entra ID
- Understand authentication flows and token acquisition
- Explore ID tokens vs access tokens
- Learn token claims and their purposes
- Troubleshoot common authentication errors

**What Students Will Do:**
- Register an application in Azure Portal
- Configure the application with Tenant/Client IDs
- Sign in and examine ID tokens and access tokens
- Compare token audiences and purposes
- View and search all token claims
- Simulate redirect URI mismatch errors

[Start Lab 1 →](Lab1_Authentication.md)

---

### Lab 2: Authorization (~15 minutes)
**Learning Objectives:**
- Understand authorization concepts (what you can do after proving who you are)
- Implement scope-based authorization with delegated permissions
- Implement role-based authorization with local app roles
- Handle authorization failures gracefully

**What Students Will Do:**
- Configure Microsoft Graph API permissions (User.Read scope)
- Test protected API endpoints requiring authentication
- Self-assign local Admin role for testing
- Test role-based authorization policies
- Observe clear authorization failure messages

[Start Lab 2 →](Lab2_SimpleAuthorization.md)

---

**Key Features:**
- ✅ No coding required by students - fully functional out of the box
- ✅ Interactive token exploration with jwt.ms integration
- ✅ Clear teaching aids and conceptual explanations
- ✅ Error simulation for common authentication issues
- ✅ Protected API endpoints demonstrating authorization

## 📋 Prerequisites

### For Instructors/Lab Setup
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) installed
- [Visual Studio Code](https://code.visualstudio.com/) or Visual Studio 2022
- Code editor for reviewing project structure

### For Students
- Access to [Azure Portal](https://portal.azure.com)
- Microsoft Entra ID tenant (free tier is sufficient)
- Permissions to create app registrations in the tenant
- A user account to test authentication
- Basic understanding of web applications (helpful but not required)

## 🚀 Quick Start

1. **Build the Solution:**
   ```bash
   cd src/Module1
   dotnet restore
   dotnet build
   ```

2. **Start with Lab 1:**
   - Open [Lab 1: Authentication](Lab1_Authentication.md)
   - Follow the step-by-step instructions to create an app registration
   - Configure and run the application
   - Explore tokens and claims

3. **Continue to Lab 2:**
   - Open [Lab 2: Authorization](Lab2_SimpleAuthorization.md)
   - Configure API permissions and test authorization
   - Test role-based authorization

## 🏗️ Project Architecture

### Technology Stack
- **ASP.NET Core 8.0** with Razor Pages
- **Microsoft.Identity.Web** for authentication
- **Microsoft Graph SDK** for API calls
- **Authorization Code + PKCE** flow
- JWT token handling and display

### Project Structure

```
Module1/
├── AuthzLabs.sln                  # Solution file
├── WebAuthzDemo/                  # Main web application
│   ├── Pages/                     # Razor Pages UI
│   ├── Controllers/               # Protected API endpoints
│   ├── Services/                  # Token and Graph services
│   ├── Authorization/             # Policy definitions
│   └── Program.cs                 # App configuration
└── TokenInspector/                # JWT decode/format utilities
```

### Key Components

**TokenService:** Acquires and decodes ID tokens and access tokens  
**GraphService:** Calls Microsoft Graph API with delegated permissions  
**LocalRoleService:** In-memory role management for demonstration  
**Authorization Policies:** Enforces authentication and role-based access control  
**Token Viewers:** Display formatted JWT tokens with claims highlighted

## 🔧 Running the Application

```bash
cd src/Module1
dotnet run --project WebAuthzDemo
```

The application will start on `https://localhost:7166`

## 📖 For Instructors

### Key Concepts to Emphasize

**Authentication vs Authorization:**
- **Authentication:** "Who are you?" (proving identity)
- **Authorization:** "What can you do?" (permissions after authentication)

**Token Types:**
- **ID Token:** For the client application (audience = your app's Client ID)
- **Access Token:** For APIs/resources (audience = the API, e.g., Microsoft Graph)
- Never send ID tokens to APIs; never use access tokens for identity

**Scopes vs Roles (Module 1 Context):**
- **Scopes:** Delegated permissions - user consents for app to act on their behalf
- **Roles:** In Module 1, these are application-managed roles stored locally (not Entra ID app roles)

**Claims-Based Security:**
- Tokens are JWTs containing claims (key-value pairs)
- ASP.NET Core policies evaluate claims to enforce authorization

### Suggested Teaching Flow

1. **Lab 1 Setup (Step 0):** Emphasize what app registrations represent
2. **Lab 1 Sign-In:** Show the OAuth redirect flow in browser
3. **Token Exploration:** Side-by-side comparison of ID vs Access tokens
4. **Lab 2 Authorization:** Demonstrate the difference between authentication and authorization
5. **Lab 2 Roles:** Show local role assignment and authorization enforcement
6. **Error Moments:** Use failures as teaching opportunities

## 🔗 Next Steps

After completing Module 1, students can proceed to **Module 2** to learn about:
- Protected Web APIs with custom scopes
- On-Behalf-Of (OBO) flow for downstream API calls
- Cross-tenant scenarios
- App-only (client credentials) authentication

---

**Ready to get started?** → [Begin with Lab 1: Authentication](Lab1_Authentication.md)
