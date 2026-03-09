using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public interface IFollowRepository
    {
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<bool> FollowAsync(int followerId, int followingId);
        Task<bool> UnfollowAsync(int followerId, int followingId);
        Task<IEnumerable<User>> GetFollowersAsync(int userId);
        Task<IEnumerable<User>> GetFollowingAsync(int userId);
        Task<int> GetFollowerCountAsync(int userId);
        Task<int> GetFollowingCountAsync(int userId);
    }
}
