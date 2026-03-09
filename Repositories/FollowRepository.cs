using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public class FollowRepository : IFollowRepository
    {
        private readonly MiniSocialNetworkContext _context;

        public FollowRepository(MiniSocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<bool> IsFollowingAsync(int followerId, int followingId)
        {
            return await _context.Follows
                .AnyAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);
        }

        public async Task<bool> FollowAsync(int followerId, int followingId)
        {
            // Can't follow yourself
            if (followerId == followingId)
                return false;

            // Check if already following
            if (await IsFollowingAsync(followerId, followingId))
                return false;

            var follow = new Follow
            {
                FollowerId = followerId,
                FollowingId = followingId,
                CreatedAt = DateTime.Now
            };

            _context.Follows.Add(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> UnfollowAsync(int followerId, int followingId)
        {
            var follow = await _context.Follows
                .FirstOrDefaultAsync(f => f.FollowerId == followerId && f.FollowingId == followingId);

            if (follow == null)
                return false;

            _context.Follows.Remove(follow);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<User>> GetFollowersAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowingId == userId)
                .Include(f => f.Follower)
                .Select(f => f.Follower)
                .ToListAsync();
        }

        public async Task<IEnumerable<User>> GetFollowingAsync(int userId)
        {
            return await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Include(f => f.Following)
                .Select(f => f.Following)
                .ToListAsync();
        }

        public async Task<int> GetFollowerCountAsync(int userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowingId == userId);
        }

        public async Task<int> GetFollowingCountAsync(int userId)
        {
            return await _context.Follows.CountAsync(f => f.FollowerId == userId);
        }
    }
}
