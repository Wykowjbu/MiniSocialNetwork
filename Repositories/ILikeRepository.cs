using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public interface ILikeRepository
    {
        Task<IEnumerable<User>> GetUsersWhoLikedPostAsync(int postId);
        Task<int> GetLikeCountAsync(int postId);
        Task<bool> HasUserLikedPostAsync(int postId, int userId);
    }
}
