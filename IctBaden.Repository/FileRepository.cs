using Microsoft.Extensions.Logging;

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

    public IItemRepository<TItem> CreateItemRepository<TItem>() where TItem : new()
    {
        var itemType = typeof(TItem);
        var itemRepoPath = Path.Combine(_repoPath, itemType.Name);
        return new FileRepositoryBase<TItem>(_logger, itemRepoPath);
    }
    
}
