using Jellyfin.Data.Enums;
using Jellyfin.Database.Implementations.Enums;
using Jellyfin.Plugin.UnifiedLibrary.Api;
using Jellyfin.Plugin.UnifiedLibrary.Configuration;
using MediaBrowser.Controller.Dto;
using MediaBrowser.Controller.Entities;
using MediaBrowser.Controller.Library;
using MediaBrowser.Model.Dto;
using MediaBrowser.Model.Entities;
using MediaBrowser.Model.Querying;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;

namespace Jellyfin.Plugin.UnifiedLibrary.Services;

public class UnifiedLibraryQueryService
{
    private readonly ILibraryManager _libraryManager;
    private readonly IUserManager _userManager;
    private readonly IDtoService _dtoService;
    private readonly ILogger<UnifiedLibraryQueryService> _logger;

    public UnifiedLibraryQueryService(
        ILibraryManager libraryManager,
        IUserManager userManager,
        IDtoService dtoService,
        ILogger<UnifiedLibraryQueryService> logger)
    {
        _libraryManager = libraryManager;
        _userManager = userManager;
        _dtoService = dtoService;
        _logger = logger;
    }

    public QueryResult<BaseItemDto> GetItems(Guid userId, UnifiedItemsRequest request)
    {
        var config = Plugin.Instance?.Configuration;
        if (config is null || !config.Enabled || config.IncludedLibraryIds.Length == 0)
            return new QueryResult<BaseItemDto>();

        var user = _userManager.GetUserById(userId);
        if (user is null)
            return new QueryResult<BaseItemDto>();

        var libraryIds = config.IncludedLibraryIds
            .Where(id => Guid.TryParse(id, out _))
            .Select(id => Guid.Parse(id))
            .ToArray();

        if (libraryIds.Length == 0)
            return new QueryResult<BaseItemDto>();

        // Парсим Fields из запроса — только нужные поля для карточек
        var dtoOptions = new DtoOptions();
        if (!string.IsNullOrWhiteSpace(request.Fields))
        {
            var fieldStrings = request.Fields.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            var fields = new ItemFields[fieldStrings.Length];
            for (int i = 0; i < fieldStrings.Length; i++)
            {
                if (Enum.TryParse<ItemFields>(fieldStrings[i], true, out var f))
                    fields[i] = f;
            }
            dtoOptions.Fields = fields.Where(f => f != default).ToArray();
        }
        else
        {
            // Минимальный набор по умолчанию для карточек
            dtoOptions.Fields = new[]
            {
                ItemFields.PrimaryImageAspectRatio,
                ItemFields.SortName,
                ItemFields.DateCreated
            };
        }

        var query = new InternalItemsQuery(user)
        {
            Recursive = true,
            AncestorIds = libraryIds,
            IncludeItemTypes = config.IncludedItemTypes,
            StartIndex = request.StartIndex,
            Limit = request.Limit > 0 ? request.Limit : config.DefaultPageSize,
            EnableTotalRecordCount = true,
            DtoOptions = dtoOptions
        };

        if (!string.IsNullOrWhiteSpace(request.SearchTerm))
            query.SearchTerm = request.SearchTerm;
        if (request.Genres is { Length: > 0 })
            query.Genres = request.Genres;
        if (request.Years is { Length: > 0 })
            query.Years = request.Years;
        if (request.IsFavorite.HasValue)
            query.IsFavorite = request.IsFavorite.Value;

        // ← ДОБАВЛЕНО: фильтры просмотрено/не просмотрено
        if (request.IsFavorite.HasValue)
            query.IsFavorite = request.IsFavorite.Value;

        // Фильтр просмотрено/не просмотрено
        if (request.IsPlayed == true)
        {
            query.IsPlayed = true;       // только просмотренные
        }
        else if (request.IsUnplayed == true)
        {
            query.IsPlayed = false;      // только непросмотренные
        }


        // Сортировка
        var sortOrder = Enum.TryParse<SortOrder>(request.SortOrder, true, out var parsedOrder)
            ? parsedOrder
            : SortOrder.Ascending;

        var sortBy = ItemSortBy.SortName;
        if (!string.IsNullOrWhiteSpace(request.SortBy) &&
            Enum.TryParse<ItemSortBy>(request.SortBy, true, out var parsedSort))
        {
            sortBy = parsedSort;
        }

        query.OrderBy = new[] { (sortBy, sortOrder) };

        var result = _libraryManager.GetItemsResult(query);

        // ВАЖНО: используем dtoOptions из запроса, а не new DtoOptions(true)
        var dtos = _dtoService.GetBaseItemDtos(result.Items, dtoOptions, user);

        return new QueryResult<BaseItemDto>
        {
            Items = dtos,
            TotalRecordCount = result.TotalRecordCount,
            StartIndex = request.StartIndex
        };
    }
}