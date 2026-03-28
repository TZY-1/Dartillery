using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dartillery.Web.Pages;

/// <summary>
/// Error page model for displaying unhandled exception details.
/// </summary>
[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public partial class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    /// <summary>Initializes the error model with a logger instance.</summary>
    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    /// <summary>The unique request identifier for correlation.</summary>
    public string? RequestId { get; set; }

    /// <summary>Whether the request ID should be displayed.</summary>
    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    /// <summary>Handles GET requests to the error page.</summary>
    public void OnGet()
    {
        LogErrorPageRequested(_logger);
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Error page requested")]
    private static partial void LogErrorPageRequested(ILogger logger);
}
