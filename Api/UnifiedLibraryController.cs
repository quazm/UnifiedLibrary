using System;
using System.IO;
using System.Linq;
using System.Security.Claims;
using Jellyfin.Plugin.UnifiedLibrary.Services;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Querying;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Jellyfin.Plugin.UnifiedLibrary.Api;

[ApiController]
[Authorize]
[Route("Plugin/UnifiedLibrary")]
public class UnifiedLibraryController : ControllerBase
{
    private readonly UnifiedLibraryQueryService _queryService;
    private readonly IUserManager _userManager;
    private readonly ILibraryManager _libraryManager;

    public UnifiedLibraryController(
        UnifiedLibraryQueryService queryService,
        IUserManager userManager,
        ILibraryManager libraryManager)
    {
        _queryService = queryService;
        _userManager = userManager;
        _libraryManager = libraryManager;
    }

    [HttpGet("Items")]
    public ActionResult<QueryResult<BaseItemDto>> GetItems(
        [FromQuery] UnifiedItemsRequest request,
        [FromQuery] Guid? userId = null)
    {
        var resolvedUserId = userId ?? GetCurrentUserIdFromClaims();

        if (resolvedUserId == Guid.Empty)
            return Unauthorized("User ID is required.");

        var result = _queryService.GetItems(resolvedUserId, request);
        return Ok(result);
    }

    [HttpGet("Filters")]
    public ActionResult GetFilters([FromQuery] Guid userId)
    {
        var resolvedUserId = userId != Guid.Empty ? userId : GetCurrentUserIdFromClaims();
        if (resolvedUserId == Guid.Empty)
            return Unauthorized();

        var config = Plugin.Instance?.Configuration;
        if (config == null)
            return Ok(new { Years = Array.Empty<int>() });

        var user = _userManager.GetUserById(resolvedUserId);
        if (user == null)
            return Ok(new { Years = Array.Empty<int>() });

        var libraryIds = config.IncludedLibraryIds
            .Where(id => Guid.TryParse(id, out _))
            .Select(id => Guid.Parse(id))
            .ToArray();

        // Убрали DtoOptions, используем настройки по умолчанию
        var query = new InternalItemsQuery(user)
        {
            Recursive = true,
            AncestorIds = libraryIds,
            IncludeItemTypes = config.IncludedItemTypes,
            Limit = int.MaxValue
        };

        var result = _libraryManager.GetItemsResult(query);
        var years = result.Items
            .Select(i => i.ProductionYear)
            .Where(y => y.HasValue)
            .Select(y => y!.Value)
            .Distinct()
            .OrderByDescending(y => y)
            .ToArray();

        return Ok(new { Years = years });
    }

    [HttpGet("Page")]
    [AllowAnonymous]
    public ActionResult GetPage()
    {
        var plugin = Plugin.Instance;
        if (plugin == null)
            return NotFound("Plugin not loaded");

        var pluginPath = plugin.DataFolderPath;
        var htmlPath = Path.Combine(pluginPath, "unifiedPage.external.html");

        if (!System.IO.File.Exists(htmlPath))
            return NotFound("External HTML file not found: " + htmlPath);

        var html = System.IO.File.ReadAllText(htmlPath);
        return Content(html, "text/html");
    }

    private Guid GetCurrentUserIdFromClaims()
    {
        var userIdClaim = User.FindFirst("userid")?.Value
                       ?? User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                       ?? User.FindFirst("sub")?.Value
                       ?? User.FindFirst("Jellyfin-UserId")?.Value;

        if (string.IsNullOrWhiteSpace(userIdClaim))
            return Guid.Empty;

        return Guid.TryParse(userIdClaim, out var userId) ? userId : Guid.Empty;
    }
}