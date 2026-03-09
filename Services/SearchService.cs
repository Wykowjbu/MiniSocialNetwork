using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

namespace MiniSocialNetwork.Services
{
    public interface ISearchService
    {
        Task<IEnumerable<UserSearchViewModel>> SearchUsersAsync(string query, int? currentUserId = null);
    }

    public class SearchService : ISearchService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IFollowRepository _followRepository;

        public SearchService(
            IUserRepository userRepository, 
            IPostRepository postRepository,
            IFollowRepository followRepository)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _followRepository = followRepository;
        }

        public async Task<IEnumerable<UserSearchViewModel>> SearchUsersAsync(string query, int? currentUserId = null)
        {
            if (string.IsNullOrWhiteSpace(query))
                return Enumerable.Empty<UserSearchViewModel>();

            var users = await _userRepository.GetAllAsync();
            var filteredUsers = users.Where(u => 
                u.Username.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                (u.FullName != null && u.FullName.Contains(query, StringComparison.OrdinalIgnoreCase)));

            var results = new List<UserSearchViewModel>();

            foreach (var user in filteredUsers)
            {
                results.Add(new UserSearchViewModel
                {
                    UserId = user.UserId,
                    Username = user.Username,
                    FullName = user.FullName,
                    AvatarPath = user.AvatarPath,
                    Bio = user.Bio,
                    PostCount = await _postRepository.GetPostCountByUserAsync(user.UserId),
                    FollowerCount = await _followRepository.GetFollowerCountAsync(user.UserId),
                    IsFollowedByCurrentUser = currentUserId.HasValue && 
                        currentUserId.Value != user.UserId &&
                        await _followRepository.IsFollowingAsync(currentUserId.Value, user.UserId)
                });
            }

            return results;
        }
    }
}
