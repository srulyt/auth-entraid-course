# Module 3: Public Client Authentication for CLI Tools

This module contains a hands-on lab that demonstrates Microsoft Entra ID authentication for command-line applications using public client flows.

## Overview

**Duration**: 45-60 minutes

**Prerequisites**:
- Completion of Module 1 (Authentication basics)
- Understanding of OAuth 2.0 flows and tokens
- Familiarity with command-line interfaces

## Learning Objectives

By completing this module, you will understand:
- Why client secrets cannot be used in public clients (CLI, desktop, mobile apps)
- How Authorization Code + PKCE flow protects public clients
- How Device Code flow works for limited-input scenarios
- Token caching and security considerations for local storage
- Best practices for handling tokens in CLI applications
- The principle of least privilege in scope requests

## Lab Structure

### Lab 5: Public Client CLI Authentication (45-60 minutes)
**Scenario**: Build and configure a command-line tool that authenticates users with Microsoft Entra ID using modern public client flows.

[Start Lab 5 →](Lab5_PublicClientCLI.md)

### Lab 6: External Users and Identity Types (20-25 minutes)
**Scenario**: Compare how Microsoft Entra ID handles different user types (internal members vs external guests) and explore how identity claims differ based on the authentication source.

[Start Lab 6 →](Lab6_ExternalUsers.md)

**Lab 5 Key Concepts**:
- Public vs confidential clients
- Authorization Code + PKCE (Proof Key for Code Exchange)
- Device Code flow for headless/limited-input scenarios
- Local token caching with security warnings
- Delegated permissions without client secrets
- Interactive vs non-interactive authentication

**Architecture**:
```
CLI Tool (Public Client)
    ↓ PKCE Flow: Opens browser for user auth
Microsoft Entra ID
    ↓ Returns access token (no client secret needed)
CLI Tool
    ↓ Calls Microsoft Graph with user's token
Microsoft Graph
```

**Alternative Flow**:
```
CLI Tool (Public Client)
    ↓ Device Code Flow: Displays code + URL
User's Browser (any device)
    ↓ User enters code and authenticates
Microsoft Entra ID
    ↓ Returns access token to CLI
CLI Tool
    ↓ Calls Microsoft Graph with user's token
Microsoft Graph
```

**Lab 6 Key Concepts**:
- Member (internal) vs Guest (external) users
- Identity provider federation
- User Principal Name (UPN) and identity claims
- Token claims comparison across user types
- Multi-factor authentication setup
- B2B collaboration fundamentals

## Project Structure

### Labs.Cli
Command-line application featuring:
- Modern CLI design using System.CommandLine
- Two authentication flows: PKCE and Device Code
- Token inspection and management commands
- Microsoft Graph integration
- Configuration management
- Diagnostic tools

**Commands**:
- `login --mode pkce|device-code` - Sign in with different flows
- `logout` - Sign out and clear tokens
- `account show` - Display current user
- `token show|clear` - Manage tokens
- `graph me|scopes` - Call Microsoft Graph
- `config show|set` - Manage configuration
- `diagnose` - Check setup and connectivity

## Getting Started

### System Requirements
- .NET 8.0 SDK or later
- Visual Studio 2022, VS Code, or any text editor
- Microsoft Entra ID tenant (or free trial)
- Access to create app registrations
- Command-line terminal (PowerShell, bash, cmd)

### Quick Start

1. **Navigate to Module 3**:
   ```bash
   cd src/Module3
   ```

2. **Restore packages**:
   ```bash
   dotnet restore
   ```

3. **Start the labs**:
   - [Lab 5: Public Client CLI Authentication](Lab5_PublicClientCLI.md)
   - [Lab 6: External Users and Identity Types](Lab6_ExternalUsers.md)

## Key Differences from Modules 1 & 2

