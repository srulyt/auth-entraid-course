# Lab 4: Cross-Tenant Daemon Application

**Duration**: 12-15 minutes  
**Difficulty**: Intermediate

## Overview

In this lab, you'll build a daemon (background service) application that authenticates without user interaction and can access resources across multiple Microsoft Entra ID tenants. This demonstrates service-to-service authentication patterns commonly used in enterprise scenarios.

## Learning Objectives

By completing this lab, you will:
- Understand app-only (daemon) authentication vs delegated authentication
- Implement the client credentials flow
- Configure multi-tenant applications
- Work with application permissions instead of delegated permissions
- Acquire tokens for different tenants
- Call Microsoft Graph with application-level access

## Architecture

```
┌──────────────────────┐
│ Daemon Console App   │
└──────────┬───────────┘
           │ 1. Client credentials flow
           │    (no user interaction)
           ↓
┌────────────────────────┐
│ Entra ID (Home Tenant) │
└──────────┬─────────────┘
           │ 2. App-only access token
           │    (aud: https://graph.microsoft.com)
           ↓
┌──────────────────────┐
│  Microsoft Graph     │
│  (Home Tenant data)  │
└──────────────────────┘

           OR

┌──────────────────────┐
│ Daemon Console App   │
└──────────┬───────────┘
           │ 3. Client credentials + tenant ID
           ↓
┌───────────────────────────┐
│ Entra ID (Customer Tenant)│
└──────────┬────────────────┘
           │ 4. App-only access token
           ↓
┌──────────────────────┐
│  Microsoft Graph     │
│ (Customer Tenant)    │
└──────────────────────┘
```

## Prerequisites

- Completion of Module 1 and Lab 3
- Microsoft Entra ID tenant (your "home" tenant)
- (Optional) Access to a second tenant for cross-tenant testing
- Admin rights to grant application permissions
- .NET 8.0 SDK installed

## Key Concepts

### Delegated vs Application Permissions

| Aspect | Delegated | Application |
|--------|-----------|-------------|
| **User context** | ✅ Requires signed-in user | ❌ No user required |
| **Consent type** | User or admin | Admin only |
| **Access level** | User's permissions | Tenant-wide access |
| **Token type** | Access token with user claims | Access token with app claims |
| **Use case** | User acting through app | Background services |
| **Example** | Read user's email | Read all users' email |

### Client Credentials Flow

```
1. Daemon → Entra ID: POST /token
   - grant_type: client_credentials
   - client_id: [app-id]
   - client_secret: [secret]
   - scope: https://graph.microsoft.com/.default

2. Entra ID validates credentials

3. Entra ID → Daemon: Access token
   - No user claims (oid, upn, etc.)
   - Contains app identity (appid, tid)
   - Includes application permissions (roles)
```

## Part 1: Register the Daemon Application (5 minutes)

### Step 1.1: Create App Registration

