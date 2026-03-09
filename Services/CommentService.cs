using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

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

        public CommentService(ICommentRepository commentRepository)
        {
            _commentRepository = commentRepository;
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

        public async Task<CommentViewModel?> CreateCommentAsync(int postId, int userId, string content)
        {
            var comment = new Comment
            {
                PostId = postId,
                UserId = userId,
                Content = content,
                CreatedAt = DateTime.Now
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

        public async Task<bool> DeleteCommentAsync(int commentId, int userId)
        {
            var comment = await _commentRepository.GetByIdAsync(commentId);
            if (comment == null || comment.UserId != userId)
                return false;

            return await _commentRepository.DeleteAsync(commentId);
        }
    }
}
