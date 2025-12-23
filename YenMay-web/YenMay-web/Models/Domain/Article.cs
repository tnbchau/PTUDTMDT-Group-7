using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace YenMay_web.Models.Domain
{
    public class Article
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string ShortDescription { get; set; } 
        public string Content { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedDate { get; set; } 
        public bool IsPublished { get; set; }
        public string ImageUrl { get; set; }
        [Display(Name = "Danh mục bài viết")]
        public int? CategoryArticleId { get; set; } 

        [ForeignKey("CategoryArticleId")]
        public CategoryArticle? CategoryArticle { get; set; }
    }
}