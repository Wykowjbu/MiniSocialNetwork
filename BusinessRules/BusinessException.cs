namespace MiniSocialNetwork.BusinessRules
{
    /// <summary>
    /// Base exception for all Business Rule violations.
    /// Maps to HTTP status codes for clean API/Razor Pages error handling.
    /// </summary>
    public class BusinessException : Exception
    {
        public string RuleId { get; }
        public int HttpStatusCode { get; }

        public BusinessException(string ruleId, string message, int httpStatusCode = 400)
            : base(message)
        {
            RuleId = ruleId;
            HttpStatusCode = httpStatusCode;
        }
    }

    // ════════════════════════════════════════
    // 400 Bad Request Exceptions
    // ════════════════════════════════════════

    /// <summary>BR-USR-001 / BR-USR-002 / BR-USR-003 / BR-USR-005</summary>
    public class ValidationException : BusinessException
    {
        public ValidationException(string ruleId, string message)
            : base(ruleId, message, 400) { }
    }

    /// <summary>BR-INT-001 — Cannot like own post</summary>
    public class SelfLikeException : BusinessException
    {
        public SelfLikeException()
            : base("BR-INT-001", "You cannot like your own post.", 400) { }
    }

    /// <summary>BR-CON-001 — Cannot follow yourself</summary>
    public class SelfFollowException : BusinessException
    {
        public SelfFollowException()
            : base("BR-CON-001", "You cannot follow yourself.", 400) { }
    }

    /// <summary>BR-CON-004 — Following limit exceeded</summary>
    public class FollowingLimitException : BusinessException
    {
        public FollowingLimitException()
            : base("BR-CON-004", "You have reached the maximum following limit (5000 users).", 400) { }
    }

    // ════════════════════════════════════════
    // 401 Unauthorized Exceptions
    // ════════════════════════════════════════

    /// <summary>BR-SEC-001 — Authentication required</summary>
    public class AuthenticationRequiredException : BusinessException
    {
        public AuthenticationRequiredException()
            : base("BR-SEC-001", "You must be logged in to perform this action.", 401) { }
    }

    // ════════════════════════════════════════
    // 403 Forbidden Exceptions
    // ════════════════════════════════════════

    /// <summary>BR-PST-003 / BR-INT-004 / BR-SEC-002 — Resource ownership violation</summary>
    public class ResourceOwnershipException : BusinessException
    {
        public ResourceOwnershipException(string resource = "resource")
            : base("BR-SEC-002", $"Access denied: you do not own this {resource}.", 403) { }
    }

    /// <summary>BR-USR-004 — Account deactivated</summary>
    public class AccountDeactivatedException : BusinessException
    {
        public AccountDeactivatedException()
            : base("BR-USR-004", "Your account has been deactivated. Please contact support.", 403) { }
    }

    /// <summary>BR-USR-006 — Profile ownership</summary>
    public class ProfileOwnershipException : BusinessException
    {
        public ProfileOwnershipException()
            : base("BR-USR-006", "You are not authorized to modify this profile.", 403) { }
    }

    // ════════════════════════════════════════
    // 404 Not Found Exceptions
    // ════════════════════════════════════════

    /// <summary>BR-CON-003 — Target user not found</summary>
    public class UserNotFoundException : BusinessException
    {
        public UserNotFoundException()
            : base("BR-CON-003", "User not found.", 404) { }
    }

    // ════════════════════════════════════════
    // 429 Too Many Requests Exceptions
    // ════════════════════════════════════════

    /// <summary>BR-PST-005 — Post rate limit</summary>
    public class PostRateLimitException : BusinessException
    {
        public PostRateLimitException()
            : base("BR-PST-005", "You have reached the posting limit (10 posts/hour). Please wait before posting again.", 429) { }
    }

    /// <summary>BR-INT-005 — Comment rate limit</summary>
    public class CommentRateLimitException : BusinessException
    {
        public CommentRateLimitException()
            : base("BR-INT-005", "You have reached the comment limit (30 comments/hour). Please wait before commenting again.", 429) { }
    }

    /// <summary>BR-SEC-004 — Login/Register rate limit</summary>
    public class AuthRateLimitException : BusinessException
    {
        public AuthRateLimitException(string action = "attempts")
            : base("BR-SEC-004", $"Too many failed {action}. Please try again in 15 minutes.", 429) { }
    }
}
