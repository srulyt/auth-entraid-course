# Lab 5: Public Client CLI Authentication

**Duration**: 45-60 minutes  
**Difficulty**: Intermediate

## Overview

In this lab, you'll configure and use a command-line tool that demonstrates public client authentication with Microsoft Entra ID. Unlike web applications that can securely store client secrets, CLI tools are "public clients" that cannot protect secrets. You'll learn how modern OAuth flows (PKCE and Device Code) solve this security challenge.

## Learning Objectives

By completing this lab, you will:
- Understand why client secrets cannot be used in CLI/desktop/mobile apps
- Configure a public client app registration in Entra ID
- Use Authorization Code + PKCE flow for interactive authentication
- Use Device Code flow for limited-input scenarios
- Explore token caching and security trade-offs
- Call Microsoft Graph API from a CLI tool
- Implement security best practices for public clients

## Architecture

```
┌─────────────────┐
│   CLI Tool      │  Public Client (no secret)
└────────┬────────┘
         │ 1. Auth Code + PKCE flow
         │    (opens browser)
         ↓
┌─────────────────────┐
│  Microsoft Entra ID │
└────────┬────────────┘
         │ 2. Returns access token
         │    (no secret validation)
         ↓
┌─────────────────┐
│   CLI Tool      │
└────────┬────────┘
         │ 3. Calls Graph with token
         ↓
┌─────────────────┐
│ Microsoft Graph │
└─────────────────┘
```

## Prerequisites

- .NET 8.0 SDK installed
- Microsoft Entra ID tenant with permissions to create app registrations
- Command-line terminal (PowerShell, bash, or cmd)
- Understanding of OAuth 2.0 and tokens from Module 1

## Part 1: Understanding Public Clients (5 minutes)

### What Makes a Client "Public"?

**Public clients** are applications that run on user devices where code and data can be inspected:
- Command-line tools
- Desktop applications
- Mobile apps
- Single-page applications (SPAs)

**Why secrets don't work**:
```
❌ Embedding a secret in a CLI tool:

const string CLIENT_SECRET = "xAb3...";  // In your code

Problems:
1. Anyone can decompile your application
2. Secret visible in process memory
3. Secret can be extracted from binary
4. If one user extracts it, everyone's security is compromised
```

**The solution**: Use cryptographic proof (PKCE) instead of shared secrets.

### PKCE in 60 Seconds

