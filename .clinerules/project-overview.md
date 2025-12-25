## Brief overview

This Cline rule file provides project context for an **ASP.NET Core 8.0 training lab application** designed to teach Microsoft Entra ID authentication and authorization concepts. The project is organized into two modules with four hands-on labs where students learn by configuration only—no coding required during labs. All code is prebuilt with a focus on token exploration, authorization patterns, and multi-tier authentication flows.

## Project structure

The repository is organized into three modules:

**Module 1: Authentication & Authorization Basics** (`src/Module1/`)
- `WebAuthzDemo`: Main web application (Razor Pages) with authentication, token viewers, and protected API
- `TokenInspector`: Class library for JWT decode/format helpers (shared across all modules)

**Module 2: Protected Web APIs & Cross-Tenant** (`src/Module2/`)
- `Labs.Shared`: Common models, constants, and configuration classes
- `Labs.MiddleTierApi`: Protected Web API with OBO flow to Microsoft Graph
- `Labs.ClientWeb`: Razor Pages client calling the protected API
- `Labs.CrossTenantDaemon`: Console daemon app for cross-tenant scenarios

**Module 3: Public Client Authentication** (`src/Module3/`)
- `Labs.Cli`: Modern CLI tool demonstrating public client flows (PKCE and Device Code)

**Documentation structure**:
- `docs/README.md`: Main course overview
- `docs/Module1/README.md`: Module 1 overview
- `docs/Module1/Lab1_Authentication.md`: Authentication lab (10-15 min)
- `docs/Module1/Lab2_SimpleAuthorization.md`: Authorization lab (10-15 min)
- `docs/Module2/README.md`: Module 2 overview
- `docs/Module2/Lab3_ProtectedWebAPI.md`: Protected API + OBO lab (12-15 min)
- `docs/Module2/Lab4_CrossTenantDaemon.md`: Cross-tenant daemon lab (12-15 min)
- `docs/Module3/README.md`: Module 3 overview
- `docs/Module3/Lab5_PublicClientCLI.md`: Public client CLI authentication lab (45-60 min)

**Scripts**: `scripts/setup.ps1` for generating configuration templates

## Tech stack requirements

- **.NET 8** and **ASP.NET Core**
- **Microsoft.Identity.Web** and **Microsoft.Identity.Web.UI** packages
- **Authorization Code + PKCE** flow for interactive login
- **Microsoft Graph SDK** (optional) for delegated API calls
- **Razor Pages** for UI (lightweight, instructor-friendly)
- **Minimal APIs** or standard controllers for protected endpoints

## Lab objectives

**Module 1: Foundations**

**Lab 1 (Authentication):**
- Sign in with Microsoft Entra ID
- Explore ID token vs Access token differences
- View claims in tabular format
- Understand token lifetimes and renewal concepts
- Simulate common errors (redirect URI mismatch, missing consent)

**Lab 2 (Authorization - Simplified):**
- Understand the difference between authentication and authorization
- Test authentication-only authorization (`[Authorize]` attribute)
- Test local application-managed RBAC (self-assignment of Admin role)
- Call Microsoft Graph API with delegated permissions (`User.Read` scope)
- Understand that Entra ID provides identity while your app manages permissions

**Module 2: Advanced Scenarios**

**Lab 3 (Protected Web API + OBO Flow):**
- Expose and protect custom API with custom scopes (`api.read`)
- Understand token audience validation for APIs
- Implement scope-based authorization in ASP.NET Core
- Use On-Behalf-Of (OBO) flow to call Microsoft Graph from the API
- Explore three-tier authentication (Client → API → Graph)
- Compare delegated permissions vs custom API scopes

**Lab 4 (Cross-Tenant Daemon):**
- Implement app-only (daemon) authentication with client credentials flow
- Understand application permissions vs delegated permissions
- Configure multi-tenant applications
- Acquire tokens for multiple Entra ID tenants
- Call Microsoft Graph without user context
- Explore cross-tenant consent and security considerations

**Module 3: Public Client Authentication**

**Lab 5 (Public Client CLI):**
- Understand why client secrets cannot be used in CLI/desktop/mobile apps
- Configure public client app registration in Entra ID
- Use Authorization Code + PKCE flow for interactive authentication
- Use Device Code flow for limited-input scenarios
- Explore token caching and security trade-offs for local storage
- Call Microsoft Graph API from a CLI tool
- Implement security best practices for public clients

## Key constraints

- **No coding by students**: All code must be prebuilt and functional
- **Configuration only**: Students only update `appsettings.json` or environment variables with Client ID, Tenant ID, etc.
- **Clear documentation**: Copy/paste ready instructions for app registration and API permissions
- **Error simulation**: Include UI toggles to demonstrate common mistakes for teaching moments
- **Security**: Never display refresh tokens; only explain them conceptually

## Authentication patterns

