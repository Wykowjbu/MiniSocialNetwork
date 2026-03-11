namespace MiniSocialNetwork.BusinessRules
{
    /// <summary>
    /// Validates Post-domain business rules.
    /// BR-PST-001, BR-PST-002, BR-PST-003, BR-PST-005
    /// </summary>
    public static class PostValidator
    {
        private static readonly HashSet<string> AllowedImageExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif" };

        private const long MaxImageSizeBytes = 10 * 1024 * 1024; // 10 MB
        private const int MaxContentLength = 5000;

        // ── BR-PST-001 ──────────────────────────────────────────────────────────
        /// <summary>Validates that post content is non-empty and within 5000 chars.</summary>
        public static void ValidateContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ValidationException("BR-PST-001", "Post content is required.");

            if (content.Trim().Length > MaxContentLength)
                throw new ValidationException("BR-PST-001",
                    $"Post content cannot exceed {MaxContentLength} characters.");
        }

        // ── BR-PST-002 ──────────────────────────────────────────────────────────
        /// <summary>Validates attached image: allowed type and max size 10 MB.</summary>
        public static void ValidateImage(IFormFile? image)
        {
            if (image == null || image.Length == 0) return; // image is optional

            var extension = Path.GetExtension(image.FileName);
            if (!AllowedImageExtensions.Contains(extension))
                throw new ValidationException("BR-PST-002",
                    "Only image files (jpg, jpeg, png, gif) are allowed.");

            if (image.Length > MaxImageSizeBytes)
                throw new ValidationException("BR-PST-002",
                    "Image file size cannot exceed 10MB.");
        }

        // ── BR-PST-003 ──────────────────────────────────────────────────────────
        /// <summary>Ensures that only the post owner can modify or delete the post.</summary>
        public static void EnsureOwnership(int postOwnerId, int currentUserId)
        {
            if (postOwnerId != currentUserId)
                throw new ResourceOwnershipException("post");
        }
    }

    /// <summary>
    /// Validates Comment-domain business rules.
    /// BR-INT-003, BR-INT-004
    /// </summary>
    public static class CommentValidator
    {
        private const int MaxCommentLength = 1000;

        // ── BR-INT-003 ──────────────────────────────────────────────────────────
        /// <summary>Validates that comment content is non-empty and within 1000 chars.</summary>
        public static void ValidateContent(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                throw new ValidationException("BR-INT-003", "Comment content is required.");

            if (content.Trim().Length > MaxCommentLength)
                throw new ValidationException("BR-INT-003",
                    $"Comment cannot exceed {MaxCommentLength} characters.");
        }

        // ── BR-INT-004 ──────────────────────────────────────────────────────────
        /// <summary>Ensures that only the comment owner can delete the comment.</summary>
        public static void EnsureOwnership(int commentOwnerId, int currentUserId)
        {
            if (commentOwnerId != currentUserId)
                throw new ResourceOwnershipException("comment");
        }
    }

    /// <summary>
    /// Validates Interaction-domain business rules for Likes.
    /// BR-INT-001
    /// </summary>
    public static class LikeValidator
    {
        // ── BR-INT-001 ──────────────────────────────────────────────────────────
        /// <summary>Prevents a user from liking their own post.</summary>
        public static void EnsureNotSelfLike(int postOwnerId, int currentUserId)
        {
            if (postOwnerId == currentUserId)
                throw new SelfLikeException();
        }
    }

    /// <summary>
    /// Validates Connection-domain business rules for Follow.
    /// BR-CON-001, BR-CON-003, BR-CON-004
    /// </summary>
    public static class FollowValidator
    {
        private const int MaxFollowingCount = 5000;

        // ── BR-CON-001 ──────────────────────────────────────────────────────────
        /// <summary>Prevents a user from following themselves.</summary>
        public static void EnsureNotSelfFollow(int followerId, int followingId)
        {
            if (followerId == followingId)
                throw new SelfFollowException();
        }

        // ── BR-CON-003 ──────────────────────────────────────────────────────────
        /// <summary>Ensures the target user to follow exists and is active.</summary>
        public static void EnsureTargetUserExists(bool targetUserExists)
        {
            if (!targetUserExists)
                throw new UserNotFoundException();
        }

        // ── BR-CON-004 ──────────────────────────────────────────────────────────
        /// <summary>Enforces max following count of 5000.</summary>
        public static void EnsureFollowingLimitNotExceeded(int currentFollowingCount)
        {
            if (currentFollowingCount >= MaxFollowingCount)
                throw new FollowingLimitException();
        }
    }
}
