using System.Diagnostics.CodeAnalysis;

namespace Budgetty.Mvc;

[ExcludeFromCodeCoverage]
public class GoogleOAuthConfig
{
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
}