using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Dartillery.Web.Pages;

[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
[IgnoreAntiforgeryToken]
public partial class ErrorModel : PageModel
{
    private readonly ILogger<ErrorModel> _logger;

    public ErrorModel(ILogger<ErrorModel> logger)
    {
        _logger = logger;
    }

    public string? RequestId { get; set; }

    public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);

    public void OnGet()
    {
        LogErrorPageRequested(_logger);
        RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier;
    }

    [LoggerMessage(Level = LogLevel.Debug, Message = "Error page requested")]
    private static partial void LogErrorPageRequested(ILogger logger);
}
