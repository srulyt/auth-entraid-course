# Lab 3: Protected Web API with On-Behalf-Of (OBO) Flow

**Duration**: 12-15 minutes  
**Difficulty**: Intermediate

## Overview

In this lab, you'll build a three-tier application architecture where a web client calls your protected API, which then calls Microsoft Graph on behalf of the signed-in user. This demonstrates real-world patterns for building secure multi-tier applications.

## Learning Objectives

By completing this lab, you will:
- Expose and protect a custom Web API with Microsoft Entra ID
- Understand custom API scopes vs Microsoft Graph permissions
- Implement scope-based authorization in ASP.NET Core
- Use the On-Behalf-Of (OBO) flow to call downstream APIs
- Understand token audience validation
- Explore the difference between access tokens and ID tokens

## Architecture

```
┌─────────────┐
│   Browser   │
└──────┬──────┘
       │ 1. User signs in (OIDC)
       │ 2. Gets ID token
       ↓
┌─────────────────────┐
│   ClientWeb (5001)  │  Razor Pages App
└──────┬──────────────┘
       │ 3. Requests access token for API
       │    Scope: api://[API-ID]/api.read
       ↓
┌────────────────────────┐
│ MiddleTierApi (5002)   │  Protected Web API
└──────┬─────────────────┘
       │ 4. Validates access token
       │ 5. Uses OBO to get Graph token
       │ 6. Calls Microsoft Graph
       ↓
┌─────────────────┐
│ Microsoft Graph │
└─────────────────┘
```

## Prerequisites

- Completion of Module 1 labs
- Microsoft Entra ID tenant with permissions to create app registrations
- .NET 8.0 SDK installed
- Understanding of OAuth 2.0 and tokens

## Part 1: Register the Protected API (5 minutes)

### Step 1.1: Create the API App Registration

