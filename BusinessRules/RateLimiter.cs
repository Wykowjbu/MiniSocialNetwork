using System.Collections.Concurrent;

namespace MiniSocialNetwork.BusinessRules
{
    /// <summary>
    /// In-memory rate limiter for anti-spam rules.
    /// BR-PST-005, BR-INT-005, BR-SEC-004
    ///
    /// NOTE: This is a lightweight, single-instance implementation suitable for
    /// development and single-server deployments. For production multi-server
    /// scenarios, replace with a distributed cache such as Redis.
    /// </summary>
    public interface IRateLimiter
    {
        /// <summary>Returns true if the action is allowed; false if the limit is exceeded.</summary>
        bool IsAllowed(string key, int maxRequests, TimeSpan window);
    }

    public class InMemoryRateLimiter : IRateLimiter
    {
        // key → list of timestamps within the window
        private readonly ConcurrentDictionary<string, Queue<DateTime>> _store = new();
        private readonly object _lock = new();

        public bool IsAllowed(string key, int maxRequests, TimeSpan window)
        {
            lock (_lock)
            {
                var now = DateTime.UtcNow;
                var queue = _store.GetOrAdd(key, _ => new Queue<DateTime>());

                // Remove timestamps outside the window
                while (queue.Count > 0 && (now - queue.Peek()) > window)
                    queue.Dequeue();

                if (queue.Count >= maxRequests)
                    return false;

                queue.Enqueue(now);
                return true;
            }
        }
    }

    /// <summary>
    /// Facade for applying rate limit business rules with clear rule IDs.
    /// </summary>
    public static class RateLimitPolicies
    {
        // ── BR-PST-005 ── 10 posts / hour / user
        public static void EnforcePostRateLimit(IRateLimiter limiter, int userId)
        {
            if (!limiter.IsAllowed($"post:{userId}", maxRequests: 10, window: TimeSpan.FromHours(1)))
                throw new PostRateLimitException();
        }

        // ── BR-INT-005 ── 30 comments / hour / user
        public static void EnforceCommentRateLimit(IRateLimiter limiter, int userId)
        {
            if (!limiter.IsAllowed($"comment:{userId}", maxRequests: 30, window: TimeSpan.FromHours(1)))
                throw new CommentRateLimitException();
        }

        // ── BR-SEC-004 ── 5 failed login attempts / 15 min / IP
        public static void EnforceLoginRateLimit(IRateLimiter limiter, string ipAddress)
        {
            if (!limiter.IsAllowed($"login:{ipAddress}", maxRequests: 5, window: TimeSpan.FromMinutes(15)))
                throw new AuthRateLimitException("login attempts");
        }

        // ── BR-SEC-004 ── 3 registrations / hour / IP
        public static void EnforceRegisterRateLimit(IRateLimiter limiter, string ipAddress)
        {
            if (!limiter.IsAllowed($"register:{ipAddress}", maxRequests: 3, window: TimeSpan.FromHours(1)))
                throw new AuthRateLimitException("registration attempts");
        }
    }
}
