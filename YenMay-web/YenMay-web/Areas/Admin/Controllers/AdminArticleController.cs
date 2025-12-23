using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using YenMay_web.Areas.Admin.ViewModels.AdminArticle;
using YenMay_web.Models.Domain;
using YenMay_web.Models.ViewModels.Common;
using YenMay_web.Repositories.Interfaces;
using YenMay_web.Utilities;

namespace YenMay_web.Areas.Admin.Controllers
{
    [Area("Admin")]
    
    public class AdminArticleController : Controller
    {
        private readonly IUnitOfWork _uow;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AdminArticleController(IUnitOfWork uow, IWebHostEnvironment webHostEnvironment)
        {
            _uow = uow;
            _webHostEnvironment = webHostEnvironment;
        }

        // ==========================================
        // INDEX: LIST, SEARCH, SORT, PAGINATION
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Index(string searchTerm, int? selectedCategoryId, string sortOrder = "newest", int page = 1)
        {
            // 1. Get Data
            var allArticles = await _uow.Article.GetAllAsync();
            var allCategories = await _uow.CategoryArticle.GetAllAsync();

            // 2. Filter by Category
            if (selectedCategoryId.HasValue && selectedCategoryId.Value > 0)
            {
                allArticles = allArticles.Where(a => a.CategoryArticleId == selectedCategoryId.Value);
            }

            // 3. Filter by Search Term
            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                allArticles = allArticles.Where(a =>
                    a.Title.ToLower().Contains(searchTerm) ||
                    (a.ShortDescription != null && a.ShortDescription.ToLower().Contains(searchTerm))
                );
            }

            // 4. Sort
            allArticles = sortOrder switch
            {
                "oldest" => allArticles.OrderBy(a => a.CreatedDate),
                "name_asc" => allArticles.OrderBy(a => a.Title),
                "name_desc" => allArticles.OrderByDescending(a => a.Title),
                _ => allArticles.OrderByDescending(a => a.CreatedDate)
            };

            // 5. Pagination
            int pageSize = 10;
            int totalCount = allArticles.Count();

            var pagedArticles = allArticles
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new AdminArticleRowViewModel
                {
                    Id = a.Id,
                    Title = a.Title,
                    ShortDescription = a.ShortDescription,
                    ImageUrl = a.ImageUrl,
                    IsPublished = a.IsPublished,
                    CreatedDate = a.CreatedDate,
                    UpdatedDate = a.UpdatedDate,
                    CategoryName = a.CategoryArticle != null ? a.CategoryArticle.Name : "Chưa phân loại"
                }).ToList();

            // 6. Populate View Model
            var viewModel = new AdminArticleIndexViewModel
            {
                Articles = pagedArticles,
                Categories = allCategories.ToList(),
                SearchTerm = searchTerm,
                SortOrder = sortOrder,
                SelectedCategoryId = selectedCategoryId,
                CategoryOptions = new SelectList(allCategories, "Id", "Name"),
                Pagination = new PaginationViewModel
                {
                    PageIndex = page,
                    PageSize = pageSize,
                    TotalCount = totalCount
                }
            };

