using Microsoft.EntityFrameworkCore; // Nhớ using cái này để có Async
using YenMay_web.Data;
using YenMay_web.Models.Domain;
using YenMay_web.Repositories.Interfaces;

namespace YenMay_web.Repositories.Implementations
{
    public class ProductRepository : IProductRepository
    {
        private readonly YMDbContext _context;
        public ProductRepository(YMDbContext context) => _context = context;

        // --- READ (ASYNC) ---
        public async Task<IEnumerable<Product>> GetAllAsync(string? includeProperties = null)
        {
            IQueryable<Product> query = _context.Products;
            if (!string.IsNullOrEmpty(includeProperties))
            {
                foreach (var prop in includeProperties.Split(','))
                    query = query.Include(prop);
            }
            return await query.ToListAsync(); // Dùng ToListAsync
        }

        public async Task<Product?> GetProductWithDetailsAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id); // Dùng FirstOrDefaultAsync
        }

        public async Task<Product?> GetByIdAsync(int id)
        {
            return await _context.Products.FindAsync(id); // FindAsync có sẵn
        }

        public async Task<ProductImage?> GetImageByIdAsync(int id)
        {
            return await _context.ProductImages.FindAsync(id);
        }

        public void Add(Product entity) => _context.Products.Add(entity);
        public void Update(Product entity) => _context.Products.Update(entity);
        public void Delete(Product entity) => _context.Products.Remove(entity);

        public void AddProductImage(ProductImage image) => _context.ProductImages.Add(image);
        public void DeleteProductImage(ProductImage image) => _context.ProductImages.Remove(image);

        public async Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAdminAsync(
            string? categoryName = null,
            string? searchTerm = null,
            string sortBy = "name",
            string sortOrder = "asc",
            int pageNumber = 1,
            int pageSize = 10)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images);
            if (!string.IsNullOrEmpty(categoryName))
            {
                query = query.Where(p => p.Category.Name == categoryName);
            }
            if (!string.IsNullOrEmpty(searchTerm))
            {
                query = query.Where(p => p.Name.Contains(searchTerm) || p.SKU.Contains(searchTerm));
            }
            query = sortBy.ToLower() switch
            {
                "name" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Name)
                    : query.OrderBy(p => p.Name),
                "sku" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.SKU)
                    : query.OrderBy(p => p.SKU),
                "stock" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.StockQuantity)
                    : query.OrderBy(p => p.StockQuantity),
                "price" => sortOrder == "desc"
                    ? query.OrderByDescending(p => p.Price)
                    : query.OrderBy(p => p.Price),
                _ => query.OrderBy(p => p.Name)
            };

            int totalCount = await query.CountAsync();
            var products = await query
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            return (products, totalCount);
        }
        public async Task<int> GetSoldCountAsync(int productId)
        {
            return await _context.Set<OrderItem>()
                .Where(oi => oi.ProductId == productId)
                .SumAsync(oi => oi.Quantity);
        }
        public async Task<bool> CheckSkuExistsAsync(string sku, int? excludeProductId = null)
        {
            var query = _context.Products.Where(p => p.SKU == sku);

            if (excludeProductId.HasValue)
            {
                query = query.Where(p => p.Id != excludeProductId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<Product?> GetBySlugAsync(string slug)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews) // Để hiện đánh giá ở trang chi tiết
                    .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(p => p.Slug == slug);
        }
        public async Task<Product?> GetProductWithImagesAsync(int id)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == id);
        }
        public async Task<List<Product>> GetRelatedProductsAsync(int categoryId, int productId, int take = 4)
        {
            return await _context.Products
                .AsNoTracking()
                .Where(p => p.CategoryId == categoryId && p.Id != productId && p.StockQuantity > 0)
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedDate) // Lấy sp mới nhất
                .Take(take)
                .ToListAsync();
        }
        public async Task<(List<ProductReview> Reviews, int TotalCount)> GetReviewsByProductIdAsync(int productId, int page, int pageSize)
        {
            var query = _context.Set<ProductReview>() 
                .AsNoTracking()
                .Where(r => r.ProductId == productId);
            var totalCount = await query.CountAsync();

            var reviews = await query
                .Include(r => r.User) // Include User để lấy Tên và Avatar người bình luận
                .OrderByDescending(r => r.CreatedAt) // Mới nhất lên đầu
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (reviews, totalCount);
        }
        public async Task<(IEnumerable<Product>, int)> GetProductsAsync(
            string? searchTerm,
            string? categorySlug,
            decimal? minPrice,
            decimal? maxPrice,
            string? sortBy,
            int page,
            int pageSize)
        {
            IQueryable<Product> query = _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .AsNoTracking();

            // Search
            if (!string.IsNullOrWhiteSpace(searchTerm))
                query = query.Where(p => p.Name.Contains(searchTerm));

            // Category
            if (!string.IsNullOrWhiteSpace(categorySlug))
                query = query.Where(p => p.Category.Slug == categorySlug);

            // Price
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);

            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            // Sort
            query = sortBy switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedDate),
                _ => query.OrderByDescending(p => p.CreatedDate)
            };

            int totalCount = await query.CountAsync();

            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
            var productIds = products.Select(p => p.Id).ToList();

            if (productIds.Any())
            {
                // Query bảng OrderItem, chỉ lấy dữ liệu của các ID trên, Group lại và tính tổng
                var soldCounts = await _context.Set<OrderItem>() 
                    .Where(oi => productIds.Contains(oi.ProductId))
                    .GroupBy(oi => oi.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        TotalSold = g.Sum(x => x.Quantity)
                    })
                    .ToDictionaryAsync(x => x.ProductId, x => x.TotalSold);

                // Gán dữ liệu ngược lại vào list products
                foreach (var product in products)
                {
                    if (soldCounts.TryGetValue(product.Id, out int count))
                    {
                        product.SoldCount = count;
                    }
                    else
                    {
                        product.SoldCount = 0;
                    }
                }
            }

            return (products, totalCount);
        }
        // Sản phẩm mới nhất
        public async Task<IEnumerable<Product>> GetNewestProductsAsync(int count)
        {
            return await _context.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedDate)
                .Take(count)
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count)
        {
            return await _context.Products
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Include(p => p.Reviews)
                .OrderByDescending(p => p.CreatedDate) // Thay SoldCount bằng CreatedDate hoặc Id
                .Take(count)
                .ToListAsync();
        }
    }
}