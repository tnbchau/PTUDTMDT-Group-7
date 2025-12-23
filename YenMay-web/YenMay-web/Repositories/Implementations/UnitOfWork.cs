using Microsoft.CodeAnalysis.CSharp.Syntax;
using YenMay_web.Data;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly YMDbContext _context;

        public IProductRepository Product { get; private set; }
        public ICategoryRepository Category { get; private set; }
        public IEditorImageRepository EditorImage { get; private set; }
        public IArticleRepository Article { get; private set; }
        public ICategoryArticleRepository CategoryArticle { get; private set; }
        public ICartRepository Cart { get; private set; }
        public IOrderRepository Order { get; private set; }
        public IShippingRepository Shipping { get; private set; }
        public UnitOfWork(YMDbContext context)
        {
            _context = context;
            Product = new ProductRepository(_context);
            Category = new CategoryRepository(_context);
            EditorImage = new EditorImageRepository(_context);
            Article = new ArticleRepository(_context);
            CategoryArticle = new CategoryArticleRepository(_context);
            Cart = new CartRepository(_context);
            Order = new OrderRepository(_context);
            Shipping = new ShippingRepository(_context);
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
