using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;
using MiniSocialNetwork.BusinessRules;

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
        private readonly IRateLimiter _rateLimiter;

        public PostService(
            IPostRepository postRepository,
            IWebHostEnvironment webHostEnvironment,
            IRateLimiter rateLimiter)
        {
            _postRepository = postRepository;
            _webHostEnvironment = webHostEnvironment;
            _rateLimiter = rateLimiter;
        }

        public async Task<PostViewModel?> GetPostAsync(int postId, int? currentUserId = null)
        {
            var post = await _postRepository.GetByIdWithUserAsync(postId);
            if (post == null) return null;
            return await MapToViewModelAsync(post, currentUserId);
        }

        public async Task<IEnumerable<PostViewModel>> GetAllPostsAsync(int? currentUserId = null)
        {
            var posts = await _postRepository.GetAllAsync();
            var viewModels = new List<PostViewModel>();
            foreach (var post in posts)
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetUserPostsAsync(int userId, int? currentUserId = null)
        {
            var posts = await _postRepository.GetByUserIdAsync(userId);
            var viewModels = new List<PostViewModel>();
            foreach (var post in posts)
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10, int? currentUserId = null)
        {
            var posts = await _postRepository.GetHomeFeedAsync(pageNumber, pageSize);
            var viewModels = new List<PostViewModel>();
            foreach (var post in posts)
                viewModels.Add(await MapToViewModelAsync(post, currentUserId));
            return viewModels;
        }

        public async Task<IEnumerable<PostViewModel>> GetFollowingFeedAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            var posts = await _postRepository.GetFollowingFeedAsync(userId, pageNumber, pageSize);
            var viewModels = new List<PostViewModel>();
            foreach (var post in posts)
                viewModels.Add(await MapToViewModelAsync(post, userId));
            return viewModels;
        }

        /// <summary>
        /// Creates a post after applying:
        /// BR-PST-001 (content validation), BR-PST-005 (rate limit)
        /// </summary>
        public async Task<Post> CreatePostAsync(PostViewModel model, int userId)
        {
            // ── BR-PST-005: Rate limit ───────────────────────────────────────────
            RateLimitPolicies.EnforcePostRateLimit(_rateLimiter, userId);

            // ── BR-PST-001: Content validation ──────────────────────────────────
            PostValidator.ValidateContent(model.Content);

            var post = new Post
            {
                UserId = userId,
                Content = model.Content.Trim(),   // BR-SEC-003: trim whitespace
                ImagePath = model.ImagePath,
                CreatedAt = DateTime.UtcNow
            };

            return await _postRepository.CreateAsync(post);
        }

        /// <summary>
        /// Updates a post after applying:
        /// BR-PST-001 (content), BR-PST-003 (ownership), BR-PST-004 (UpdatedAt)
        /// </summary>
        public async Task<bool> UpdatePostAsync(PostViewModel model, int userId)
        {
            var post = await _postRepository.GetByIdAsync(model.PostId);
            if (post == null) return false;

            // ── BR-PST-003: Ownership check ──────────────────────────────────────
            PostValidator.EnsureOwnership(post.UserId, userId);

            // ── BR-PST-001: Content validation ──────────────────────────────────
            PostValidator.ValidateContent(model.Content);

            post.Content = model.Content.Trim();    // BR-SEC-003: trim
            if (!string.IsNullOrEmpty(model.ImagePath))
                post.ImagePath = model.ImagePath;

            // ── BR-PST-004: Record edit timestamp ───────────────────────────────
            post.UpdatedAt = DateTime.UtcNow;

            await _postRepository.UpdateAsync(post);
            return true;
        }

        /// <summary>
        /// Deletes a post after applying:
        /// BR-PST-003 (ownership)
        /// </summary>
        public async Task<bool> DeletePostAsync(int postId, int userId)
        {
            var post = await _postRepository.GetByIdAsync(postId);
            if (post == null) return false;

            // ── BR-PST-003: Ownership check ──────────────────────────────────────
            PostValidator.EnsureOwnership(post.UserId, userId);

            if (!string.IsNullOrEmpty(post.ImagePath))
            {
                var imagePath = Path.Combine(_webHostEnvironment.WebRootPath, post.ImagePath.TrimStart('/'));
                if (File.Exists(imagePath)) File.Delete(imagePath);
            }

            return await _postRepository.DeleteAsync(postId);
        }

        /// <summary>
        /// Uploads a post image after applying:
        /// BR-PST-002, BR-SEC-006 (file type and size validation)
        /// </summary>
        public async Task<string?> UploadPostImageAsync(IFormFile file, int userId)
        {
            if (file == null || file.Length == 0) return null;

            // ── BR-PST-002 / BR-SEC-006: Validate file ──────────────────────────
            PostValidator.ValidateImage(file);

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "posts");
            Directory.CreateDirectory(uploadsFolder);

            // ── BR-SEC-006: Use GUID filename to prevent path traversal ──────────
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            var fileName = $"{userId}_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid():N}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
                await file.CopyToAsync(stream);

            return $"/uploads/posts/{fileName}";
        }

        /// <summary>
        /// Toggles a like after applying:
        /// BR-INT-001 (no self-like), BR-INT-002 (toggle behavior)
        /// </summary>
        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            // ── BR-INT-001: Cannot like own post ─────────────────────────────────
            var post = await _postRepository.GetByIdAsync(postId);
            if (post != null)
                LikeValidator.EnsureNotSelfLike(post.UserId, userId);

            // ── BR-INT-002: Toggle like (handled in repository) ──────────────────
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