            return View(viewModel);
        }

        // ==========================================
        // AJAX: CHECK CATEGORY USAGE (Dùng cho JS)
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> CheckCategoryUsage(int id)
        {
            var allArticles = await _uow.Article.GetAllAsync();
            int count = allArticles.Count(x => x.CategoryArticleId == id);
            return Json(new { hasArticles = count > 0, count = count });
        }

        // ==========================================
        // CREATE ARTICLE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Create()
        {
            var categories = await _uow.CategoryArticle.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name");

            return View(new AdminArticleFormViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AdminArticleFormViewModel model)
        {
            if (ModelState.IsValid)
            {
                string imageUrl = "";
                if (model.ImageFile != null)
                {
                    imageUrl = await SaveThumbnailImage(model.ImageFile, model.Title);
                }

                var article = new Article
                {
                    Title = model.Title,
                    Slug = SlugHelper.GenerateSlug(model.Title),
                    ShortDescription = model.ShortDescription,
                    Content = model.Content,
                    IsPublished = model.IsPublished,
                    CreatedDate = DateTime.UtcNow,
                    ImageUrl = imageUrl,
                    CategoryArticleId = model.CategoryArticleId 
                };

                await _uow.Article.AddAsync(article);
                await _uow.SaveAsync(); // Nhớ save
                TempData["Success"] = "Thêm bài viết thành công";
                return RedirectToAction(nameof(Index));
            }
            var categories = await _uow.CategoryArticle.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryArticleId);
            return View(model);
        }

        // ==========================================
        // EDIT ARTICLE
        // ==========================================
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var article = await _uow.Article.GetByIdAsync(id);
            if (article == null) return NotFound();

            var categories = await _uow.CategoryArticle.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", article.CategoryArticleId);

            var viewModel = new AdminArticleFormViewModel
            {
                Id = article.Id,
                Title = article.Title,
                ShortDescription = article.ShortDescription,
                Content = article.Content,
                IsPublished = article.IsPublished,
                CurrentImageUrl = article.ImageUrl,
                CategoryArticleId = article.CategoryArticleId
            };
            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, AdminArticleFormViewModel model)
        {
            if (id != model.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    var existingArticle = await _uow.Article.GetByIdAsync(id);
                    if (existingArticle == null) return NotFound();

                    existingArticle.Title = model.Title;
                    existingArticle.ShortDescription = model.ShortDescription;
                    existingArticle.Content = model.Content;
                    existingArticle.IsPublished = model.IsPublished;
                    existingArticle.Slug = SlugHelper.GenerateSlug(model.Title);
                    existingArticle.UpdatedDate = DateTime.UtcNow;
                    existingArticle.CategoryArticleId = model.CategoryArticleId;

                    if (model.ImageFile != null)
                    {
                        existingArticle.ImageUrl = await SaveThumbnailImage(model.ImageFile, model.Title);
                    }

                    await _uow.Article.UpdateAsync(existingArticle);
                    await _uow.SaveAsync();

                    TempData["Success"] = "Cập nhật bài viết thành công";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Lỗi hệ thống: " + ex.Message);
                }
            }
            var categories = await _uow.CategoryArticle.GetAllAsync();
            ViewBag.Categories = new SelectList(categories, "Id", "Name", model.CategoryArticleId);
            return View(model);
        }

        // ==========================================
        // DELETE ARTICLE
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var article = await _uow.Article.GetByIdAsync(id);
            if (article == null)
            {
                TempData["Error"] = "Không tìm thấy bài viết.";
                return RedirectToAction(nameof(Index));
            }
            await _uow.Article.DeleteAsync(id);
            await _uow.SaveAsync();
            TempData["Success"] = "Xóa bài viết thành công";
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // CREATE CATEGORY
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
            {
                TempData["Error"] = "Tên danh mục không được để trống";
                return RedirectToAction(nameof(Index));
            }

            try
            {
                var newCategory = new CategoryArticle
                {
                    Name = categoryName.Trim(),
                    Slug = SlugHelper.GenerateSlug(categoryName)
                };

                await _uow.CategoryArticle.AddAsync(newCategory);
                await _uow.SaveAsync(); // Quan trọng: Save Changes

                TempData["Success"] = "Thêm danh mục thành công";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi khi thêm danh mục: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
        // ==========================================
        // 3. EDIT CATEGORY
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCategory(int id, string newName)
        {
            var category = await _uow.CategoryArticle.GetByIdAsync(id);
            if (category != null && !string.IsNullOrWhiteSpace(newName))
            {
                category.Name = newName.Trim();
                category.Slug = SlugHelper.GenerateSlug(newName);
                await _uow.CategoryArticle.UpdateAsync(category);
                await _uow.SaveAsync();
                TempData["Success"] = "Cập nhật tên danh mục thành công";
            }
            return RedirectToAction(nameof(Index));
        }
        // ==========================================
        // DELETE CATEGORY (SAFE DELETE)
        // ==========================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            try
            {
                var category = await _uow.CategoryArticle.GetByIdAsync(id);
                if (category != null)
                {
                    // Gỡ bài viết khỏi danh mục trước khi xóa
                    var allArticles = await _uow.Article.GetAllAsync();
                    var articlesInCat = allArticles.Where(x => x.CategoryArticleId == id).ToList();

                    foreach (var item in articlesInCat)
                    {
                        item.CategoryArticleId = null; // Set về null (Chưa phân loại)
                        await _uow.Article.UpdateAsync(item);
                    }

                    await _uow.CategoryArticle.DeleteAsync(id);
                    await _uow.SaveAsync();
                    TempData["Success"] = $"Đã xóa danh mục. ({articlesInCat.Count} bài viết được giữ lại)";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Lỗi xóa danh mục: " + ex.Message;
            }
            return RedirectToAction(nameof(Index));
        }

        // ==========================================
        // HELPER
        // ==========================================
        private async Task<string> SaveThumbnailImage(IFormFile imageFile, string title)
        {
            string relativePath = ImageHelper.GenerateArticleImagePath(title, imageFile.FileName);
            string fullPath = Path.Combine(_webHostEnvironment.WebRootPath, relativePath.TrimStart('/'));
            string directory = Path.GetDirectoryName(fullPath);

            if (!Directory.Exists(directory)) Directory.CreateDirectory(directory!);

            using (var fileStream = new FileStream(fullPath, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return relativePath.StartsWith("/") ? relativePath : "/" + relativePath;
        }
    }
}