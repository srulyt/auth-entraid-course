namespace Labs.Shared;

/// <summary>
/// Constants used across the Module 2 labs
/// </summary>
public static class Constants
{
    /// <summary>
    /// Scope names for the custom API
    /// </summary>
    public static class Scopes
    {
        /// <summary>
        /// Custom API scope for reading data
        /// Format: api://{clientId}/api.read
        /// </summary>
        public const string ApiRead = "api.read";
        
        /// <summary>
        /// Microsoft Graph User.Read scope
        /// </summary>
        public const string UserRead = "User.Read";
        
        /// <summary>
        /// Default scope for client credentials (.default)
        /// </summary>
        public const string Default = ".default";
    }
    
    /// <summary>
    /// Common claim types used in tokens
    /// </summary>
    public static class ClaimTypes
    {
        // v2.0 token claim types (short form)
        public const string Scope = "scp";
        public const string Roles = "roles";
        public const string ObjectId = "oid";
        public const string TenantId = "tid";
        public const string PreferredUserName = "preferred_username";
        public const string Name = "name";
        public const string Audience = "aud";
        public const string Issuer = "iss";
        
        // v1.0 token claim types (full URIs)
        public const string ScopeV1 = "http://schemas.microsoft.com/identity/claims/scope";
        public const string ObjectIdV1 = "http://schemas.microsoft.com/identity/claims/objectidentifier";
        public const string TenantIdV1 = "http://schemas.microsoft.com/identity/claims/tenantid";
    }
    
    /// <summary>
    /// Microsoft Graph API endpoints
    /// </summary>
    public static class GraphEndpoints
    {
        public const string BaseUrl = "https://graph.microsoft.com/v1.0";
        public const string Me = "/me";
        public const string Users = "/users";
        public const string Organization = "/organization";
    }
    
    /// <summary>
    /// Authorization policy names
    /// </summary>
    public static class Policies
    {
        public const string RequireApiReadScope = "RequireApiReadScope";
    }
}