1. Navigate to [Microsoft Entra admin center](https://entra.microsoft.com)
2. Go to **Identity** > **Applications** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `Labs-CrossTenantDaemon`
   - **Supported account types**: 
     - For single tenant: **Accounts in this organizational directory only**
     - For cross-tenant: **Accounts in any organizational directory (Any Microsoft Entra ID tenant - Multitenant)**
   - **Redirect URI**: Leave blank (daemon apps don't use redirect URIs)
5. Click **Register**

### Step 1.2: Create Client Secret

1. Go to **Certificates & secrets**
2. Click **New client secret**
3. Configure:
   - **Description**: `Lab4 Daemon Secret`
   - **Expires**: 6 months (or per policy)
4. Click **Add**
5. **⚠️ CRITICAL**: Copy the secret **Value** immediately

### Step 1.3: Add Application Permissions

Add Microsoft Graph application permissions:

1. Go to **API permissions**
2. Click **Add a permission**
3. Select **Microsoft Graph** > **Application permissions** > **Application Permissions**
4. Search for and select:
   - `Organization.Read.All` - Read organization information
   - `User.Read.All` - Read all users' basic profiles
5. Click **Add permissions**

**⚠️ Important**: Application permissions require admin consent

### Step 1.4: Grant Admin Consent

1. In **API permissions**, click **Grant admin consent for [Your Tenant]**
2. Review the permissions
3. Click **Yes** to confirm
4. Wait for the status to show green checkmarks

### Step 1.5: Record Configuration

Copy these values:

```
Home Tenant ID:    [your-tenant-id]
Client ID:         [daemon-client-id]
Client Secret:     [daemon-client-secret]
```

If testing cross-tenant:
```
Customer Tenant ID: [other-tenant-id]
```

## Part 2: Configure the Daemon Application (3 minutes)

### Step 2.1: Update appsettings.json

1. Navigate to `src/Module2/Labs.CrossTenantDaemon`
2. Open `appsettings.json`
3. Update with your values:

```json
{
  "Daemon": {
    "HomeTenantId": "[Home Tenant ID from Step 1.6]",
    "CustomerTenantId": "CUSTOMER_TENANT_ID",
    "ClientId": "[Client ID from Step 1.6]",
    "ClientSecret": "[Client Secret from Step 1.6]"
  }
}
```

**Notes**:
- If you don't have a customer tenant, use the same tenant ID for both
- For production, use Azure Key Vault or managed identities instead of appsettings.json

## Part 3: Run and Test (7 minutes)

### Step 3.1: Build and Run

1. Open a terminal
2. Navigate to `src/Module2/Labs.CrossTenantDaemon`
3. Run the application:
   ```bash
   dotnet run
   ```

You should see the interactive menu:
```
================================================================================
Lab 4: Cross-Tenant Daemon (App-Only Authentication)
================================================================================

Choose an option:
1. Acquire token for home tenant
2. Acquire token for customer tenant
3. Call Graph /organization (customer tenant)
4. Call Graph /users?$top=1 (customer tenant)
5. Display last acquired token
0. Exit
>
```

### Step 3.2: Test Token Acquisition (Home Tenant)

1. Enter `1` to acquire a token for your home tenant
2. Observe the output:
   ```
   Acquiring app-only token for Home Tenant ([tenant-id])...
   ✓ Token acquired successfully!
     Expires: 2024-12-15 02:00:00
   ```

### Step 3.3: Inspect the Token

1. Enter `5` to display the token details
2. Review the decoded token:

**Expected claims**:
- `aud`: `https://graph.microsoft.com` (audience)
- `iss`: `https://sts.windows.net/[tenant-id]/` (issuer)
- `appid`: Your daemon's client ID
- `tid`: Your tenant ID
- `roles`: Application permissions granted (e.g., `Organization.Read.All`, `User.Read.All`)
- **No user claims**: No `oid`, `upn`, `name` (this is app-only)

**Key observations**:
- This is an app-only token (no user identity)
- The `roles` claim contains application permissions
- Token is valid for the entire tenant
- Default lifetime is typically 60-90 minutes

### Step 3.4: Call Microsoft Graph

1. Enter `3` to call Graph `/organization` endpoint
2. Review the response:
   ```json
   {
     "value": [
       {
         "id": "[org-id]",
         "displayName": "Your Organization",
         "verifiedDomains": [...]
       }
     ]
   }
   ```

3. Enter `4` to call Graph `/users?$top=1`
4. Review the response showing the first user in your tenant

**What's happening**:
- The daemon uses its app-only token
- Calls are made without user context
- Access is granted based on application permissions
- The app can read data across the entire tenant

### Step 3.5: (Optional) Test Cross-Tenant

If you configured a customer tenant:

1. Enter `2` to acquire a token for the customer tenant
2. The flow is identical, but targets a different tenant
3. Test with option `3` or `4` to call Graph in the customer tenant

**Prerequisites for cross-tenant**:
- The app must be multi-tenant (Step 1.5)
- Admin in customer tenant must consent to the app
- Customer tenant ID must be configured

## Part 4: Cross-Tenant Consent (Optional)

If testing across tenants, the customer tenant must grant consent:

### Method 1: Admin Consent URL

1. Build the consent URL:
   ```
   https://login.microsoftonline.com/{customer-tenant-id}/adminconsent
   ?client_id={your-daemon-client-id}
   &redirect_uri=https://localhost
   ```

2. Have an admin from the customer tenant visit this URL
3. Review and accept the permissions
4. After consent, the app can acquire tokens for that tenant

### Method 2: Azure Portal (Customer Tenant)

1. Customer tenant admin logs into Azure Portal
2. Go to **Enterprise Applications**
3. Find your app (or add via consent prompt)
4. Grant admin consent for the required permissions

## Key Concepts Explained

### The .default Scope

In the code, you'll see:
```csharp
var scopes = new[] { "https://graph.microsoft.com/.default" };
```

The `.default` scope means:
- Request all application permissions granted to this app
- Simplified scope syntax for daemon apps
- Alternative to listing individual permissions

### Token Differences: Delegated vs App-Only

**Delegated token** (from Lab 3):
```json
{
  "aud": "api://[api-id]",
  "scp": "api.read",
  "oid": "[user-object-id]",
  "upn": "user@tenant.com",
  "name": "John Doe"
}
```

**App-only token** (this lab):
```json
{
  "aud": "https://graph.microsoft.com",
  "roles": ["Organization.Read.All", "User.Read.All"],
  "appid": "[daemon-client-id]",
  "tid": "[tenant-id]"
  // No user claims
}
```

### Multi-Tenant Applications

When you mark an app as multi-tenant:
- It can be used in any Entra ID tenant
- Each tenant must explicitly consent
- The app has separate identities in each tenant
- Useful for ISV scenarios or managed services

**Security consideration**: Multi-tenant apps require careful design to prevent cross-tenant data leakage.

### Tenant-Specific vs Common Endpoint

```csharp
// Tenant-specific (recommended for daemons)
.WithAuthority($"https://login.microsoftonline.com/{tenantId}")

// Common endpoint (multi-tenant, not for daemons)
.WithAuthority("https://login.microsoftonline.com/common")
```

For daemon apps, always use tenant-specific endpoints.

## Common Issues and Solutions

### Issue: "AADSTS7000215: Invalid client secret is provided"

**Solution**:
1. Verify you copied the secret **Value**, not the **Secret ID**
2. Check for extra spaces in `appsettings.json`
3. Regenerate the secret if needed

### Issue: "Insufficient privileges to complete the operation"

**Solution**:
1. Verify application permissions are configured (not delegated)
2. Ensure admin consent was granted (green checkmarks)
3. Wait a few minutes after granting consent for changes to propagate

### Issue: "AADSTS700016: Application was not found in the directory"

**Solution**:
1. Verify the tenant ID is correct
2. For multi-tenant apps, ensure consent was granted in the target tenant
3. Check that the app is registered as multi-tenant if accessing other tenants

### Issue: Token has roles claim but Graph call fails

**Solution**:
1. Verify the specific permission needed (e.g., `User.Read.All` for `/users`)
2. Check that admin consent was granted
3. Confirm the token's `roles` claim includes the required permission

## Security Best Practices

### ✅ Do:
- Use certificates instead of client secrets in production
- Store secrets in Azure Key Vault or similar
- Use managed identities when running in Azure
- Implement the principle of least privilege
- Monitor and audit daemon app activity
- Rotate secrets regularly

### ❌ Don't:
- Commit secrets to source control
- Grant more permissions than needed
- Use the same credentials across environments
- Ignore token expiration and renewal
- Allow unrestricted multi-tenant access without validation

## Cleanup

1. Press `0` to exit the application
2. (Optional) Delete the app registration if no longer needed
3. (Optional) Revoke admin consent in customer tenants

## What You've Learned

✅ Difference between delegated and application permissions  
✅ Client credentials flow for app-only authentication  
✅ How to configure and consent to application permissions  
✅ Multi-tenant application registration  
✅ Cross-tenant token acquisition  
✅ Token inspection for app-only scenarios  
✅ Calling Microsoft Graph without user context  

## Real-World Use Cases

Daemon apps with application permissions are used for:
- **Background sync services**: Syncing data between systems
- **Reporting and analytics**: Aggregating data across users
- **Security monitoring**: Analyzing logs and audit data
- **User provisioning**: Automating user lifecycle management
- **Backup services**: Backing up organizational data
- **Compliance tools**: Scanning content for policy violations

## Next Steps

- Explore the code in `Labs.CrossTenantDaemon/Program.cs`
- Try adding different Graph API calls (e.g., `/groups`, `/domains`)
- Experiment with different application permissions
- Convert to use certificates instead of client secrets
- Implement token caching for production scenarios

## Additional Resources

- [Microsoft identity platform and the OAuth 2.0 client credentials flow](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-oauth2-client-creds-grant-flow)
- [Application permissions vs delegated permissions](https://docs.microsoft.com/en-us/azure/active-directory/develop/v2-permissions-and-consent)
- [Multi-tenant applications](https://docs.microsoft.com/en-us/azure/active-directory/develop/howto-convert-app-to-be-multi-tenant)
- [Microsoft Graph permissions reference](https://docs.microsoft.com/en-us/graph/permissions-reference)
- [Use client credentials with certificate](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-client-credentials)

---

**Congratulations!** You've completed Module 2. You now understand both user-based (delegated) and app-based (application) authentication patterns with Microsoft Entra ID.
