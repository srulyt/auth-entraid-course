# Entra ID Authentication & Authorization Labs

Complete ASP.NET Core 8.0 lab application demonstrating authentication and authorization with Microsoft Entra ID (formerly Azure Active Directory).

## 🎯 Overview

This repository contains **two modules** with hands-on labs designed to teach authentication and authorization concepts through configuration and exploration—**no coding required**.

## 📚 Course Modules

### [Module 1: Authentication & Authorization Basics](Module1/README.md)
Foundational concepts for single-application scenarios (25-30 minutes)

### [Module 2: Protected Web APIs & Cross-Tenant](Module2/README.md)
Advanced multi-tier and service-to-service scenarios (25-30 minutes)

---

## Module 1: Foundations

This module contains two hands-on labs for learning authentication basics.

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
- Implement role-based authorization with app roles
- Handle authorization failures gracefully

**What Students Will Do:**
- Configure Microsoft Graph API permissions (User.Read scope)
- Test protected API endpoints requiring specific scopes
- Create and assign app roles in Entra ID
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

1. **Clone and Build:**
   ```bash
   git clone <repository-url>
   cd auth-entraid-course
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
   - Configure API permissions and app roles
   - Test authorization policies

## 🏗️ Project Architecture

### Technology Stack
- **ASP.NET Core 8.0** with Razor Pages
- **Microsoft.Identity.Web** for authentication
- **Microsoft Graph SDK** for API calls
- **Authorization Code + PKCE** flow
- JWT token handling and display

### Project Structure

```
auth-entraid-course/
├── src/
│   ├── WebAuthzDemo/              # Main web application
│   │   ├── Pages/                 # Razor Pages UI
│   │   ├── Controllers/           # Protected API endpoints
│   │   ├── Services/              # Token and Graph services
│   │   ├── Authorization/         # Policy definitions
│   │   └── Program.cs             # App configuration
│   └── TokenInspector/            # JWT decode/format utilities
└── docs/
    ├── README.md                  # This file
    ├── Lab1_Authentication.md     # Lab 1 instructions
    └── Lab2_SimpleAuthorization.md # Lab 2 instructions
```

### Key Components

**TokenService:** Acquires and decodes ID tokens and access tokens  
**GraphService:** Calls Microsoft Graph API with delegated permissions  
**Authorization Policies:** Enforces scope and role-based access control  
**Token Viewers:** Display formatted JWT tokens with claims highlighted

## 🔧 Configuration Reference

### appsettings.json Structure

| Setting | Description | Value |
|---------|-------------|-------|
| `AzureAd:Instance` | Microsoft login endpoint | `https://login.microsoftonline.com/` |
| `AzureAd:Domain` | Your tenant domain | `yourdomain.onmicrosoft.com` |
| `AzureAd:TenantId` | Directory (tenant) ID from Azure Portal | GUID format |
| `AzureAd:ClientId` | Application (client) ID from Azure Portal | GUID format |
| `AzureAd:CallbackPath` | OAuth callback path | `/signin-oidc` (must match redirect URI) |
| `Graph:Scopes` | Microsoft Graph scopes | `User.Read` |

**Note:** Configuration is done during Lab 1. All values are obtained from the Azure Portal during app registration.

## 🐛 Common Issues and Troubleshooting

| Error Code | Symptom | Solution |
|------------|---------|----------|
| **AADSTS50011** | Redirect URI mismatch | Verify redirect URI exactly matches in Azure Portal and application (protocol, port, path) |
| **AADSTS700016** | Application not found | Check Client ID in appsettings.json matches Azure Portal |
| **AADSTS65001** | Consent required | Grant admin consent for API permissions in Azure Portal |
| **403 Forbidden** | Authorization failure on protected endpoint | Verify permissions granted and user assigned to required role |
| **Token missing claims** | Missing `roles` or `scp` claims | Sign out completely and sign in again to refresh token |

### General Troubleshooting Tips
- Use the browser's developer tools (F12) to inspect network requests
- Check the application logs for detailed error messages
- Verify all GUIDs (Tenant ID, Client ID) are copied correctly
- Ensure HTTPS is used for all redirect URIs
- When in doubt, sign out completely and sign in again

## 📖 For Instructors

### Key Concepts to Emphasize

**Authentication vs Authorization:**
- **Authentication:** "Who are you?" (proving identity)
- **Authorization:** "What can you do?" (permissions after authentication)

**Token Types:**
- **ID Token:** For the client application (audience = your app's Client ID)
- **Access Token:** For APIs/resources (audience = the API, e.g., Microsoft Graph)
- Never send ID tokens to APIs; never use access tokens for identity

**Scopes vs Roles:**
- **Scopes:** Delegated permissions - user consents for app to act on their behalf
- **Roles:** Assigned permissions - admin assigns roles to users

**Claims-Based Security:**
- Tokens are JWTs containing claims (key-value pairs)
- ASP.NET Core policies evaluate claims to enforce authorization

### Suggested Teaching Flow

1. **Lab 1 Setup (Step 0):** Emphasize what app registrations represent
2. **Lab 1 Sign-In:** Show the OAuth redirect flow in browser
3. **Token Exploration:** Side-by-side comparison of ID vs Access tokens
4. **Lab 2 Scopes:** Demonstrate consent and delegated permissions
5. **Lab 2 Roles:** Show admin-assigned roles in tokens
6. **Error Moments:** Use failures as teaching opportunities

## 🔗 Additional Resources

### Microsoft Documentation
- [Microsoft Identity Platform](https://docs.microsoft.com/azure/active-directory/develop/)
- [Microsoft.Identity.Web Library](https://docs.microsoft.com/azure/active-directory/develop/microsoft-identity-web)
- [OAuth 2.0 and OpenID Connect](https://docs.microsoft.com/azure/active-directory/develop/active-directory-v2-protocols)
- [Microsoft Graph API](https://docs.microsoft.com/graph/)
- [App Registration Documentation](https://docs.microsoft.com/azure/active-directory/develop/quickstart-register-app)

### External Tools
- [JWT.ms - Token Decoder](https://jwt.ms)
- [OpenID Connect Specification](https://openid.net/connect/)

## 📄 License

MIT License - free for educational use.

---

**Ready to get started?** → [Begin with Lab 1: Authentication](Lab1_Authentication.md)
