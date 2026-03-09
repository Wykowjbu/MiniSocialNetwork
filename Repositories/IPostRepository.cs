using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public interface IPostRepository
    {
        Task<Post?> GetByIdAsync(int postId);
        Task<Post?> GetByIdWithUserAsync(int postId);
        Task<IEnumerable<Post>> GetAllAsync();
        Task<IEnumerable<Post>> GetByUserIdAsync(int userId);
        Task<IEnumerable<Post>> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10);
        Task<IEnumerable<Post>> GetFollowingFeedAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<Post> CreateAsync(Post post);
        Task<Post> UpdateAsync(Post post);
        Task<bool> DeleteAsync(int postId);
        Task<int> GetPostCountByUserAsync(int userId);
        Task<int> GetLikeCountAsync(int postId);
        Task<int> GetCommentCountAsync(int postId);
        Task<bool> IsLikedByUserAsync(int postId, int userId);
        Task<bool> ToggleLikeAsync(int postId, int userId);
    }
}
