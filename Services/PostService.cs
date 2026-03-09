using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

namespace MiniSocialNetwork.Services
{
    public interface IPostService
    {
        Task<PostViewModel?> GetPostAsync(int postId, int? currentUserId = null);
        Task<IEnumerable<PostViewModel>> GetAllPostsAsync(int? currentUserId = null);
        Task<IEnumerable<PostViewModel>> GetUserPostsAsync(int userId, int? currentUserId = null);
        Task<IEnumerable<PostViewModel>> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10, int? currentUserId = null);
        Task<IEnumerable<PostViewModel>> GetFollowingFeedAsync(int userId, int pageNumber = 1, int pageSize = 10);
        Task<Post> CreatePostAsync(PostViewModel model, int userId);
        Task<bool> UpdatePostAsync(PostViewModel model, int userId);
        Task<bool> DeletePostAsync(int postId, int userId);
        Task<string?> UploadPostImageAsync(IFormFile file, int userId);
        Task<bool> ToggleLikeAsync(int postId, int userId);
    }

    public class PostService : IPostService
    {
        private readonly IPostRepository _postRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public PostService(IPostRepository postRepository, IWebHostEnvironment webHostEnvironment)
        {
            _postRepository = postRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<PostViewModel?> GetPostAsync(int postId, int? currentUserId = null)
        {
            var post = await _postRepository.GetByIdWithUserAsync(postId);
            if (post == null)
                return null;

            return await MapToViewModelAsync(post, currentUserId);
        }

        public async Task<IEnumerable<PostViewModel>> GetAllPostsAsync(int? currentUserId = null)
        {
            var posts = await _postRepository.GetAllAsync();
            var viewModels = new List<PostViewModel>();

            foreach (var post in posts)
            {
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            }

            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetUserPostsAsync(int userId, int? currentUserId = null)
        {
            var posts = await _postRepository.GetByUserIdAsync(userId);
            var viewModels = new List<PostViewModel>();

            foreach (var post in posts)
            {
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            }

            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10, int? currentUserId = null)
        {
            var posts = await _postRepository.GetHomeFeedAsync(pageNumber, pageSize);
            var viewModels = new List<PostViewModel>();

            foreach (var post in posts)
            {
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            }

            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetFollowingFeedAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetFollowingFeedAsync(userId, pageNumber, pageSize);
            var viewModels = new List<PostViewModel>();

            foreach (var post in posts)
            {
                viewModels.Add(await MapToViewModelAsync(post, userId));
            }

            return viewModels;
        }

        public async Task<Post> CreatePostAsync(PostViewModel model, int userId)
        {
            var post = new Post
            {
                UserId = userId,
                Content = model.Content,
                ImagePath = model.ImagePath,
                CreatedAt = DateTime.Now
            };

            return await _postRepository.CreateAsync(post);
        }

        public async Task<bool> UpdatePostAsync(PostViewModel model, int userId)
        {
            var post = await _postRepository.GetByIdAsync(model.PostId);
            if (post == null || post.UserId != userId)
                return false;

            post.Content = model.Content;
            if (!string.IsNullOrEmpty(model.ImagePath))
            {
                post.ImagePath = model.ImagePath;
            }
            post.UpdatedAt = DateTime.Now;

            await _postRepository.UpdateAsync(post);
            return true;
        }

        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null || post.UserId != userId)
                return false;

            // Delete post image if exists
            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImagePath.TrimStart('/'));
                if (File.Exists(imagePath))
                {
                    File.Delete(imagePath);
                }
            }

            return await _postRepository.DeleteAsync(postId);
        }

        public async Task<string?> UploadPostImageAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return null;

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/posts/{fileName}";
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            return await _postRepository.ToggleLikeAsync(postId, userId);
        }

        private async Task<PostViewModel> MapToViewModelAsync(Post post, int? currentUserId)
        {
            return new PostViewModel
            {
                PostId = post.PostId,
                UserId = post.UserId,
                Content = post.Content,
                ImagePath = post.ImagePath,
                CreatedAt = post.CreatedAt,
                UpdatedAt = post.UpdatedAt,
                Username = post.User?.Username ?? "Unknown",
                AvatarPath = post.User?.AvatarPath,
                LikeCount = await _postRepository.GetLikeCountAsync(post.PostId),
                CommentCount = await _postRepository.GetCommentCountAsync(post.PostId),
                IsLikedByCurrentUser = currentUserId.HasValue && await _postRepository.IsLikedByUserAsync(post.PostId, currentUserId.Value),
                IsOwnedByCurrentUser = currentUserId.HasValue && post.UserId == currentUserId.Value
            };
        }
    }
}
