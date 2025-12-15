# Module 2: Protected Web APIs and Cross-Tenant Authentication

This module contains two hands-on labs that build upon Module 1's foundation to explore advanced authentication scenarios with Microsoft Entra ID.

## Overview

**Duration**: 25-30 minutes total (12-15 min per lab)

**Prerequisites**:
- Completion of Module 1 (Authentication basics)
- Understanding of ID tokens vs Access tokens
- Familiarity with OAuth 2.0 and OpenID Connect concepts

## Learning Objectives

By completing this module, you will understand:
- How to protect custom Web APIs with Microsoft Entra ID
- Custom API scopes vs delegated permissions
- On-Behalf-Of (OBO) flow for calling downstream APIs
- App-only (daemon) authentication patterns
- Cross-tenant token acquisition
- Application permissions vs delegated permissions

## Lab Structure

### Lab 3: Protected Web API with On-Behalf-Of Flow (12-15 minutes)
**Scenario**: Build a three-tier application where a web client calls your protected API, which then calls Microsoft Graph on behalf of the user.

**Key Concepts**:
- Registering and exposing custom API scopes
- Scope-based authorization in ASP.NET Core
- Token audience validation
- On-Behalf-Of (OBO) flow mechanics
- Chaining API calls with user context

**Architecture**:
```
User Browser
    ↓ (OIDC + auth code flow)
ClientWeb (Razor Pages)
    ↓ (Bearer token with api.read scope)
MiddleTierApi (ASP.NET Core API)
    ↓ (OBO flow with user context)
Microsoft Graph
```

### Lab 4: Cross-Tenant Daemon Application (12-15 minutes)
**Scenario**: Create a daemon/service application that can authenticate to multiple Entra ID tenants and call Microsoft Graph with application permissions.

**Key Concepts**:
- Client credentials flow (app-only authentication)
- Application permissions vs delegated permissions
- Cross-tenant app registration and consent
- Service-to-service authentication
- Token acquisition without user context

**Architecture**:
```
Daemon Console App
    ↓ (client credentials flow)
Microsoft Entra ID (Home Tenant)
    ↓ (app-only token)
Microsoft Graph

Daemon Console App
    ↓ (client credentials with tenant ID)
Microsoft Entra ID (Customer Tenant)
    ↓ (app-only token)
Microsoft Graph
```

## Projects in This Module

### Labs.Shared
Common library containing:
- Shared models (`ApiResponse`, `TokenClaimsResponse`, `UserProfile`)
- Constants for scopes, claim types, and policies
- Configuration models (`AzureAdSettings`, `DaemonSettings`)

### Labs.MiddleTierApi
ASP.NET Core Web API (Lab 3) featuring:
- Three endpoints demonstrating different authorization patterns
- JWT Bearer authentication
- Scope-based authorization policies
- On-Behalf-Of flow to Microsoft Graph
- Swagger/OpenAPI documentation

### Labs.ClientWeb
Razor Pages web application (Lab 3) featuring:
- Interactive UI for calling protected API
- Token acquisition with automatic consent
- ID token viewer
- API call results display

### Labs.CrossTenantDaemon
Console application (Lab 4) featuring:
- App-only authentication
- Multi-tenant token acquisition
- Interactive menu for testing scenarios
- Token decoding and inspection
- Microsoft Graph API calls

## Getting Started

### System Requirements
- .NET 8.0 SDK or later
- Visual Studio 2022 or VS Code with C# extension
- Microsoft Entra ID tenant (or free trial)
- Access to create app registrations

### Quick Start

1. **Navigate to Module 2**:
   ```bash
   cd src/Module2
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Choose your lab**:
   - [Lab 3: Protected Web API + OBO Flow](Lab3_ProtectedWebAPI.md)
   - [Lab 4: Cross-Tenant Daemon](Lab4_CrossTenantDaemon.md)

Each lab is independent but builds on concepts from the previous one.

## Key Differences from Module 1

| Aspect | Module 1 | Module 2 |
|--------|----------|----------|
| **Authentication** | User sign-in only | User sign-in + app-only |
| **Token Types** | ID tokens primarily | Access tokens for APIs |
| **Scope** | Single app | Multi-tier with API calls |
| **Flow** | Auth code + PKCE | Auth code + OBO + client credentials |
| **Permissions** | Delegated (User.Read) | Custom scopes + application permissions |
| **Architecture** | Single web app | Client → API → Graph |

## Common Configuration Pattern

All apps in Module 2 follow this configuration pattern:

**appsettings.json structure**:
```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourtenant.onmicrosoft.com",
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "ClientSecret": "YOUR_CLIENT_SECRET",  // API and Daemon only
    "CallbackPath": "/signin-oidc"          // Client Web only
  }
}
```

## Security Best Practices Demonstrated

- ✅ HTTPS enforced for all applications
- ✅ Secure token storage (in-memory cache)
- ✅ Least privilege principle (minimal scopes/permissions)
- ✅ Token validation on API endpoints
- ✅ Scope-based authorization
- ✅ Client secrets stored securely (not in source control)
- ✅ CORS configured for local development only

## Troubleshooting

### Common Issues

**"Invalid audience" errors**:
- Verify the API's Application ID URI matches the audience in the token
- Check that the client is requesting the correct scope

**"Insufficient permissions" errors**:
- Ensure admin consent is granted for application permissions
- Verify API permissions are configured in the client app registration

**OBO flow failures**:
- Confirm the API has delegated permissions to Microsoft Graph
- Check that the user has consented to the required scopes
- Verify the token being exchanged has the correct audience

**Cross-tenant authentication fails**:
- Ensure the daemon app is registered as multi-tenant
- Verify admin consent is granted in the customer tenant
- Check that application permissions are properly configured

## Additional Resources

- [Microsoft identity platform documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/)
- [On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)
- [Client credentials flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [Application vs delegated permissions](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent)
- [Multi-tenant applications](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant)

## Next Steps

After completing this module, you'll have hands-on experience with:
- Building and securing custom Web APIs
- Implementing multi-tier authentication flows
- Working with both delegated and application permissions
- Understanding token audience and scope validation
- Cross-tenant authentication scenarios

These are fundamental patterns used in enterprise applications built on Microsoft Entra ID.

---

**Ready to start?** Choose your lab:
- [Lab 3: Protected Web API with OBO Flow →](Lab3_ProtectedWebAPI.md)
- [Lab 4: Cross-Tenant Daemon →](Lab4_CrossTenantDaemon.md)
