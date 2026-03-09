using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public class CommentRepository : ICommentRepository
    {
        private readonly MiniSocialNetworkContext _context;

        public CommentRepository(MiniSocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.CommentId == commentId);
        }

        public async Task<IEnumerable<Comment>> GetByPostIdAsync(int postId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .Where(c => c.PostId == postId)
                .OrderByDescending(c => c.CreatedAt)
                .ToListAsync();
        }

        public async Task<Comment> CreateAsync(Comment comment)
        {
            comment.CreatedAt = DateTime.Now;
            _context.Comments.Add(comment);
            await _context.SaveChangesAsync();
            return comment;
        }

        public async Task<bool> DeleteAsync(int commentId)
        {
            var comment = await _context.Comments.FindAsync(commentId);
            if (comment == null)
                return false;

            _context.Comments.Remove(comment);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetCommentCountByPostAsync(int postId)
        {
            return await _context.Comments.CountAsync(c => c.PostId == postId);
        }
    }
}
