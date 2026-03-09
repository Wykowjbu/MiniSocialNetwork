using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using MiniSocialNetwork.Models;

namespace MiniSocialNetwork.Repositories
{
    public class PostRepository : IPostRepository
    {
        private readonly MiniSocialNetworkContext _context;

        public PostRepository(MiniSocialNetworkContext context)
        {
            _context = context;
        }

        public async Task<Post?> GetByIdAsync(int postId)
        {
            return await _context.Posts
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<Post?> GetByIdWithUserAsync(int postId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .FirstOrDefaultAsync(p => p.PostId == postId);
        }

        public async Task<IEnumerable<Post>> GetAllAsync()
        {
            return await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetByUserIdAsync(int userId)
        {
            return await _context.Posts
                .Include(p => p.User)
                .Where(p => p.UserId == userId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetHomeFeedAsync(int pageNumber = 1, int pageSize = 10)
        {
            return await _context.Posts
                .Include(p => p.User)
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<IEnumerable<Post>> GetFollowingFeedAsync(int userId, int pageNumber = 1, int pageSize = 10)
        {
            // Get posts from users that the current user is following, plus own posts
            var followingIds = await _context.Follows
                .Where(f => f.FollowerId == userId)
                .Select(f => f.FollowingId)
                .ToListAsync();

            // Include own posts
            followingIds.Add(userId);

            return await _context.Posts
                .Include(p => p.User)
                .Where(p => followingIds.Contains(p.UserId))
                .OrderByDescending(p => p.CreatedAt)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<Post> CreateAsync(Post post)
        {
            post.CreatedAt = DateTime.Now;
            _context.Posts.Add(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<Post> UpdateAsync(Post post)
        {
            post.UpdatedAt = DateTime.Now;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();
            return post;
        }

        public async Task<bool> DeleteAsync(int postId)
        {
            var post = await GetByIdAsync(postId);
            if (post == null)
                return false;

            _context.Posts.Remove(post);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<int> GetPostCountByUserAsync(int userId)
        {
            return await _context.Posts
                .CountAsync(p => p.UserId == userId);
        }

        public async Task<int> GetLikeCountAsync(int postId)
        {
            return await _context.Likes
                .CountAsync(l => l.PostId == postId);
        }

        public async Task<int> GetCommentCountAsync(int postId)
        {
            return await _context.Comments
                .CountAsync(c => c.PostId == postId);
        }

        public async Task<bool> IsLikedByUserAsync(int postId, int userId)
        {
            return await _context.Likes
                .AnyAsync(l => l.PostId == postId && l.UserId == userId);
        }

        public async Task<bool> ToggleLikeAsync(int postId, int userId)
        {
            var existingLike = await _context.Likes
                .FirstOrDefaultAsync(l => l.PostId == postId && l.UserId == userId);

            if (existingLike != null)
            {
                _context.Likes.Remove(existingLike);
                await _context.SaveChangesAsync();
                return false; // Unliked
            }
            else
            {
                var like = new Like
                {
                    PostId = postId,
                    UserId = userId,
                    CreatedAt = DateTime.Now
                };
                _context.Likes.Add(like);
                await _context.SaveChangesAsync();
                return true; // Liked
            }
        }
    }
}
