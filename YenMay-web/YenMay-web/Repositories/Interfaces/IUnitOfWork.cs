namespace YenMay_web.Repositories.Interfaces
{
    public interface IUnitOfWork
    {
        IProductRepository Product { get; }
        ICategoryRepository Category { get; }
        IEditorImageRepository EditorImage { get; }
        IArticleRepository Article { get; }
        ICategoryArticleRepository CategoryArticle { get; }
        ICartRepository Cart { get; }
        IOrderRepository Order { get; }
        IShippingRepository Shipping { get; }

        Task SaveAsync();
    }
}