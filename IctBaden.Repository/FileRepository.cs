using Microsoft.Extensions.Logging;
// ReSharper disable MemberCanBePrivate.Global

namespace IctBaden.Repository;

public class FileRepository
{
    private readonly ILogger _logger;
    private readonly string _repoPath;

    public FileRepository(ILogger logger, string repoPath)
    {
        _logger = logger;
        _repoPath = repoPath;

        if (!Directory.Exists(repoPath))
        {
            Directory.CreateDirectory(repoPath);
        }
    }

    /// <summary>
    /// Create item repository with default name
    /// i.e. item type name 
    /// </summary>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public IItemRepository<TItem> CreateItemRepository<TItem>() where TItem : new()
    {
        var itemType = typeof(TItem);
        return CreateItemRepository<TItem>(itemType.Name);
    }
    
    /// <summary>
    /// Create item repository with given name
    /// </summary>
    /// <param name="name"></param>
    /// <typeparam name="TItem"></typeparam>
    /// <returns></returns>
    public IItemRepository<TItem> CreateItemRepository<TItem>(string name) where TItem : new()
    {
        var itemRepoPath = Path.Combine(_repoPath, name);
        return new FileRepositoryBase<TItem>(_logger, itemRepoPath);
    }
    
    
    
}
