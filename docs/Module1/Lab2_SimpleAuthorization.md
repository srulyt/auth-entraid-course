# Lab 2: Authorization with Microsoft Entra ID

**Duration:** 10-15 minutes  
**Difficulty:** Beginner to Intermediate

## 🎯 Learning Objectives

By the end of this lab, you will understand:
1. The difference between **authentication** and **authorization**
2. How your app can maintain its own **local role assignments**
3. How to call **downstream APIs** (Microsoft Graph) with delegated permissions

## 📋 Prerequisites

- **Completed Lab 1: Authentication**
- Signed in to the application
- Basic understanding of tokens and claims from Lab 1

## 🔑 Key Concepts

### Authentication vs Authorization

| Concept | Question | Purpose |
|---------|----------|---------|
| **Authentication (AuthN)** | *"Who are you?"* | Proves your identity |
| **Authorization (AuthZ)** | *"What can you do?"* | Grants permissions |

**Critical Insight:** Being authenticated doesn't mean you can do everything. Authorization is a separate check that happens AFTER authentication.

### Identity vs Permissions

- **Entra ID provides:** Your identity (who you are) via claims like `oid`, `name`, `email`
- **Your Application decides:** What you can do (permissions, roles, access levels)

This separation allows your application to maintain its own permission model!

---

## 🚀 Lab Exercises

### Exercise 1: Authentication-Only Authorization (3 minutes)

**Learning Goal:** Understand that authentication is the baseline - it proves who you are, but doesn't grant specific permissions.

#### Steps:

1. **Navigate to the Authorization Lab:**
   - In the application, click **Protected API** in the navigation menu
   - You should see "Lab 2: Authorization with Microsoft Entra ID"

2. **Check Your Status:**
   - Under "Your Authorization Status", verify you see:
     - ✓ **Authenticated** - Yes
   - This confirms you completed Lab 1 and are signed in

3. **Test Example 1:**
   - Scroll to **Example 1: Authentication-Only Authorization**
   - Read the teaching point about the `[Authorize]` attribute
   - Click **Test Authentication-Only Endpoint**
   - You should see a success message

4. **Observe the Response:**
   ```json
   {
     "success": true,
     "message": "You are authenticated! This endpoint only requires you to be signed in.",
     "user": {
       "name": "Your Name",
       "id": "...",
       "authenticationType": "AuthenticationTypes.Federation"
     },
     "teachingPoint": "Authentication proves WHO you are. Any authenticated user can access this endpoint."
   }
   ```

#### 💡 Key Takeaway

The endpoint uses `[Authorize]` with no additional policy. This means:
- ✅ Any signed-in user can access it
- ❌ Anonymous users are rejected
- This is the **baseline** for authorization

---

### Exercise 2: Local Admin RBAC (Self-Assignment) (5 minutes)

**Learning Goal:** Understand that your application can maintain its own role assignments, completely separate from Entra ID.

#### Background: Application-Managed Roles

Your application can implement its own permission system:
- **Entra ID:** Provides identity (who you are) via the `oid` claim
- **Your App:** Maintains its own role store (what you can do)
- **Real-world example:** E-commerce sites manage their own "Premium Member" or "Seller" roles

This lab uses an in-memory role store for demonstration. In production, roles would be stored in a database.

#### Steps:

1. **Check Your Initial Status:**
   - Under "Your Authorization Status", check:
     - **Local Admin Role:** You should see "✗ No - Assign yourself the role to access Example 2"
   - Note your **User ID** - this is your unique identifier from the `oid` claim

2. **Test Without Admin Role (Expect Failure):**
   - Scroll to **Example 2: Local Admin RBAC (Self-Assignment)**
   - Your status should show: "✗ You do NOT have the Admin role"
   - Click **Test Admin-Only Endpoint**
   - You should see a **403 Forbidden** message:
   ```
   ⚠ Authorization Failed (403 Forbidden)
   
   You are authenticated (we know who you are), but you are not authorized 
   (you lack the required permission).
   
   Why? The endpoint requires the "Admin" role in our application's local role store.
   
   Solution: Click the "Assign Me Admin Role" button above, then try again!
   ```

3. **Assign Yourself the Admin Role:**
   - Click **➕ Assign Me Admin Role** button
   - You should see a success message:
   ```
   ✓ Success! Admin role assigned successfully!
   You now have the Admin role. Try testing the Admin-Only endpoint!
   ```
   - Notice the button is now disabled and the "Remove My Admin Role" button is enabled

4. **Test With Admin Role (Expect Success):**
   - Click **Test Admin-Only Endpoint** again
   - You should now see a success message:
   ```json
   {
     "success": true,
     "message": "You have the Admin role in our application's local role store!",
     "user": {
       "name": "Your Name",
       "userId": "...",
       "localRoles": ["Admin"]
     },
     "teachingPoint": "Your application maintains its own role assignments. Entra ID provides WHO you are (identity), but YOUR app decides WHAT you can do (permissions)."
   }
   ```

5. **Remove the Admin Role (Optional):**
   - Click **➖ Remove My Admin Role** button
   - You should see a success message
   - Try testing the endpoint again - it should fail with 403 Forbidden

