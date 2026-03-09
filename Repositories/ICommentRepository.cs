using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public interface ICommentRepository
    {
        Task<Comment?> GetByIdAsync(int commentId);
        Task<IEnumerable<Comment>> GetByPostIdAsync(int postId);
        Task<Comment> CreateAsync(Comment comment);
        Task<bool> DeleteAsync(int commentId);
        Task<int> GetCommentCountByPostAsync(int postId);
    }
}