| Aspect | Module 1 & 2 | Module 3 |
|--------|--------------|----------|
| **Client Type** | Confidential (web apps with secrets) | Public (no secrets) |
| **Auth Flow** | Authorization Code (with secret) | Auth Code + PKCE (no secret) |
| **Alternative Flow** | N/A | Device Code (for limited input) |
| **Secret Storage** | Server-side, secure | No secrets (can't be protected) |
| **Token Cache** | Server memory | Local file (security trade-off) |
| **Use Cases** | Web applications, APIs | CLI tools, desktop apps, mobile apps |
| **Trust Model** | Server protects secret | PKCE cryptographically binds request |

## Security Concepts Explained

### Why No Client Secrets in Public Clients?

**The Problem**:
```
❌ BAD IDEA: CLI tool with embedded secret
const string SECRET = "abc123..."; // Anyone can extract this!

Tools that can extract it:
- Decompiler (dotPeek, ILSpy)
- Process memory dump
- Network traffic inspection
- Simply reading the binary
```

**The Solution**:
```
✅ GOOD: Public client with PKCE
1. Generate random code_verifier (client-side)
2. Hash it to create code_challenge
3. Send code_challenge to Entra ID
4. Get authorization code
5. Exchange code + original code_verifier for token
6. Entra ID verifies: hash(code_verifier) == code_challenge

Result: Even if attacker intercepts the code, they can't
exchange it without the original code_verifier.
```

### PKCE (Proof Key for Code Exchange)

**How it works**:
1. **Client generates**: Random 43-128 character string (code_verifier)
2. **Client computes**: SHA256(code_verifier) = code_challenge
3. **Authorization request**: Includes code_challenge
4. **Entra ID stores**: code_challenge with authorization code
5. **Token request**: Includes code_verifier
6. **Entra ID verifies**: SHA256(code_verifier) == stored code_challenge
7. **If valid**: Returns access token

**Benefits**:
- No client secret needed
- Protects against authorization code interception
- Works even on untrusted devices
- Required for public clients in modern OAuth

### Device Code Flow

**When to use**:
- Input-limited devices (smart TVs, IoT devices)
- Headless/remote systems
- Command-line tools where browser redirect is problematic
- Scenarios where user authentication happens on a different device

**How it works**:
```
CLI Tool                    Entra ID                User's Browser
    |                           |                           |
    |-- Request device code --> |                           |
    |<-- Code + URL ------------|                           |
    |                           |                           |
Display code to user            |                           |
    |                           |                           |
    |                           |<-- User visits URL -------|
    |                           |<-- User enters code ------|
    |                           |-- User authenticates ---->|
    |                           |<-- User consents ---------|
    |                           |                           |
    |-- Poll for token -------->|                           |
    |<-- Access token ----------|                           |
```

### Token Caching Security

**Local token cache considerations**:

✅ **Benefits**:
- User doesn't need to sign in repeatedly
- Better user experience
- Tokens refresh automatically

⚠️ **Risks**:
- Tokens stored on local file system
- Anyone with file access can read tokens
- Shared computers are especially risky

**Best practices**:
- Use OS-provided secure storage when available (Keychain on macOS, DPAPI on Windows)
- Clear cache on shared/public computers
- Implement token lifetime limits
- Educate users about the risks

## Common Configuration Patterns

**appsettings.json structure**:
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

**Key differences from confidential clients**:
- ❌ No `ClientSecret`
- ✅ `RedirectUri` = `http://localhost` (native redirect)
- ✅ Minimal scopes (principle of least privilege)

## Troubleshooting

### Common Issues

**"unauthorized_client" error**:
- **Cause**: Public client flows not enabled in app registration
- **Solution**: Enable "Allow public client flows" in Azure Portal
- **Location**: App registration → Authentication → Advanced settings

**Device Code flow not working**:
- **Cause**: Organization policy may block device code flow
- **Solution**: Use PKCE flow instead, or contact admin to allow device code

**Browser doesn't open for PKCE**:
- **Cause**: No default browser set, or running in SSH session
- **Solution**: Use Device Code flow instead, or set default browser

**Tokens not cached**:
- **Cause**: Insufficient permissions to create cache file
- **Solution**: Check user has write access to `~/.entra-lab/` directory

**"invalid_grant" error**:
- **Cause**: Token cache corrupted or password changed
- **Solution**: Run `entra-lab token clear` and sign in again

## Additional Resources

- [Microsoft Identity Platform - Public client apps](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-client-applications)
- [OAuth 2.0 for Native Apps (RFC 8252)](https://tools.ietf.org/html/rfc8252)
- [PKCE RFC 7636](https://tools.ietf.org/html/rfc7636)
- [Device Authorization Grant (RFC 8628)](https://tools.ietf.org/html/rfc8628)
- [MSAL.NET documentation](https://docs.microsoft.com/en-us/azure/active-directory/develop/msal-net-initializing-client-applications)

## What You'll Learn

After completing this module, you'll understand:

✅ The fundamental difference between public and confidential clients  
✅ Why client secrets can't be protected in CLI/desktop/mobile apps  
✅ How PKCE cryptographically secures the authorization code flow  
✅ When and how to use Device Code flow  
✅ Token caching strategies and security trade-offs  
✅ Best practices for building secure public client applications  
✅ How to implement least-privilege scope requests  

## Next Steps

After completing this module, you'll have hands-on experience with:
- Building modern CLI tools with Microsoft Entra ID authentication
- Implementing both PKCE and Device Code flows
- Managing tokens securely in public clients
- Understanding the security boundaries of different client types

These patterns are essential for building:
- Command-line developer tools
- Desktop applications
- Mobile applications
- IoT and smart device apps

---

**Ready to start?** 
- [Lab 5: Public Client CLI Authentication](Lab5_PublicClientCLI.md) → 
- [Lab 6: External Users and Identity Types](Lab6_ExternalUsers.md) →
