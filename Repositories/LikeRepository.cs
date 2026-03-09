using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public class LikeRepository : ILikeRepository
    {
        private readonly MiniSocialNetworkContext _context;

        public LikeRepository(MiniSocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<User>> GetUsersWhoLikedPostAsync(int postId)
        {
            return await _context.Likes
                .Where(l => l.PostId == postId)
                .Include(l => l.User)
                .OrderByDescending(l => l.CreatedAt)
                .Select(l => l.User)
                .ToListAsync();
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.Likes.CountAsync(l => l.PostId == postId);
        }

        public async Task<bool> HasUserLikedPostAsync(int postId, int userId)
        {
            return await _context.Likes.AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }
    }
}