1. Navigate to [Microsoft Entra admin center](https://entra.microsoft.com)
2. Go to **Entra ID** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `Labs-MiddleTierApi`
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: Leave blank
5. Click **Register**

### Step 1.2: Expose an API Scope

1. In your API app registration, go to **Expose an API**
2. Click **Add a scope** (you may need to set Application ID URI first)
3. Accept the default Application ID URI: `api://[your-api-client-id]`
4. Click **Save and continue**
5. Configure the scope:
   - **Scope name**: `api.read`
   - **Who can consent?**: Admins and users
   - **Admin consent display name**: `Read API data`
   - **Admin consent description**: `Allows the app to read data from the protected API`
   - **User consent display name**: `Read your API data`
   - **User consent description**: `Allows the app to read data on your behalf`
   - **State**: Enabled
6. Click **Add scope**

**✏️ Note the Application ID URI**: `api://[your-api-client-id]`

### Step 1.3: Add Microsoft Graph Permissions (for OBO)

The API needs delegated permissions to call Microsoft Graph on behalf of users:
This is probably already defined by default, but go check it out.

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph** > **Delegated permissions**
4. Search for and select:
   - `User.Read`
5. Click **Add permissions**
6. (Optional) Click **Grant admin consent** to pre-consent for all users


### Step 1.5: Create a Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Description: `Lab3 API Secret`
4. Expires: 6 months (or as per your org policy)
5. Click **Add**
6. **⚠️ IMPORTANT**: Copy the secret **Value** immediately (not the Secret ID)

### Step 1.6: Record API Configuration

Copy these values - you'll need them shortly:

```
API Tenant ID:    [your-tenant-id]
API Client ID:    [your-api-client-id]
API Client Secret: [your-api-client-secret]
API Scope:        api://[your-api-client-id]/api.read
```

## Part 2: Register the Client Web App (5 minutes)

### Step 2.1: Create the Client App Registration

1. Go to **App registrations** > **New registration**
2. Configure:
   - **Name**: `Labs-ClientWeb`
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: 
     - Platform: **Web**
     - URI: `https://localhost:5001/signin-oidc`
3. Click **Register**

### Step 2.2: Configure Authentication

1. Go to **Authentication**
2. Under **Implicit grant and hybrid flows**, check:
   - ✅ **ID tokens** (used for implicit and hybrid flows)
3. Click **Save**

### Step 2.3: Add API Permissions

The client needs permission to call your protected API:

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **My APIs** tab
4. Click on **Labs-MiddleTierApi**
5. Select **Delegated permissions**
6. Check `api.read`
7. Click **Add permissions**
8. The permission list should now show:
   - `Microsoft Graph > User.Read` (default)
   - `Labs-MiddleTierApi > api.read`

### Step 2.4: Create a Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Description: `Lab3 Client Secret`
4. Expires: 6 months (or as per your org policy)
5. Click **Add**
6. **⚠️ IMPORTANT**: Copy the secret **Value** immediately (not the Secret ID)

### Step 2.5: Record Client Configuration

Copy these values:

```
Client Tenant ID: [your-tenant-id]
Client Client ID: [your-client-client-id]
Client Client Secret: [your-client-client-secret]
```

### Step 2.6: Configure Known Client Applications (Critical for Combined Consent)

This step enables **combined consent** for the OBO flow. When configured correctly, users will see a single consent prompt that covers both the client calling your API AND your API calling Microsoft Graph.

**Why this is required:**

Without this configuration, the On-Behalf-Of flow fails because:
- The user consented to the client calling your API ✅
- But the user didn't consent to your API calling Microsoft Graph ❌
- This creates a "consent gap" in the authentication chain

**Configuration steps:**

1. Go back to your **API app registration** (`Labs-MiddleTierApi`)
2. Click **Manifest** in the left menu
3. Find the `knownClientApplications` property (it should be an empty array: `[]`)
4. Add your Client app's Client ID to the array:
   ```json
   "knownClientApplications": [
       "[your-client-client-id-from-step-2.5]"
   ],
   ```
5. Click **Save** at the top

**⚠️ Important**: Use the Client ID from Step 2.5, not the API's Client ID.

**✅ Verification**: The manifest should now show your client app ID in the `knownClientApplications` array.

**What this enables:**
```
User signs into ClientWeb
    ↓
Entra ID sees client is in knownClientApplications
    ↓
Entra ID presents combined consent showing:
  ✓ ClientWeb accessing MiddleTierApi (.default scope)
  ✓ MiddleTierApi accessing Microsoft Graph (User.Read)
    ↓
User clicks Accept once
    ↓
OBO flow works seamlessly!
```

## Part 3: Configure the Applications (5 minutes)

### Step 3.1: Configure the API

1. Navigate to `src/Module2/Labs.MiddleTierApi`
2. Open `appsettings.json`
3. Update the configuration:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourtenant.onmicrosoft.com",
    "TenantId": "[API Tenant ID from Step 1.5]",
    "ClientId": "[API Client ID from Step 1.5]",
    "ClientSecret": "[API Client Secret from Step 1.5]"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### Step 3.2: Configure the Client Web App

1. Navigate to `src/Module2/Labs.ClientWeb`
2. Open `appsettings.json`
3. Update the configuration:

```json
{
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "yourtenant.onmicrosoft.com",
    "TenantId": "[Client Tenant ID from Step 2.6]",
    "ClientId": "[Client Client ID from Step 2.6]",
    "ClientSecret": "[API Client Secret from Step 2.6]",
    "CallbackPath": "/signin-oidc"
  },
  "DownstreamApi": {
    "BaseUrl": "https://localhost:5002",
    "Scopes": [ "api://[your-api-client-id]/.default" ]
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**⚠️ Important**: Replace `[your-api-client-id]` with your actual API Client ID. Note that we use the `.default` scope, which is required for combined consent with `knownClientApplications`.

### Step 3.3: Update CallApi.cshtml.cs

The ClientWeb code has a placeholder that needs to be updated:

1. Open `src/Module2/Labs.ClientWeb/Pages/CallApi.cshtml.cs`
2. Find the line with `[AuthorizeForScopes(Scopes = new[] { "api://YOUR_API_CLIENT_ID/.default" })]`
3. Replace `YOUR_API_CLIENT_ID` with your actual API Client ID:

```csharp
[AuthorizeForScopes(Scopes = new[] { "api://[your-api-client-id]/.default" })]
```

**💡 Why `.default`?** The `.default` scope tells Entra ID to request all statically configured permissions for the API. Combined with `knownClientApplications`, this enables the combined consent flow that includes both the API's scopes and its downstream Graph permissions.

## Part 4: Run and Test (7 minutes)

### Step 4.0: Trust the Development Certificate (First Time Setup)

If this is your first time running ASP.NET Core applications with HTTPS, you need to trust the development certificate:

```bash
dotnet dev-certs https --trust
```

This command generates and trusts a development HTTPS certificate on your machine. You only need to do this once.

**If you see SSL errors**, clean up and regenerate the certificate:

```bash
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

### Step 4.1: Start the API

1. Open a terminal and navigate to `src/Module2/Labs.MiddleTierApi`
2. Run the API:
   ```bash
   dotnet run
   ```
3. The API should start on `https://localhost:5002`
4. You should see: `Now listening on: https://localhost:5002`

### Step 4.2: Start the Client Web App

1. Open a **new terminal** and navigate to `src/Module2/Labs.ClientWeb`
2. Run the client:
   ```bash
   dotnet run
   ```
3. The app should start on `https://localhost:5001`
4. Open your browser to `https://localhost:5001`

### Step 4.3: Test Authentication

1. On the home page, click **Sign In**
2. Sign in with your Microsoft Entra ID account
3. You may be prompted to consent to permissions:
   - ✅ Sign you in and read your profile
   - ✅ Read API data (or similar for your custom scope)
4. Click **Accept**
5. You should be redirected back to the home page, now authenticated

### Step 4.4: Test the API Endpoints

Click **Call Protected API** and test each endpoint:

#### Test 1: /api/whoami
- **Purpose**: View the access token claims
- **Expected**: Success with token metadata
- **Key Observations**:
  - Token audience (`aud`) should be `api://[your-api-client-id]`
  - Scope (`scp`) should contain `api.read`
  - No `roles` claim (this is delegated permission)
  - `oid` claim identifies the user

#### Test 2: /api/secure
- **Purpose**: Scope-protected endpoint
- **Expected**: Success message: "Hello, [your-name]!"
- **Key Observations**:
  - Endpoint validates the `api.read` scope
  - Returns timestamp and scope information
  - This is the simplest protected API pattern

#### Test 3: /api/graphme
- **Purpose**: Call Microsoft Graph using OBO flow
- **Expected**: Your user profile from Microsoft Graph
- **Key Observations**:
  - API exchanges your access token for a Graph token
  - Graph token is requested "on behalf of" you
  - Returns your display name, email, and job title
  - This demonstrates the full OBO flow

### Step 4.5: View ID Token

Click **View ID Token** to see your identity claims:
- Compare the ID token to the access token from `/api/whoami`
- Notice the audience difference:
  - ID token `aud`: [client-app-id]
  - Access token `aud`: api://[api-client-id]

## Key Concepts Explained

### Access Token vs ID Token

| Property | ID Token | Access Token |
|----------|----------|--------------|
| **Purpose** | Proves user identity | Grants access to resources |
| **Audience** | Client application | Protected API |
| **Contains** | User claims (name, email) | Permissions (scopes, roles) |
| **Used by** | Client app | Resource server (API) |
| **Flow** | OIDC authentication | OAuth 2.0 authorization |

### Custom API Scopes

```
api://[api-client-id]/api.read
│      │               │
│      │               └─ Scope name (you define)
│      └─ Your API's unique identifier
└─ Indicates this is an API scope
```

- **Delegated**: User must be signed in
- **Requires consent**: User (or admin) must approve
- **Included in token**: Shows up as `scp` claim

### On-Behalf-Of (OBO) Flow

```
1. User → ClientWeb: Sign in
2. ClientWeb → Entra ID: Request access token for MiddleTierApi
3. Entra ID → ClientWeb: Access token (aud: api://[api-id], scp: api.read)
4. ClientWeb → MiddleTierApi: Call API with access token
5. MiddleTierApi validates token
6. MiddleTierApi → Entra ID: Exchange token for Graph token (OBO)
7. Entra ID → MiddleTierApi: Graph access token (aud: https://graph.microsoft.com)
8. MiddleTierApi → Graph: Call /me with Graph token
9. Graph → MiddleTierApi: User profile
10. MiddleTierApi → ClientWeb: Return profile
```

**Why OBO?**
- Maintains user context through the call chain
- API acts on behalf of the user, not itself
- Respects user permissions in downstream APIs
- Audit trails show the actual user, not just the API

### Scope-Based Authorization

In the API code (`SecureController.cs`):

```csharp
[Authorize(Policy = "RequireApiReadScope")]
public IActionResult Secure()
```

The policy checks for the `api.read` scope:

```csharp
options.AddPolicy("RequireApiReadScope", policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireClaim("scp", "api.read");
});
```

### Known Client Applications and Combined Consent

The **Known Client Applications** pattern enables a seamless consent experience for multi-tier applications:

**How it works:**

1. You configure the API's manifest to list trusted client applications
2. When a user consents to the client app accessing your API, Entra ID automatically includes consent for the API's downstream dependencies
3. This creates a single, unified consent prompt instead of multiple consent dialogs

**Benefits:**

- **Separation of concerns**: Client app doesn't need to know what downstream APIs the middle-tier calls
- **Single consent experience**: User sees one consent dialog instead of being prompted multiple times
- **Maintainability**: When the API adds new Graph permissions, no changes needed in client app code
- **Security**: Only explicitly trusted clients can trigger combined consent

**Example consent flow:**

```
Without Known Client Apps:
  User → ClientWeb → Consent for api.read → Success
  Later: API calls Graph → ❌ MsalUiRequiredException (no consent for Graph)

With Known Client Apps:
  User → ClientWeb → Combined consent prompt showing:
    ✓ ClientWeb accessing MiddleTierApi (api.read)
    ✓ MiddleTierApi accessing Microsoft Graph (User.Read)
  → All permissions granted in one step → ✅ OBO flow works!
```

This pattern is particularly important for:
- Multi-tier architectures with OBO flows
- When middle-tier APIs call multiple downstream services
- Enterprise applications where user consent is required (not admin pre-consent)
- Guest user scenarios where admin consent doesn't apply

## Common Issues and Solutions

### Issue: "AADSTS65001: The user or administrator has not consented"

**Solution**:
1. In the ClientWeb app registration, go to **API permissions**
2. Ensure `api.read` permission is added for your API
3. Try clicking **Grant admin consent** (if you have admin rights)
4. Or, sign out and sign in again to trigger user consent

### Issue: "Invalid audience" when calling API

**Solution**:
1. Check that the API's `appsettings.json` has the correct `ClientId`
2. Verify the token audience in `/api/whoami` matches `api://[api-client-id]`
3. Ensure the ClientWeb `DownstreamApi.Scopes` uses the full scope URI

### Issue: OBO flow fails - "AADSTS50013" or "IDW10502: MsalUiRequiredException"

**Root Cause**: The API needs to call Microsoft Graph on behalf of the user, but the user hasn't consented to the API calling Graph.

**Solution**:
1. Verify you completed **Step 2.6** to configure `knownClientApplications` in the API's manifest
2. Check the API's manifest has your client app ID in `knownClientApplications`
3. Verify the client is using the `.default` scope in `appsettings.json` and `CallApi.cshtml.cs`
4. Verify the API app registration has Microsoft Graph `User.Read` delegated permission (Step 1.3)
5. Check that the API's client secret is correct in `appsettings.json`
6. Sign out completely from the client app and sign in again to trigger the combined consent prompt
7. Alternative: Grant admin consent for the API's Graph permissions (Step 1.3)

**To verify the configuration:**
- Check the API manifest: `"knownClientApplications": ["your-client-app-id"]`
- Check the client's `appsettings.json`: `"Scopes": [ "api://[api-id]/.default" ]`
- Check the client's `CallApi.cshtml.cs`: `[AuthorizeForScopes(Scopes = new[] { "api://[api-id]/.default" })]`
- When signing in to the client, you should see consent for both the API scope AND Graph permissions in a single prompt

### Issue: Cannot access /api/secure - 403 Forbidden

**Solution**:
1. Check the access token has the `api.read` scope (use `/api/whoami`)
2. Verify the authorization policy name matches in `Program.cs` and the controller
3. Ensure the scope claim type is correct (`scp` or `http://schemas.microsoft.com/identity/claims/scope`)

## Cleanup

When finished:
1. Press `Ctrl+C` in both terminal windows to stop the applications
2. (Optional) Delete the app registrations if no longer needed

## What You've Learned

✅ How to expose and protect custom API scopes  
✅ Difference between ID tokens and access tokens  
✅ Token audience validation and why it matters  
✅ Scope-based authorization in ASP.NET Core  
✅ On-Behalf-Of (OBO) flow mechanics  
✅ Chaining API calls while maintaining user context  
✅ How to configure multi-tier authentication  

## Next Steps

- [Lab 4: Cross-Tenant Daemon →](Lab4_CrossTenantDaemon.md)
- Explore the API code in `Labs.MiddleTierApi/Controllers/SecureController.cs`
- Try adding additional scopes (e.g., `api.write`)
- Experiment with application roles for API authorization

## Additional Resources

- [Protected web API: Verify scopes and app roles](https://docs.microsoft.com/en-us/azure/active-directory/develop/scenario-protected-web-api-verification-scope-app-roles)
- [Microsoft identity platform and OAuth 2.0 On-Behalf-Of flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-on-behalf-of-flow)
- [Scopes and permissions in the Microsoft identity platform](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent)