- Use **Microsoft.Identity.Web** patterns consistently throughout the application
- Implement **Authorization Code + PKCE** (not implicit flow)
- Configure via `appsettings.json`:
  - `AzureAd:Instance`, `AzureAd:Domain`, `AzureAd:TenantId`, `AzureAd:ClientId`, `AzureAd:CallbackPath`
- Maintain **HTTPS redirect URI consistency** across app configuration, README, and lab docs
- Use `launchSettings.json` with consistent HTTPS port

## Authorization patterns

**Module 1 patterns:**
- Define **named policies** for authorization requirements:
  - `RequireLocalAdmin`: Checks local role store for "Admin" role
- Apply `[Authorize]` attribute with or without policy names:
  - `[Authorize]` - Authentication-only (baseline)
  - `[Authorize(Policy = "RequireLocalAdmin")]` - Application-managed RBAC
- Use **in-memory role store** (`LocalRoleService`) for demonstration:
  - Maps user `oid` claim to application roles
  - Thread-safe using `ConcurrentDictionary`
  - Would use database in production
- Provide **role management endpoints**:
  - `POST /api/roles/assign-admin` - Self-assign Admin role
  - `POST /api/roles/remove-admin` - Remove Admin role

**Module 2 patterns:**
- **Custom API scopes**: Define and expose API-specific scopes (e.g., `api.read`)
- **Scope-based authorization**: Validate scopes in access tokens with named policies
  - `RequireApiReadScope`: Checks for `api.read` scope claim
- **On-Behalf-Of (OBO) flow**: API exchanges user's access token for Graph token
- **App-only authentication**: Client credentials flow for daemon apps
- **Application permissions**: Tenant-wide permissions requiring admin consent
- **Multi-tenant support**: Cross-tenant token acquisition and consent
- Provide clear guidance when authorization fails with actionable error messages

## UI/UX priorities

- **Instructor-friendly**: Clean, simple UI focused on learning concepts
- **Token exploration**: Dedicated pages for ID Token, Access Token, and Claims
- **Pretty-print tokens**: Display header/payload/signature with key claims highlighted
- **Help sidebars**: Include conceptual notes (Authentication vs Authorization, ID token vs Access token, Scopes vs Roles)
- **External tools**: Provide "Copy token" button and "Open in jwt.ms" link
- **Error visibility**: Surface clear error messages for teaching moments

## Development workflow

- **Build without manual edits**: App must run immediately after configuration
- **Services encapsulation**: Use `TokenService` for token acquisition/decode, `GraphService` for Graph API calls
- **JWT helpers**: Centralize token parsing in `TokenInspector/JwtTools.cs`
- **Error handling**: Gracefully handle and display authorization failures with actionable guidance

## Documentation standards

- **Step-by-step instructions**: Numbered steps with exact portal actions
- **Copy/paste ready**: Provide exact JSON for app roles, exact URIs for redirect configuration
- **Troubleshooting section**: Cover common errors (AADSTS50011 redirect mismatch, missing consent, audience mismatch)
- **Screenshot placeholders**: Include markdown image links with descriptive captions
- **Teaching flow**: Guide instructors through logical progression (Sign In → ID Token → Access Token → Protected API)

## Security considerations

- **Refresh tokens**: Never display in UI; only explain concept and lifetime
- **PII logging**: Set `EnablePiiLogging=false` in production
- **Token redaction**: Safe redaction of signature section when displaying tokens
- **HTTPS only**: Enforce HTTPS for all redirect URIs and local development

## Code organization principles

- Keep `Program.cs` clean with clear authentication and authorization setup
- Encapsulate token operations in `TokenService`
- Encapsulate Graph API calls in `GraphService`
- Encapsulate local role management in `LocalRoleService`
- Separate authorization policies in `Authorization/Policies.cs`
- Use consistent naming: policy name `RequireLocalAdmin`
- Group related pages: `Pages/Tokens/` for token viewers
- Register `LocalRoleService` as singleton (static in-memory store)

## Quality standards

- All applications build and run without manual code edits after configuration
- Clear, actionable error messages for authorization failures
- **Module 1**: Three distinct authorization examples with clear learning goals:
  1. Authentication-only (baseline)
  2. Local application-managed roles (interactive self-assignment)
  3. Delegated permissions to Microsoft Graph
- **Module 2**: Advanced scenarios demonstrating real-world patterns:
  1. Custom API protection with scopes
  2. On-Behalf-Of (OBO) flow for API chaining
  3. App-only authentication for background services
  4. Cross-tenant authentication and consent
- UI provides educational value for token exploration and authorization concepts
- Interactive demonstrations show separation of identity (Entra ID) and permissions (app/API)
- Documentation enables self-service setup by instructors
- Module 1 requires minimal Azure configuration (User.Read is default)
- Module 2 demonstrates enterprise-grade authentication patterns
- All solutions use consistent architecture and coding patterns

## Updates to this document

- This project-overview.md file should be kept up to date as the poject evolves
- Any large change to the project must be reflected in this document
- Prefer to keep the current document structure and update the statements that need to be changed