**Without PKCE** (why it's needed):
```
Attacker intercepts authorization code
  → Attacker exchanges code for token
  → Attacker has access to user's data ❌
```

**With PKCE**:
```
1. CLI generates random secret (code_verifier)
2. CLI hashes it (code_challenge = SHA256(code_verifier))
3. CLI sends code_challenge to Entra ID
4. Entra ID returns authorization code
5. CLI sends code + original code_verifier
6. Entra ID verifies: SHA256(code_verifier) == code_challenge
7. Only if match: return token ✓

Even if attacker intercepts code, they don't have code_verifier!
```

## Part 2: Create Public Client App Registration (10 minutes)

### Step 2.1: Register the Application

1. Navigate to [Microsoft Entra admin center](https://entra.microsoft.com)
2. Go to **Entra ID** > **App registrations**
3. Click **New registration**
4. Configure:
   - **Name**: `Labs-PublicClientCLI`
   - **Supported account types**: Accounts in this organizational directory only
   - **Redirect URI**: 
     - Platform: **Public client/native (mobile & desktop)**
     - URI: `http://localhost`
5. Click **Register**

**⚠️ Important**: Notice we selected "Public client/native" - this tells Entra ID not to expect a client secret.

### Step 2.2: Enable Public Client Flows

1. In your app registration, go to **Authentication**
2. Scroll down to **Advanced settings**
3. Under **Allow public client flows**, set to **Yes**
4. Click **Save**

**Why this matters**: This enables Device Code flow, which we'll test later in the lab.

### Step 2.3: Configure API Permissions

The default permission is already sufficient:

1. Go to **API permissions**
2. Verify `Microsoft Graph > User.Read` is listed (this is added by default)
3. **Optional**: Click **Grant admin consent** if you have admin rights

**Note**: `User.Read` is a delegated permission - the CLI will act on behalf of the signed-in user.

### Step 2.4: Record Configuration Values

Copy these values - you'll need them shortly:

```
Tenant ID:  [your-tenant-id]
Client ID:  [your-client-id]
```

**Where to find them**:
- Both are visible on the app registration **Overview** page
- Tenant ID is also called "Directory ID"
- Client ID is also called "Application ID"

## Part 3: Build and Publish the CLI (5 minutes)

### Step 3.1: Publish the CLI Tool

Instead of running with `dotnet run`, we'll publish the CLI tool so you can run it directly with the `entra-lab` command.

**Windows (PowerShell):**
1. Open PowerShell and navigate to the Module 3 directory:
   ```powershell
   cd src/Module3
   ```

2. Run the publish script:
   ```powershell
   powershell -ExecutionPolicy Bypass -File .\publish.ps1
   ```
   
   **Note**: The `-ExecutionPolicy Bypass` flag is needed if your system blocks script execution.

3. Add the CLI to your PATH for this session:
   ```powershell
   $env:PATH = "$PWD\Labs.Cli\publish;$env:PATH"
   ```
   
   **Verify it worked**:
   ```powershell
   entra-lab --version
   ```

**macOS/Linux (Bash):**
1. Open Terminal and navigate to the Module 3 directory:
   ```bash
   cd src/Module3
   ```

2. Make the script executable and run it:
   ```bash
   chmod +x publish.sh
   ./publish.sh
   ```

3. Add the CLI to your PATH for this session:
   ```bash
   export PATH="$(pwd)/Labs.Cli/publish:$PATH"
   ```

**Note**: The PATH change only affects your current terminal session. When you close the terminal, the CLI will no longer be in your PATH (which is fine for this lab).

### Step 3.2: Verify Installation

Run the CLI without any arguments to see the help:
```bash
entra-lab
```

You should see the help message listing available commands:
```
Entra Lab CLI - Microsoft Entra ID Public Client Authentication

Commands:
  login     Sign in to Microsoft Entra ID
  logout    Sign out and clear cached tokens
  account   Manage account information
  token     Manage and display tokens
  graph     Call Microsoft Graph API
  config    Manage application configuration
  diagnose  Check configuration and connectivity
```

### Step 3.3: Configure with Your Tenant and Client IDs

**Option A: Using the config command** (recommended):
```bash
entra-lab config set --tenant YOUR_TENANT_ID --client YOUR_CLIENT_ID
```

**Option B: Edit appsettings.json manually**:

Navigate to `src/Module3/Labs.Cli/publish/appsettings.json` and update:
```json
{
  "PublicClient": {
    "TenantId": "YOUR_TENANT_ID",
    "ClientId": "YOUR_CLIENT_ID",
    "RedirectUri": "http://localhost"
  },
  "Graph": {
    "BaseUrl": "https://graph.microsoft.com/v1.0",
    "Scopes": [ "User.Read" ]
  }
}
```

### Step 3.4: Verify Configuration

```bash
entra-lab config show
```

Expected output:
```
================================================================================
Application Configuration
================================================================================

✓ Configuration is valid

Tenant ID           : xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx
Client ID           : yyyyyyyy-yyyy-yyyy-yyyy-yyyyyyyyyyyy
Redirect URI        : http://localhost
Graph Base URL      : https://graph.microsoft.com/v1.0
Graph Scopes        : User.Read
```

### Step 3.5: Run Diagnostics

```bash
entra-lab diagnose
```

This checks:
- ✓ Configuration validity
- ✓ Token cache status
- ✓ Authentication status
- ✓ Network connectivity to login.microsoftonline.com

## Part 4: Authorization Code + PKCE Flow (12 minutes)

### Step 4.1: Sign In with PKCE

Run the login command:
```bash
entra-lab login --mode pkce
```

**What happens**:
1. CLI generates a random `code_verifier` (PKCE secret)
2. CLI computes `code_challenge` = SHA256(code_verifier)
3. CLI opens your default browser
4. Browser navigates to Entra ID with code_challenge
5. You sign in and consent to permissions
6. Entra ID redirects back with authorization code
7. CLI exchanges code + code_verifier for access token
8. Entra ID validates code_verifier matches code_challenge
9. Token is returned and cached locally

Expected output:
```
================================================================================
Sign In to Microsoft Entra ID
================================================================================

ℹ Using Authorization Code + PKCE flow
Opening your default browser for authentication...

✓ Authentication successful!
Account             : yourname@yourtenant.com
Token expires       : 2024-12-23 23:30:00

Tokens are cached locally for future use.
Run 'entra-lab account show' to view account details.
```

### Step 4.2: View Account Information

```bash
entra-lab account show
```

Expected output shows your signed-in account details.

### Step 4.3: Explore the Access Token

```bash
entra-lab token show
```

**Key observations**:
- **Audience (aud)**: `https://graph.microsoft.com` (not your CLI client ID!)
- **Scopes (scp)**: `User.Read`
- **No client_id claim**: Token doesn't reveal which app requested it
- **Expires in**: ~60 minutes (tokens are short-lived)

**Try this**:
```bash
entra-lab token show --print-token
```

This shows truncated token (first/last 20 characters). Full token is never displayed by default for security.

### Step 4.4: Call Microsoft Graph

```bash
entra-lab graph me
```

Expected output:
```
ℹ Calling Microsoft Graph API: /me

================================================================================
User Profile from Microsoft Graph
================================================================================

Display Name        : Your Name
UPN                 : yourname@yourtenant.com
Email               : yourname@yourtenant.com
Job Title           : [Your job title if set]
...

✓ Graph API call successful!
```

**What happened**:
- CLI acquired cached access token (silent refresh)
- Made authenticated request to `https://graph.microsoft.com/v1.0/me`
- Graph API validated token and returned your profile

### Step 4.5: View Requested Scopes

```bash
entra-lab graph scopes
```

Shows which permissions the CLI is requesting. For this lab, only `User.Read`.

## Part 5: Device Code Flow (10 minutes)

### Step 5.1: Sign Out First

```bash
entra-lab logout
```

This clears cached tokens so we can test Device Code flow cleanly.

### Step 5.2: Sign In with Device Code

```bash
entra-lab login --mode device-code
```

**What happens**:
```
ℹ Using Device Code flow
This flow is useful for devices with limited input capabilities.

┌─ Device Code Authentication ────────────────────────────────┐
│ 1. Open your browser to: https://microsoft.com/devicelogin  │
│ 2. Enter this code: ABC-DEF-123                              │
└──────────────────────────────────────────────────────────────┘

Waiting for authentication...
```

### Step 5.3: Complete Authentication

1. Open a browser (on **any device** - this is the key advantage!)
2. Go to the URL shown (usually https://microsoft.com/devicelogin)
3. Enter the code displayed in your CLI
4. Sign in with your account
5. Consent to permissions
6. Return to your CLI

Expected output:
```
✓ Authentication successful!
Account             : yourname@yourtenant.com
Token expires       : 2024-12-23 23:45:00
```

### Step 5.4: When to Use Device Code Flow

**Use Device Code when**:
- Running in SSH/remote session (no local browser)
- Device has limited input (smart TV, IoT device)
- User wants to authenticate on a different device
- Browser redirect handling is problematic

**Use PKCE when**:
- Normal desktop/laptop environment
- Browser available on same device
- Better user experience (one less manual step)

## Part 6: Token Management (5 minutes)

### Step 6.1: View Token Cache Location

```bash
entra-lab diagnose
```

Look for the "Token Cache Check" section:
```
2. Token Cache Check
  Cache Location      : C:\Users\yourname\.entra-lab\msal_token_cache.bin
  ✓ Cache exists (2847 bytes)
```

**Security consideration**: Anyone with access to this file can read your tokens!

### Step 6.2: Clear Token Cache

```bash
entra-lab token clear
```

This deletes the cache file. You'll need to sign in again for future operations.

### Step 6.3: Verify Cache is Cleared

```bash
entra-lab account show
```

Expected output:
```
⚠ No account is currently signed in.

To sign in, run:
  entra-lab login --mode pkce
  entra-lab login --mode device-code
```

### Step 6.4: Sign In Again

```bash
entra-lab login --mode pkce
```

Notice the first sign-in after clearing cache requires full authentication. But subsequent commands use the cached token.

## Part 7: Security Deep Dive (8 minutes)

### Understanding the Trust Model

**Confidential Client (Web App)**:
```
Web Server (trusted environment)
  ├─ Client Secret stored securely
  ├─ Entra ID validates secret
  └─ Token stays on server (never exposed to browser)

Trust: Server infrastructure protects the secret
```

**Public Client (CLI Tool)**:
```
User's Device (untrusted environment)
  ├─ NO client secret (can't be protected)
  ├─ PKCE provides cryptographic proof
  └─ Token cached locally (user's responsibility)

Trust: Cryptographic binding prevents code interception
```

### Why PKCE Works

**The attack PKCE prevents**:
```
Without PKCE:
1. Attacker intercepts authorization code
2. Attacker replays code to token endpoint
3. ❌ Attacker gets access token

With PKCE:
1. Attacker intercepts authorization code (and code_challenge)
2. Attacker tries to replay code
3. Entra ID asks for code_verifier
4. Attacker doesn't have it (only CLI does)
5. ✓ Token exchange fails
```

**Key insight**: The code_verifier never leaves the CLI. Even if network traffic is intercepted, the attacker only sees the hashed code_challenge.

### Token Cache Security

**Risks**:
```
Local file: ~/.entra-lab/msal_token_cache.bin

Who can read it:
- You (the file owner)
- Administrator/root
- Malware running as your user
- Anyone with physical access to your device
```

**Mitigations**:
```
✓ Use OS-provided secure storage (Keychain/DPAPI)
  (This lab uses MSAL's built-in platform-specific storage)

✓ Clear cache on shared computers
  entra-lab token clear

✓ Understand token lifetime (typically 1 hour)
  entra-lab token show

✓ Log out when finished
  entra-lab logout
```

## Part 8: Common Scenarios and Troubleshooting (5 minutes)

### Scenario 1: Browser Doesn't Open

**Symptom**: PKCE flow fails to open browser

**Causes**:
- No default browser set
- Running in SSH/remote session
- Corporate policy blocks automatic browser launch

**Solution**: Use Device Code flow instead
```bash
entra-lab login --mode device-code
```

### Scenario 2: "unauthorized_client" Error

**Symptom**: Authentication fails with error code

**Cause**: "Allow public client flows" not enabled

**Solution**:
1. Go to app registration → Authentication
2. Advanced settings → Allow public client flows: **Yes**
3. Save and try again

### Scenario 3: Token Expired

**Symptom**: Graph API calls fail with 401

**Solution**: Sign in again (token auto-refresh should handle this, but if cache is corrupted):
```bash
entra-lab token clear
entra-lab login --mode pkce
```

### Scenario 4: Consent Required

**Symptom**: Error asking for admin consent

**Solution**:
- Admin pre-consents in Azure Portal (API permissions → Grant admin consent), OR
- User consents during first sign-in (if organization allows user consent)

## Key Concepts Review

### Public vs Confidential Clients

| Aspect | Confidential | Public |
|--------|-------------|--------|
| **Examples** | Web apps, APIs, daemons | CLI, desktop, mobile, SPA |
| **Secret** | Can protect client secret | Cannot protect secrets |
| **Flow** | Auth Code + secret | Auth Code + PKCE |
| **Trust** | Server infrastructure | Cryptographic proof |
| **Token Cache** | Server memory (secure) | Local file (user responsibility) |

### PKCE Parameters

| Parameter | Description | Example |
|-----------|-------------|---------|
| **code_verifier** | Random secret (43-128 chars) | `dBjftJeZ4CVP-mB92K27uhbUJU1p1r_wW1gFWFOEjXk` |
| **code_challenge** | SHA256 hash of code_verifier | `E9Melhoa2OwvFrEMTJguCHaoeK1t8URWbuGJSstw-cM` |
| **code_challenge_method** | Hashing algorithm | `S256` (SHA-256) |

### Device Code Flow Steps

1. CLI → Entra ID: Request device code
2. Entra ID → CLI: Return user_code + verification_uri
3. CLI displays: "Visit URL and enter code"
4. User → Browser: Goes to verification_uri
5. User → Browser: Enters user_code
6. User → Browser: Signs in and consents
7. CLI → Entra ID: Polls for token
8. Entra ID → CLI: Returns access token (after user completes auth)

## Cleanup

When finished with the lab:

```bash
# Sign out and clear tokens
entra-lab logout

# (Optional) Delete app registration in Azure Portal if no longer needed
```

## What You've Learned

✅ Why client secrets cannot be used in CLI/desktop/mobile apps  
✅ How Authorization Code + PKCE flow protects public clients  
✅ How Device Code flow enables authentication on different devices  
✅ Token caching strategies and security trade-offs  
✅ The difference between public and confidential client trust models  
✅ How to call Microsoft Graph from a public client application  
✅ Best practices for building secure CLI tools with Entra ID  

## Next Steps

Now that you understand public client authentication:

- Explore the source code in `src/Module3/Labs.Cli/`
- Try building your own CLI tool with MSAL.NET
- Experiment with different scopes and Graph API endpoints
- Consider how these patterns apply to desktop and mobile apps

## Additional Resources

- [OAuth 2.0 for Native Apps (RFC 8252)](https://tools.ietf.org/html/rfc8252)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [Device Authorization Grant (RFC 8628)](https://tools.ietf.org/html/rfc8628)
- [MSAL.NET Public Client Applications](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-initializing-client-applications#public-client-application)
- [Microsoft Identity Platform Best Practices](https://docs.microsoft.com/en-us/azure/active-directory/develop/identity-platform-integration-checklist)

---

**Congratulations!** You've completed Lab 5 and now understand how to build secure public client applications with Microsoft Entra ID.
