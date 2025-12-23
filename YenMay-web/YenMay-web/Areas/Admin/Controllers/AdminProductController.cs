using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YenMay_web.Areas.Admin.ViewModels;
using YenMay_web.Areas.Admin.ViewModels.AdminProduct;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class AdminProductController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;
        private const int DEFAULT_CATEGORY_ID = 8; // ID của "Chưa phân loại" trong Database của bạn

        public AdminProductController(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // 1. DANH SÁCH SẢN PHẨM (INDEX)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(AdminProductIndexViewModel filter)
        {
            // 1. Thiết lập các thông số cơ bản
            int pageSize = 10;
            int page = filter.Page > 0 ? filter.Page : 1;

            // 2. Lấy tên danh mục để truyền vào Repository (Dựa trên cấu trúc Repo hiện tại của bạn)
            string? categoryName = null;
            if (filter.SelectedCategoryId > 0)
            {
                var category = await _uow.Category.GetByIdAsync(filter.SelectedCategoryId);
                categoryName = category?.Name;
            }

            // 3. Gọi Repository - Đảm bảo tham số truyền vào đúng thứ tự
            // Lưu ý: filter.SortBy và filter.SortOrder phải khớp với logic xử lý trong ProductRepository
            var (products, totalCount) = await _uow.Product.GetProductsAdminAsync(
                categoryName,
                filter.SearchTerm,
                filter.SortBy ?? "name",
                filter.SortOrder ?? "asc",
                page,
                pageSize);

            // 4. Mapping sang RowViewModel
            var productVms = products.Select(p => new AdminProductRowViewModel
            {
                Id = p.Id,
                Name = p.Name,
                SKU = p.SKU,
                Price = p.Price,
                StockQuantity = p.StockQuantity,
                IsLowStock = p.StockQuantity < 10,
                SoldCount = p.SoldCount,
                CategoryName = p.Category?.Name ?? "Chưa phân loại",
                ImageUrl = (p.Images != null && p.Images.Any())
                            ? p.Images.OrderBy(i => i.Id).FirstOrDefault()?.ImageUrl
                            : "images/no-image.png"
            }).ToList();

            // 5. Chuẩn bị dữ liệu cho Dropdown danh mục
            var allCategories = await _uow.Category.GetAllAsync();

            // 6. Gán ngược dữ liệu vào chính đối tượng filter để hiển thị lại trên Form
            filter.Products = productVms;
            filter.Pagination = new PaginationViewModel
            {
                PageIndex = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };
            filter.Page = page;
            filter.CategoryOptions = allCategories.Select(c => new SelectListItem
            {
                Value = c.Id.ToString(),
                Text = c.Name,
                Selected = filter.SelectedCategoryId == c.Id
            });

            return View(filter);
        }

        // ==========================================
        // 2. THÊM SẢN PHẨM (CREATE)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var viewModel = new AdminProductFormViewModel();
            await PopulateCategoryDropdown(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminProductFormViewModel model)
        {
            if (await _uow.Product.CheckSkuExistsAsync(model.SKU))
                ModelState.AddModelError("SKU", "Mã SKU này đã tồn tại.");

            if (ModelState.IsValid)
            {
                var product = new Product
                {
                    Name = model.Name,
                    SKU = model.SKU,
                    Price = model.Price,
                    StockQuantity = model.StockQuantity,
                    CategoryId = model.CategoryId,
                    ShortDescriptionHTML = model.ShortDescriptionHTML,
                    FullDescriptionHTML = model.FullDescriptionHTML,
                    AdditionalInfoHTML = model.AdditionalInfoHTML,
                    Slug = SlugHelper.GenerateSlug(model.Name),
                    CreatedDate = DateTime.Now,
                    Images = new List<ProductImage>()
                };

                if (model.ImageFiles != null)
                {
                    var category = await _uow.Category.GetByIdAsync(model.CategoryId);
                    string catName = category?.Name ?? "Uncategorized";
                    int index = 0;
                    foreach (var file in model.ImageFiles)
                    {
                        string path = await SaveProductImage(file, catName, model.Name, index++);
                        product.Images.Add(new ProductImage { ImageUrl = path, Product = product });
                    }
                }

                _uow.Product.Add(product);
                await _uow.SaveAsync();
                TempData["Success"] = "Thêm sản phẩm thành công";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoryDropdown(model);
            return View(model);
        }

        // ==========================================
        // 3. SỬA SẢN PHẨM (EDIT)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var product = await _uow.Product.GetProductWithImagesAsync(id);
            if (product == null) return NotFound();

            var viewModel = new AdminProductFormViewModel
            {
                Id = product.Id,
                Name = product.Name,
                SKU = product.SKU,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                CategoryId = product.CategoryId,
                ShortDescriptionHTML = product.ShortDescriptionHTML,
                FullDescriptionHTML = product.FullDescriptionHTML,
                AdditionalInfoHTML = product.AdditionalInfoHTML,
                ExistingImages = product.Images.Select(i => new ProductAdminImageViewModel { Id = i.Id, ImageUrl = i.ImageUrl }).ToList()
            };
            await PopulateCategoryDropdown(viewModel);
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminProductFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                var product = await _uow.Product.GetProductWithImagesAsync(id);
                if (product == null) return NotFound();

                product.Name = model.Name;
                product.SKU = model.SKU;
                product.Price = model.Price;
                product.StockQuantity = model.StockQuantity;
                product.CategoryId = model.CategoryId;
                product.ShortDescriptionHTML = model.ShortDescriptionHTML;
                product.FullDescriptionHTML = model.FullDescriptionHTML;
                product.AdditionalInfoHTML = model.AdditionalInfoHTML;
                product.Slug = SlugHelper.GenerateSlug(model.Name);

                // Xử lý xóa ảnh cũ
                if (model.DeletedImageIds != null)
                {
                    foreach (var imgId in model.DeletedImageIds)
                    {
                        var img = product.Images.FirstOrDefault(i => i.Id == imgId);
                        if (img != null)
                        {
                            ImageHelper.DeleteImageFile(_webHostEnvironment, img.ImageUrl);
                            _uow.Product.DeleteProductImage(img);
                        }
                    }
                }

                // Thêm ảnh mới
                if (model.ImageFiles != null)
                {
                    var category = await _uow.Category.GetByIdAsync(model.CategoryId);
                    int startIndex = product.Images.Count;
                    foreach (var file in model.ImageFiles)
                    {
                        string path = await SaveProductImage(file, category?.Name ?? "Uncategorized", product.Name, startIndex++);
                        _uow.Product.AddProductImage(new ProductImage { ImageUrl = path, ProductId = product.Id });
                    }
                }

                _uow.Product.Update(product);
                await _uow.SaveAsync();
                TempData["Success"] = "Cập nhật thành công";
                return RedirectToAction(nameof(Index));
            }
            await PopulateCategoryDropdown(model);
            return View(model);
        }

        // ==========================================
        // 4. XÓA SẢN PHẨM
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _uow.Product.GetProductWithImagesAsync(id);
            if (product != null)
            {
                foreach (var img in product.Images) ImageHelper.DeleteImageFile(_webHostEnvironment, img.ImageUrl);
                _uow.Product.Delete(product);
                await _uow.SaveAsync();
                TempData["Success"] = "Đã xóa sản phẩm";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // 5. QUẢN LÝ DANH MỤC (CATEGORY)
        // ==========================================

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (!string.IsNullOrWhiteSpace(categoryName))
            {
                var newCat = new Category
                {
                    Name = categoryName.Trim(),
                    Slug = SlugHelper.GenerateSlug(categoryName)
                };
                await _uow.Category.AddAsync(newCat);
                await _uow.SaveAsync();
                TempData["Success"] = "Thêm danh mục thành công";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, string newName)
        {
            var cat = await _uow.Category.GetByIdAsync(id);
            if (cat != null && !string.IsNullOrWhiteSpace(newName))
            {
                cat.Name = newName.Trim();
                cat.Slug = SlugHelper.GenerateSlug(newName);
                await _uow.Category.UpdateAsync(cat);
                await _uow.SaveAsync();
                TempData["Success"] = "Đã đổi tên danh mục";
            }
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            // Không cho phép xóa danh mục "Chưa phân loại" (ID = 8)
            if (id == DEFAULT_CATEGORY_ID)
            {
                TempData["Error"] = "Không thể xóa danh mục mặc định.";
                return RedirectToAction(nameof(Index));
            }

            var catToDelete = await _uow.Category.GetByIdAsync(id);
            if (catToDelete != null)
            {
                var products = await _uow.Product.GetAllAsync();
                var productsInCat = products.Where(p => p.CategoryId == id).ToList();

                foreach (var p in productsInCat)
                {
                    p.CategoryId = DEFAULT_CATEGORY_ID;
                    _uow.Product.Update(p);
                }
                await _uow.Category.DeleteAsync(id);
                await _uow.SaveAsync();
                TempData["Success"] = $"Đã xóa danh mục. {productsInCat.Count} sản phẩm được chuyển vào 'Chưa phân loại'.";
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // HELPERS
        // ==========================================
        private async Task PopulateCategoryDropdown(AdminProductFormViewModel model)
        {
            var categories = await _uow.Category.GetAllAsync();
            model.CategoryList = categories.Select(c => new SelectListItem { Value = c.Id.ToString(), Text = c.Name });
        }

        private async Task<string> SaveProductImage(IFormFile file, string categoryName, string productName, int index)
        {
            string relativePath = ImageHelper.GenerateImagePath(categoryName, productName, file.FileName, index);
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);
            using (var stream = new FileStream(fullPath, FileMode.Create)) { await file.CopyToAsync(stream); }
            return relativePath;
        }
    }
}