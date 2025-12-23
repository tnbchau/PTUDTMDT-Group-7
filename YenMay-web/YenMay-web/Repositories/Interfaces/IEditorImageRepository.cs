using YenMay_web.Models.Domain;

namespace YenMay_web.Repositories.Interfaces
{
    public interface IEditorImageRepository
    {
        Task AddAsync(EditorImage image);
        Task<EditorImage?> GetByIdAsync(int id);
        Task<IEnumerable<EditorImage>> GetByProductIdAsync(int productId);
        void Delete(EditorImage image);
        Task SaveAsync();
    }
}