#### 💡 Key Takeaway

This demonstrates **separation of identity and permissions**:
- **Entra ID:** Knows nothing about your app's "Admin" role
- **Your Application:** Makes authorization decisions based on its own role store
- **The oid claim:** Links the user's identity from Entra ID to roles in your app

This is how most real applications work - they use Entra ID for identity but maintain their own permission models.

---

### Exercise 3: Call Microsoft Graph API (5 minutes)

**Learning Goal:** Understand how to call downstream APIs that require authentication on behalf of the signed-in user.

#### Background: Delegated Permissions

When your app calls Microsoft Graph **on behalf of the user**, it needs delegated permissions:
- **User.Read** - Read the signed-in user's profile
- This permission is **added by default** to new app registrations
- Microsoft.Identity.Web handles token acquisition automatically

#### Steps:

1. **Verify Permission Status:**
   - Under "Your Authorization Status", verify:
     - ✓ **User.Read Permission** - Configured by default

2. **Test Example 3:**
   - Scroll to **Example 3: Call Microsoft Graph API**
   - Read the teaching point about downstream APIs
   - Click **Test Microsoft Graph Call**
   - You should see a success message with your profile data

3. **Observe the Response:**
   ```json
   {
     "success": true,
     "message": "Successfully called Microsoft Graph API on your behalf using the User.Read permission.",
     "profile": {
       "displayName": "Your Name",
       "email": "your.email@domain.com",
       "id": "...",
       "jobTitle": "..."
     },
     "teachingPoint": "Your app acquired an access token to call Microsoft Graph on your behalf..."
   }
   ```

4. **Understanding the Token Flow:**
   - Your app has an **ID token** (proves who you are)
   - To call Graph, it needs an **access token** for Microsoft Graph
   - Microsoft.Identity.Web automatically:
     1. Requests an access token for Graph
     2. Includes the `User.Read` scope
     3. Caches the token for future calls
     4. Refreshes it when it expires

5. **Compare ID Token vs Access Token (Optional):**
   - Navigate to **ID Token** page - this token is for YOUR app
   - Navigate to **Access Token** page - this token is for Microsoft Graph
   - Notice the `aud` (audience) claim is different:
     - ID token: `aud` = your app's Client ID
     - Access token: `aud` = `00000003-0000-0000-c000-000000000000` (Microsoft Graph)

#### 💡 Key Takeaway

Calling downstream APIs involves:
1. **Delegated permissions** - User consents to your app acting on their behalf
2. **Token acquisition** - Getting the right token for the right API
3. **On-behalf-of flow** - The app uses the user's identity, not its own

This is different from **app-only permissions** where the app has its own identity (covered in future modules).

---

## 🔍 Understanding the Code

### How the Three Examples Differ

| Example | Attribute | Policy | Checks |
|---------|-----------|--------|--------|
| 1 | `[Authorize]` | None | Just authentication |
| 2 | `[Authorize(Policy = "RequireLocalAdmin")]` | Custom | Local role store for "Admin" role |
| 3 | `[Authorize]` | None | Authentication + token for Graph |

### Authorization Policy Implementation

From `Authorization/Policies.cs`:

```csharp
public static void ConfigurePolicies(AuthorizationOptions options)
{
    options.AddPolicy(RequireLocalAdmin, policy =>
    {
        policy.RequireAssertion(context =>
        {
            // Get the local role service from DI
            var roleService = httpContext.RequestServices.GetService<ILocalRoleService>();
            
            // Get user's object ID
            var userId = context.User.FindFirst("oid")?.Value;
            
            // Check if user has Admin role in our local store
            return roleService.HasRole(userId, "Admin");
        });
    });
}
```

**How it works:**
1. Policy is registered in `Program.cs` during app startup
2. The `[Authorize(Policy = "RequireLocalAdmin")]` attribute triggers the policy
3. ASP.NET Core evaluates the `RequireAssertion` lambda
4. The policy checks the local role store (not Entra ID)
5. If user has the role, access is granted; otherwise, 403 Forbidden

### Local Role Service

From `Services/LocalRoleService.cs`:

```csharp
public class LocalRoleService : ILocalRoleService
{
    // In-memory dictionary: UserId -> List of Roles
    private static readonly ConcurrentDictionary<string, HashSet<string>> _userRoles = new();

    public bool HasRole(string userId, string roleName)
    {
        return _userRoles.TryGetValue(userId, out var roles) && 
               roles.Contains(roleName, StringComparer.OrdinalIgnoreCase);
    }

    public void AssignRole(string userId, string roleName)
    {
        // Add role to user's role set
    }
}
```

**Key points:**
- Uses `oid` claim as the user identifier
- Stores roles in memory (use a database in production)
- Thread-safe using `ConcurrentDictionary`

### API Controller Endpoints

From `Controllers/ApiController.cs`:

