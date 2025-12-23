using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface IProductRepository
    {
        Task<IEnumerable<Product>> GetAllAsync(string? includeProperties = null);
        Task<Product?> GetProductWithDetailsAsync(int id);
        Task<Product?> GetByIdAsync(int id);

        void Add(Product entity);
        void Update(Product entity);
        void Delete(Product entity);

        // Ảnh
        void AddProductImage(ProductImage image);
        void DeleteProductImage(ProductImage image);

        Task<ProductImage?> GetImageByIdAsync(int id);
        Task<Product?> GetBySlugAsync(string slug);

        // Quản trị: Lấy danh sách sản phẩm với phân trang, lọc, sắp xếp
        Task<(IEnumerable<Product> Products, int TotalCount)> GetProductsAdminAsync(
            string? categoryName = null,
            string? searchTerm = null,
            string sortBy = "name",
            string sortOrder = "asc",
            int pageNumber = 1,
            int pageSize = 10);

        // Lấy số lượng đã bán
        Task<int> GetSoldCountAsync(int productId);
        Task<bool> CheckSkuExistsAsync(string sku, int? excludeProductId = null);
        Task<Product?> GetProductWithImagesAsync(int id);

        //User: Lấy sản phẩm với bộ lọc, phân trang, sắp xếp
        Task<(IEnumerable<Product> Products, int TotalCount)>GetProductsAsync(
                string? searchTerm,
                string? categorySlug,
                decimal? minPrice,
                decimal? maxPrice,
                string? sortBy,
                int page = 1,
                int pageSize = 12);
        Task<List<Product>> GetRelatedProductsAsync(int categoryId, int productId, int take = 4);
        Task<(List<ProductReview> Reviews, int TotalCount)> GetReviewsByProductIdAsync(int productId, int page, int pageSize);
        Task<IEnumerable<Product>> GetNewestProductsAsync(int count);
        Task<IEnumerable<Product>> GetFeaturedProductsAsync(int count);

    }
}