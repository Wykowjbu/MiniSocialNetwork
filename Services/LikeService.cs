using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

namespace MiniSocialNetwork.Services
{
    public interface ILikeService
    {
        Task<IEnumerable<UserSearchViewModel>> GetUsersWhoLikedPostAsync(int postId, int? currentUserId = null);
    }

    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IPostRepository _postRepository;
        private readonly IFollowRepository _followRepository;

        public LikeService(
            ILikeRepository likeRepository,
            IPostRepository postRepository,
            IFollowRepository followRepository)
        {
            _likeRepository = likeRepository;
            _postRepository = postRepository;
            _followRepository = followRepository;
        }

        public async Task<IEnumerable<UserSearchViewModel>> GetUsersWhoLikedPostAsync(int postId, int? currentUserId = null)
        {
            var users = await _likeRepository.GetUsersWhoLikedPostAsync(postId);
            var results = new List<UserSearchViewModel>();

            foreach (var user in users)
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
