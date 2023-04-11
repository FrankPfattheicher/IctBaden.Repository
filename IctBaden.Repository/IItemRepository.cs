namespace IctBaden.Repository;

public interface IItemRepository<TItem>
{
    TItem[] GetAll();
    TItem? GetById(string itemId);

    void Create(TItem item, string itemId);
    void Update(TItem item, string itemId);
    void Delete(string itemId);
}
