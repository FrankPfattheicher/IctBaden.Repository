using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
// ReSharper disable TemplateIsNotCompileTimeConstantProblem

namespace IctBaden.Repository;

internal class FileRepositoryBase<TItem> : IItemRepository<TItem>
    where TItem : new()
{
    private readonly ILogger _logger;
    private readonly string _repoPath;

    // ReSharper disable once StaticMemberInGenericType
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = null,
        PropertyNameCaseInsensitive = true,
        IgnoreReadOnlyProperties = false,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Converters = { new JsonStringEnumConverter() }
    };

    protected internal FileRepositoryBase(ILogger logger, string repoPath)
    {
        _logger = logger;
        _repoPath = repoPath;

        if (!Directory.Exists(_repoPath))
        {
            Directory.CreateDirectory(_repoPath);
        }
    }

    private string GetFileTimeStamp(DateTime timestamp)
    {
        return $"{timestamp.Year:0000}-{timestamp.Month:00}-{timestamp.Day:00}T{timestamp.Hour:00}{timestamp.Minute:00}{timestamp.Second:00}.{timestamp.Millisecond:000}Z";
    }

    private bool FileInRange(string fullPath,  string fromDate, string toDate)
    {
        var fileName = Path.GetFileName(fullPath);
        return string.Compare(fileName, fromDate, StringComparison.InvariantCultureIgnoreCase) >= 0 && 
               string.Compare(fileName, toDate, StringComparison.InvariantCultureIgnoreCase) <= 0;
    }


    private string[] GetFileNamesRange(DateTime from, DateTime to, uint maxCount, bool recent)
    {
        if (from.Year < 2000) from = new DateTime(2000, 1, 1, 0,0,0,DateTimeKind.Utc);
        if(to.Year < 2000) to = new DateTime(2000, 1, 1, 0,0,0,DateTimeKind.Utc);

        var fromDate = GetFileTimeStamp(from);
        var toDate = GetFileTimeStamp(to);
        
        if (maxCount > int.MaxValue) maxCount = int.MaxValue;
            
        var itemFiles = Directory
            .EnumerateFiles(_repoPath, "*.json");
            
        itemFiles = recent 
            ? itemFiles.OrderByDescending(Path.GetFileName)
            : itemFiles.OrderBy(Path.GetFileName);
            
        itemFiles = itemFiles
            .Where(fullPath => FileInRange(fullPath, fromDate, toDate))
            .Take((int)maxCount);

        return itemFiles.ToArray();
    }

    protected TItem[] GetRecentFileRange(DateTime from, DateTime to, uint maxCount) =>
        GetFileRange(from, to, maxCount, true);

    private TItem[] GetFileRange(DateTime from, DateTime to, uint maxCount, bool recent = false)
    {
        var itemFiles = GetFileNamesRange(from, to, maxCount, recent); 
        var items = new List<TItem>();
        foreach (var itemFile in itemFiles)
        {
            try
            {
                var json = File.ReadAllText(itemFile);
                var item = JsonSerializer.Deserialize<TItem>(json, Options);
                if (item != null) items.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }
        return items.ToArray();
    }
        
    public TItem[] GetAll()
    {
        var itemFiles = Directory.EnumerateFiles(_repoPath, "*.json");
        var items = new List<TItem>();
        foreach (var itemFile in itemFiles)
        {
            try
            {
                var json = File.ReadAllText(itemFile);
                var item = JsonSerializer.Deserialize<TItem>(json, Options);
                if (item != null) items.Add(item);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        return items.ToArray();
    }

    private string GetItemFileName(string itemId) => Path.Combine(_repoPath, $"{itemId}.json");

    public TItem GetById(string itemId)
    {
        var itemFile = GetItemFileName(itemId);
        if (!File.Exists(itemFile)) return new TItem();
            
        var json = File.ReadAllText(itemFile);
        var item = JsonSerializer.Deserialize<TItem>(json, Options);
        return item ?? new TItem();
    }

    public void Create(TItem item, string itemId)
    {
        var json = JsonSerializer.Serialize(item, Options);
        var fileName = GetItemFileName(itemId);
        File.WriteAllText(fileName, json);
    }

    public void Update(TItem item, string itemId)
    {
        var json = JsonSerializer.Serialize(item, Options);
        var fileName = GetItemFileName(itemId);
        File.WriteAllText(fileName, json);
    }

    public void Delete(string itemId)
    {
        var fileName = GetItemFileName(itemId);
        if (File.Exists(fileName))
        {
            File.Delete(fileName);
        }
    }
}
