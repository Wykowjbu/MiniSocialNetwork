using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;
using MiniSocialNetwork.BusinessRules;

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
        private readonly IUserRepository _userRepository;

        public FollowService(
            IFollowRepository followRepository,
            IPostRepository postRepository,
            IUserRepository userRepository)
        {
            _followRepository = followRepository;
            _postRepository = postRepository;
            _userRepository = userRepository;
        }

        /// <summary>
        /// Follows a user after applying:
        /// BR-CON-001, BR-CON-003, BR-CON-004
        /// </summary>
        public async Task<bool> FollowAsync(int followerId, int followingId)
        {
            // ── BR-CON-001: Cannot follow yourself ──────────────────────────────
            FollowValidator.EnsureNotSelfFollow(followerId, followingId);

            // ── BR-CON-003: Target user must exist and be active ─────────────────
            var targetUser = await _userRepository.GetByIdAsync(followingId);
            FollowValidator.EnsureTargetUserExists(targetUser is not null && targetUser.IsActive == true);

            // ── BR-CON-004: Following limit ──────────────────────────────────────
            var currentFollowingCount = await _followRepository.GetFollowingCountAsync(followerId);
            FollowValidator.EnsureFollowingLimitNotExceeded(currentFollowingCount);

            return await _followRepository.FollowAsync(followerId, followingId);
        }

        public async Task<bool> UnfollowAsync(int followerId, int followingId)
        {
            return await _followRepository.UnfollowAsync(followerId, followingId);
        }

        /// <summary>
        /// Toggles follow status after applying:
        /// BR-CON-001, BR-CON-002, BR-CON-003, BR-CON-004
        /// </summary>
        public async Task<bool> ToggleFollowAsync(int followerId, int followingId)
        {
            // ── BR-CON-001: Cannot follow yourself ──────────────────────────────
            FollowValidator.EnsureNotSelfFollow(followerId, followingId);

            // ── BR-CON-003: Target user must exist and be active ─────────────────
            var targetUser = await _userRepository.GetByIdAsync(followingId);
            FollowValidator.EnsureTargetUserExists(targetUser is not null && targetUser.IsActive == true);

            if (await _followRepository.IsFollowingAsync(followerId, followingId))
            {
                // ── BR-CON-002: Toggle — currently following → unfollow ──────────
                await _followRepository.UnfollowAsync(followerId, followingId);
                return false;
            }
            else
            {
                // ── BR-CON-004: Check limit only when trying to add a new follow ──
                var currentFollowingCount = await _followRepository.GetFollowingCountAsync(followerId);
                FollowValidator.EnsureFollowingLimitNotExceeded(currentFollowingCount);

                await _followRepository.FollowAsync(followerId, followingId);
                return true;
            }
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
            => await _followRepository.IsFollowingAsync(followerId, followingId);

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
