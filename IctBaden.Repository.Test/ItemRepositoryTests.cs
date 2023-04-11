using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using IctBaden.Framework.Logging;
using Xunit;

namespace IctBaden.Repository.Test;

public class ItemRepositoryTests : IDisposable
{
    private readonly FileRepository _fileRepository;
    private readonly string _repoPath;

    public ItemRepositoryTests()
    {
        var logger = Logger.DefaultFactory.CreateLogger("Test");
        _repoPath = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        _fileRepository = new FileRepository(logger, _repoPath);
    }

    public void Dispose()
    {
        Task.Delay(100).Wait();
        Directory.Delete(_repoPath, true);
    }
    
    [Fact]
    public void FileRepositoryShouldCreatePath()
    {
        Assert.True(Directory.Exists(_repoPath));
    }
    
    [Fact]
    public void CreateItemRepoShouldCreateItemPath()
    {
        _fileRepository.CreateItemRepository<TestItem>();
        var itemRepositoryPath = Path.Combine(_repoPath, "TestItem");
        Assert.True(Directory.Exists(itemRepositoryPath));
    }
    
    [Fact]
    public void CreatedItemsShouldBeReturnedInGetAll()
    {
        var itemRepository = _fileRepository.CreateItemRepository<TestItem>();

        var testItem1 = new TestItem { Id = 5, Text = "Test1" };
        itemRepository.Create(testItem1, testItem1.Id.ToString());

        var testItem2 = new TestItem { Id = 123, Text = "Test2" };
        itemRepository.Create(testItem2, testItem2.Id.ToString());

        var testItem3 = new TestItem { Id = 1000, Text = "Test3" };
        itemRepository.Create(testItem3, testItem3.Id.ToString());

        var items = itemRepository.GetAll();
        Assert.Equal(3, items.Length);
    }

    [Fact]
    public void UpdatedItemsShouldBeReturnedInGetAll()
    {
        var itemRepository = _fileRepository.CreateItemRepository<TestItem>();

        var initialItems = new[]
        {
            new TestItem { Id = 5, Text = "Test1" },
            new TestItem { Id = 123, Text = "Test2" },
            new TestItem { Id = 1000, Text = "Test3" }
        };
        foreach (var item in initialItems)
        {
            itemRepository.Create(item, item.Id.ToString());
        }

        var items = itemRepository.GetAll()
            .OrderBy(i => i.Id)
            .ToArray();
        
        for (var ix = 0; ix < 3; ix++)
        {
            Assert.Equal(initialItems[ix].Id, items[ix].Id);    
            Assert.Equal(initialItems[ix].Text, items[ix].Text);    
        }

        initialItems[1].Text = "UpdatedText2";
        itemRepository.Update(initialItems[1], initialItems[1].Id.ToString());
        
        var updated = itemRepository.GetById(initialItems[1].Id.ToString());
        Assert.NotNull(updated);
    }

    [Fact]
    public void DeletingItemShouldRemoveFile()
    {
        var itemRepository = _fileRepository.CreateItemRepository<TestItem>();
        var itemPath = Path.Combine(_repoPath, "TestItem", "123.json");
        
        var testItem1 = new TestItem { Id = 5, Text = "Test1" };
        itemRepository.Create(testItem1, testItem1.Id.ToString());

        var testItem2 = new TestItem { Id = 123, Text = "Test2" };
        itemRepository.Create(testItem2, testItem2.Id.ToString());

        var testItem3 = new TestItem { Id = 1000, Text = "Test3" };
        itemRepository.Create(testItem3, testItem3.Id.ToString());

        Assert.True(File.Exists(itemPath)); 
        
        itemRepository.Delete(testItem2.Id.ToString());
        
        Assert.False(File.Exists(itemPath)); 
    }
    
}