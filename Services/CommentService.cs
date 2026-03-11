using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;
using MiniSocialNetwork.BusinessRules;

namespace MiniSocialNetwork.Services
{
    public interface ICommentService
    {
        Task<IEnumerable<CommentViewModel>> GetCommentsByPostAsync(int postId, int? currentUserId = null);
        Task<CommentViewModel?> CreateCommentAsync(int postId, int userId, string content);
        Task<bool> DeleteCommentAsync(int commentId, int userId);
    }

    public class CommentService : ICommentService
    {
        private readonly ICommentRepository _commentRepository;
        private readonly IRateLimiter _rateLimiter;

        public CommentService(ICommentRepository commentRepository, IRateLimiter rateLimiter)
        {
            _commentRepository = commentRepository;
            _rateLimiter = rateLimiter;
        }

        public async Task<IEnumerable<CommentViewModel>> GetCommentsByPostAsync(int postId, int? currentUserId = null)
        {
            var comments = await _commentRepository.GetByPostIdAsync(postId);
            return comments.Select(c => new CommentViewModel
            {
                CommentId = c.CommentId,
                PostId = c.PostId,
                UserId = c.UserId,
                Content = c.Content,
                CreatedAt = c.CreatedAt,
                Username = c.User?.Username ?? "Unknown",
                AvatarPath = c.User?.AvatarPath,
                IsOwnedByCurrentUser = currentUserId.HasValue && c.UserId == currentUserId.Value
            });
        }

        /// <summary>
        /// Creates a comment after applying:
        /// BR-INT-003 (content validation), BR-INT-005 (rate limit)
        /// </summary>
        public async Task<CommentViewModel?> CreateCommentAsync(int postId, int userId, string content)
        {
            // ── BR-INT-005: Rate limit ───────────────────────────────────────────
            RateLimitPolicies.EnforceCommentRateLimit(_rateLimiter, userId);

            // ── BR-INT-003: Content validation ───────────────────────────────────
            CommentValidator.ValidateContent(content);

            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content.Trim(),   // BR-SEC-003: trim whitespace
                CreatedAt = DateTime.UtcNow
            };

            var created = await _commentRepository.CreateAsync(comment);
            var fullComment = await _commentRepository.GetByIdAsync(created.CommentId);
            if (fullComment == null) return null;

            return new CommentViewModel
            {
                CommentId = fullComment.CommentId,
                PostId = fullComment.PostId,
                UserId = fullComment.UserId,
                Content = fullComment.Content,
                CreatedAt = fullComment.CreatedAt,
                Username = fullComment.User?.Username ?? "Unknown",
                AvatarPath = fullComment.User?.AvatarPath,
                IsOwnedByCurrentUser = true
            };
        }

        /// <summary>
        /// Deletes a comment after applying:
        /// BR-INT-004 (ownership check)
        /// </summary>
        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null) return false;

            // ── BR-INT-004: Ownership check ──────────────────────────────────────
            CommentValidator.EnsureOwnership(comment.UserId, userId);

            return await _commentRepository.DeleteAsync(commentId);
        }
    }
}