```csharp
// Example 1: Just requires authentication
[HttpGet("authenticated")]
[Authorize]
public IActionResult GetAuthenticatedData() { ... }

// Example 2: Requires local Admin role
[HttpGet("admin-only")]
[Authorize(Policy = AuthorizationPolicies.RequireLocalAdmin)]
public IActionResult GetAdminData() { ... }

// Example 3: Calls Microsoft Graph
[HttpGet("my-profile")]
[Authorize]
public async Task<IActionResult> GetMyProfile()
{
    var profile = await _graphService.GetMyProfileAsync();
    return Ok(profile);
}

// Role management endpoints
[HttpPost("roles/assign-admin")]
[Authorize]
public IActionResult AssignAdminRole([FromServices] ILocalRoleService roleService)
{
    var userId = User.FindFirst("oid")?.Value;
    roleService.AssignRole(userId, "Admin");
    return Ok(...);
}
```

---

## ✅ Lab Completion Checklist

- [ ] Tested Example 1 - Authentication-only endpoint (should succeed)
- [ ] Tested Example 2 without Admin role (should fail with 403)
- [ ] Assigned yourself the Admin role
- [ ] Tested Example 2 with Admin role (should succeed)
- [ ] Tested Example 3 - Microsoft Graph call (should succeed)
- [ ] Understood the difference between AuthN and AuthZ
- [ ] Understood that your app can maintain its own role store
- [ ] Understood how to call downstream APIs with delegated permissions

---

## 🎯 Key Takeaways

### 1. Authentication ≠ Authorization

- **Authentication** happens first - it proves your identity
- **Authorization** happens second - it checks your permissions
- You can be authenticated but NOT authorized (403 Forbidden response)

### 2. Identity Provider vs Application Permissions

- **Entra ID:** Provides identity and authentication (WHO you are)
- **Your Application:** Manages authorization and permissions (WHAT you can do)
- This separation gives you flexibility in how you manage permissions

### 3. Local Role Management is Common

Most applications maintain their own permission models:
- E-commerce: "Customer", "Seller", "Premium Member"
- SaaS platforms: "Viewer", "Editor", "Owner"
- Internal tools: "Employee", "Manager", "Administrator"

They use Entra ID for identity but manage roles themselves.

### 4. Three Authorization Patterns

| Pattern | Use Case | Example |
|---------|----------|---------|
| **Authentication-only** | Basic access control | Internal apps where all users have equal access |
| **Application roles** | Custom permissions | Local role store, database-driven permissions |
| **Delegated permissions** | Calling APIs on behalf of user | Reading user's email, calendar, or profile |

---

## 🧪 Try This Next

Now that you understand local role management:

### Add More Roles

Try extending the role system to support multiple roles:

```csharp
// In the controller
roleService.AssignRole(userId, "Editor");
roleService.AssignRole(userId, "Viewer");

// In the policy
policy.RequireAssertion(context =>
{
    var userId = context.User.FindFirst("oid")?.Value;
    return roleService.HasRole(userId, "Admin") || 
           roleService.HasRole(userId, "Editor");
});
```

### Persist Roles to a Database

In a real application, replace the in-memory dictionary with a database:

```csharp
public class UserRole
{
    public int Id { get; set; }
    public string UserId { get; set; }  // oid from Entra ID
    public string RoleName { get; set; }
}
```

---

## 📚 Additional Resources

- [Authorization in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/authorization/introduction)
- [Claims-based Authorization](https://learn.microsoft.com/aspnet/core/security/authorization/claims)
- [Microsoft Graph Permissions Reference](https://learn.microsoft.com/graph/permissions-reference)
- [Delegated vs Application Permissions](https://learn.microsoft.com/azure/active-directory/develop/v2-permissions-and-consent)

---

## ❓ Quiz Yourself

1. What's the difference between authentication and authorization?
2. What claim from Entra ID identifies the user uniquely?
3. Why doesn't Entra ID know about your app's "Admin" role?
4. What happens when you try to access a protected endpoint without the required role?
5. Why does the app need a different token to call Microsoft Graph?
6. What permission is required to read the signed-in user's profile from Graph?

<details>
<summary>Click for Answers</summary>

1. Authentication proves WHO you are (identity); Authorization determines WHAT you can do (permissions)
2. The `oid` claim (object ID) - unique for each user in the tenant
3. Because your app maintains its own role store. Entra ID only provides identity; your app manages permissions.
4. 403 Forbidden (authenticated but not authorized)
5. Each API requires its own access token with the correct audience. The ID token is for your app; the access token is for Microsoft Graph.
6. `User.Read` (added by default to new app registrations)

</details>

---

## 🎉 Congratulations!

You've completed Lab 2 and now understand:
- ✅ The difference between authentication and authorization
- ✅ How your application can maintain its own role assignments
- ✅ How to call downstream APIs with delegated permissions
- ✅ That Entra ID provides identity, but YOUR app decides permissions

**Next Module Preview:**
In the next module, you'll dive deep into:
- App registrations and enterprise applications
- API permissions and admin consent workflows
- Application vs delegated permissions
- Custom scopes and roles in app manifests

---

**Lab 2 Complete!** ✅

← Back to [Lab 1: Authentication](Lab1_Authentication.md) | Return to [Main README](README.md)
