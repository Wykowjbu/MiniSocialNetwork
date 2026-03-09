using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

namespace MiniSocialNetwork.Services
{
    public interface IFollowService
    {
        Task<bool> FollowAsync(int followerId, int followingId);
        Task<bool> UnfollowAsync(int followerId, int followingId);
        Task<bool> ToggleFollowAsync(int followerId, int followingId);
        Task<bool> IsFollowingAsync(int followerId, int followingId);
        Task<IEnumerable<FollowViewModel>> GetFollowersAsync(int userId, int? currentUserId = null);
        Task<IEnumerable<FollowViewModel>> GetFollowingAsync(int userId, int? currentUserId = null);
    }

    public class FollowService : IFollowService
    {
        private readonly IFollowRepository _followRepository;
        private readonly IPostRepository _postRepository;

        public FollowService(IFollowRepository followRepository, IPostRepository postRepository)
        {
            _followRepository = followRepository;
            _postRepository = postRepository;
        }

        public async Task<bool> FollowAsync(int followerId, int followingId)
        {
            return await _followRepository.FollowAsync(followerId, followingId);
        }

        public async Task<bool> UnfollowAsync(int followerId, int followingId)
        {
            return await _followRepository.UnfollowAsync(followerId, followingId);
        }

        public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
        {
            if (await _followRepository.IsFollowingAsync(followerId, followingId))
            {
                await _followRepository.UnfollowAsync(followerId, followingId);
                return false; // Now not following
            }
            else
            {
                await _followRepository.FollowAsync(followerId, followingId);
                return true; // Now following
            }
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _followRepository.IsFollowingAsync(followerId, followingId);
        }

        public async Task<IEnumerable<FollowViewModel>> GetFollowersAsync(int userId, int? currentUserId = null)
        {
            var followers = await _followRepository.GetFollowersAsync(userId);
            var result = new List<FollowViewModel>();

            foreach (var user in followers)
            {
                result.Add(new FollowViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    AvatarPath = user.AvatarPath,
                    Bio = user.Bio,
                    IsFollowedByCurrentUser = currentUserId.HasValue && 
                        await _followRepository.IsFollowingAsync(currentUserId.Value, user.UserId)
                });
            }

            return result;
        }

        public async Task<IEnumerable<FollowViewModel>> GetFollowingAsync(int userId, int? currentUserId = null)
        {
            var following = await _followRepository.GetFollowingAsync(userId);
            var result = new List<FollowViewModel>();

            foreach (var user in following)
            {
                result.Add(new FollowViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    AvatarPath = user.AvatarPath,
                    Bio = user.Bio,
                    IsFollowedByCurrentUser = currentUserId.HasValue && 
                        await _followRepository.IsFollowingAsync(currentUserId.Value, user.UserId)
                });
            }

            return result;
        }
    }
}
