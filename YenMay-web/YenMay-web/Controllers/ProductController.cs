using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Models.ViewModels.Products;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Controllers
{
    public class ProductController : Controller
    {
        private readonly IUnitOfWork _uow;

        public ProductController(IUnitOfWork uow)
        {
            _uow = uow;
        }

        #region 1. Trang Danh sách sản phẩm (Index)

        [HttpGet("/san-pham")]
        [HttpGet("/san-pham/{categorySlug?}")]
        public async Task<IActionResult> Index(
            string? searchTerm,
            string? categorySlug,
            decimal? minPrice,
            decimal? maxPrice,
            string sortBy = "default",
            int page = 1,
            int pageSize = 12)
        {
            // 1. Gọi Repository lấy dữ liệu
            var (products, totalCount) = await _uow.Product.GetProductsAsync(
                searchTerm, categorySlug, minPrice, maxPrice, sortBy, page, pageSize
            );

            // 2. Lấy danh mục cho Sidebar
            var categories = await _uow.Category.GetAllAsync();

            // 3. Map Entity -> ViewModel (Dùng hàm Helper bên dưới)
            var productCards = products.Select(MapToProductCard).ToList();

            // 4. Tạo ViewModel tổng
            var viewModel = new ProductListViewModel
            {
                Products = productCards,
                Categories = categories,
                TotalProducts = totalCount,
                Filter = new ProductFilterViewModel
                {
                    SearchTerm = searchTerm,
                    CategorySlug = categorySlug,
                    MinPrice = minPrice,
                    MaxPrice = maxPrice,
                    SortBy = sortBy,
                    PageSize = pageSize
                },
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                },
                AppliedMinPrice = minPrice,
                AppliedMaxPrice = maxPrice
            };

            // Setup Title
            ViewBag.Title = string.IsNullOrEmpty(categorySlug)
                ? "Tất cả sản phẩm"
                : categories.FirstOrDefault(c => c.Slug == categorySlug)?.Name;

            return View(viewModel);
        }

        #endregion

        #region 2. Trang Chi tiết sản phẩm (Detail)

        [HttpGet("san-pham/{categorySlug}/{productSlug}")]
        public async Task<IActionResult> Detail(string categorySlug, string productSlug)
        {
            if (string.IsNullOrEmpty(productSlug)) return RedirectToAction(nameof(Index));

            // 1. Lấy thông tin sản phẩm
            var product = await _uow.Product.GetBySlugAsync(productSlug);
            if (product == null) return NotFound();

            // 2. SEO Redirect: Nếu sai Category thì redirect về đúng link chuẩn (301)
            if (product.Category != null && product.Category.Slug != categorySlug)
            {
                return RedirectToActionPermanent("Detail", new
                {
                    categorySlug = product.Category.Slug,
                    productSlug = product.Slug
                });
            }

            // 3. Lấy số lượng đã bán (nếu repository chưa join sẵn)
            product.SoldCount = await _uow.Product.GetSoldCountAsync(product.Id);

            // 4. Map dữ liệu sang ProductDetailViewModel
            var viewModel = new ProductDetailViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                ShortDescriptionHtml = product.ShortDescriptionHTML ?? string.Empty,
                FullDescriptionHtml = product.FullDescriptionHTML ?? string.Empty,
                AdditionalInfoHtml = product.AdditionalInfoHTML ?? string.Empty,
                StockQuantity = product.StockQuantity,
                CategoryName = product.Category?.Name ?? "Sản phẩm",
                CategorySlug = product.Category?.Slug ?? "",

                // Map ảnh: Nếu không có ảnh thì tạo list chứa ảnh mặc định
                Images = product.Images != null && product.Images.Any()
                    ? product.Images.Select((img, index) => new ProductImageViewModel
                    {
                        Id = img.Id,
                        ImageUrl = ImageHelper.GetImageUrl(img.ImageUrl),
                        AltText = $"{product.Name} - {index + 1}"
                    }).ToList()
                    : new List<ProductImageViewModel> {
                        new ProductImageViewModel { Id=0, ImageUrl="/images/no-image.png", AltText=product.Name }
                    },

                // Map Reviews: Lấy 3 review mới nhất để hiển thị ban đầu
                Reviews = product.Reviews?.OrderByDescending(r => r.CreatedAt).Take(3).Select(r => new ProductReviewViewModel
                {
                    Id = r.Id,
                    Rating = r.Rating,
                    Comment = r.Comment ?? "",
                    UserName = r.User?.UserName ?? "Khách hàng",
                    UserAvatar = "/images/default-avatar.png",
                    CreatedAt = r.CreatedAt,
                    ProductId = r.ProductId
                }).ToList() ?? new List<ProductReviewViewModel>()
            };

            // 5. Xử lý Sản phẩm liên quan (Sử dụng hàm Repository mới viết)
            if (product.CategoryId > 0)
            {
                // Lấy danh sách Entity
                var relatedEntities = await _uow.Product.GetRelatedProductsAsync(product.CategoryId, product.Id, take: 4);

                // Map sang ViewModel (Dùng chung hàm MapToProductCard)
                // Lưu ý: Hàm Repo Related đơn giản không lấy SoldCount, ta có thể bỏ qua hoặc gọi thêm nếu cần thiết.
                // Để đơn giản và nhanh, ta chấp nhận SoldCount = 0 hoặc gọi vòng lặp nếu số lượng ít (4 sp).
                foreach (var p in relatedEntities)
                {
                    p.SoldCount = await _uow.Product.GetSoldCountAsync(p.Id);
                }

                viewModel.RelatedProducts = relatedEntities.Select(MapToProductCard).ToList();
            }

            // 6. Tạo Breadcrumbs & SEO Metadata
            viewModel.Breadcrumbs = BuildBreadcrumbs(product);
            SetupSeoMetadata(product, viewModel);

            return View(viewModel);
        }

        #endregion

        #region 3. API Endpoints (AJAX)

        // API lấy đánh giá (Sử dụng hàm Repository mới viết - Phân trang DB)
        [HttpGet("api/product/{id}/reviews")]
        public async Task<IActionResult> GetProductReviews(int id, int page = 1, int pageSize = 5)
        {
            // Gọi Repository lấy list review và tổng số lượng
            var (reviewsEntities, totalCount) = await _uow.Product.GetReviewsByProductIdAsync(id, page, pageSize);

            // Map sang ViewModel
            var reviews = reviewsEntities.Select(r => new ProductReviewViewModel
            {
                Id = r.Id,
                Rating = r.Rating,
                Comment = r.Comment ?? "",
                UserName = r.User?.UserName ?? "Khách hàng",
                UserAvatar = "/images/default-avatar.png",
                CreatedAt = r.CreatedAt,
                ProductId = r.ProductId
            }).ToList();

            return Ok(new
            {
                success = true,
                reviews,
                totalCount,
                totalPages = (int)Math.Ceiling(totalCount / (double)pageSize),
                currentPage = page
            });
        }

        // API kiểm tra tồn kho
        [HttpGet("api/product/{id}/check-stock")]
        public async Task<IActionResult> CheckStock(int id, int quantity = 1)
        {
            // Để đơn giản, dùng GetById, nhưng tốt nhất nên có hàm GetStockOnly trong Repo để nhẹ hơn
            var product = await _uow.Product.GetByIdAsync(id);

            if (product == null)
                return NotFound(new { success = false, message = "Sản phẩm không tồn tại" });

            bool isAvailable = product.StockQuantity >= quantity;

            return Ok(new
            {
                success = true,
                stockQuantity = product.StockQuantity,
                inStock = product.StockQuantity > 0,
                isAvailable = isAvailable,
                message = isAvailable ? "Còn hàng" : $"Chỉ còn {product.StockQuantity} sản phẩm"
            });
        }

        #endregion

        #region 4. Helper Methods (Private)

        // Helper: Map từ Entity Product sang ProductCardViewModel
        // Dùng chung cho cả Index và Related Products
        private ProductCardViewModel MapToProductCard(Product p)
        {
            var firstImage = p.Images?.FirstOrDefault();

            return new ProductCardViewModel
            {
                Id = p.Id,
                Name = p.Name,
                Slug = p.Slug ?? string.Empty,
                Price = p.Price,
                ThumbnailUrl = firstImage != null
                    ? ImageHelper.GetImageUrl(firstImage.ImageUrl)
                    : "/images/no-image.png",
                ShortDescription = p.ShortDescriptionHTML,
                CategoryName = p.Category?.Name ?? "",
                CategorySlug = p.Category?.Slug ?? "",
                StockQuantity = p.StockQuantity,

                // Tính toán rating từ Reviews đã Include
                AverageRating = p.Reviews != null && p.Reviews.Any() ? p.Reviews.Average(r => r.Rating) : 0,
                ReviewCount = p.Reviews?.Count ?? 0,

                SoldCount = p.SoldCount,
                IsNew = (DateTime.Now - p.CreatedDate).TotalDays <= 30 // Set 30 ngày là sp mới
            };
        }

        private List<BreadcrumbItemViewModel> BuildBreadcrumbs(Product product)
        {
            var list = new List<BreadcrumbItemViewModel>
            {
                new BreadcrumbItemViewModel { Name = "Trang chủ", Url = "/", IsActive = false },
                new BreadcrumbItemViewModel { Name = "Sản phẩm", Url = "/san-pham", IsActive = false }
            };

            if (product.Category != null)
            {
                list.Add(new BreadcrumbItemViewModel
                {
                    Name = product.Category.Name,
                    Url = $"/san-pham/{product.Category.Slug}",
                    IsActive = false
                });
            }
            // Item cuối cùng là tên sản phẩm (Active)
            list.Add(new BreadcrumbItemViewModel { Name = product.Name, Url = "#", IsActive = true });
            return list;
        }

        private void SetupSeoMetadata(Product product, ProductDetailViewModel vm)
        {
            // Meta Description: Lấy từ mô tả ngắn, bỏ HTML tag
            string metaDesc = "";
            if (!string.IsNullOrEmpty(product.ShortDescriptionHTML))
            {
                var plainText = Regex.Replace(product.ShortDescriptionHTML, "<.*?>", "");
                metaDesc = plainText.Length > 150 ? plainText.Substring(0, 147) + "..." : plainText;
            }
            else
            {
                metaDesc = $"Mua {product.Name} chính hãng, giá tốt {product.Price:N0}₫.";
            }

            ViewBag.MetaDescription = metaDesc;

            // Meta Keywords
            var keywords = new List<string> { "yến sào", product.Name };
            if (product.Category != null) keywords.Add(product.Category.Name);
            ViewBag.MetaKeywords = string.Join(", ", keywords);

            // Canonical URL
            ViewBag.CanonicalUrl = $"{Request.Scheme}://{Request.Host}/san-pham/{product.Category?.Slug}/{product.Slug}";

            // Open Graph Image (Cho Facebook/Zalo share)
            ViewBag.OgImage = vm.Images.FirstOrDefault()?.ImageUrl ?? "/images/no-image.png";
        }

        #endregion
    }
}