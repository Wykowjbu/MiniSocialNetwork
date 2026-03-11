using System.Text.RegularExpressions;
using MiniSocialNetwork.ViewModels;

namespace MiniSocialNetwork.BusinessRules
{
    /// <summary>
    /// Validates all User-domain business rules.
    /// BR-USR-001, BR-USR-002, BR-USR-003, BR-USR-005, BR-USR-006
    /// </summary>
    public static class UserValidator
    {
        private static readonly Regex UsernameRegex =
            new(@"^[a-zA-Z0-9_]+$", RegexOptions.Compiled);

        private static readonly Regex PasswordComplexityRegex =
            new(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).+$", RegexOptions.Compiled);

        private static readonly HashSet<string> AllowedAvatarExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp" };

        private const long MaxAvatarSizeBytes = 5 * 1024 * 1024; // 5 MB
        private const int MinAge = 13;

        // ── BR-USR-001 ──────────────────────────────────────────────────────────
        /// <summary>Validates username format (3–30 chars, alphanumeric + underscore).</summary>
        public static void ValidateUsernameFormat(string username)
        {
            if (string.IsNullOrWhiteSpace(username))
                throw new ValidationException("BR-USR-001", "Username is required.");

            if (username.Length < 3 || username.Length > 30)
                throw new ValidationException("BR-USR-001",
                    "Username must be 3–30 characters long.");

            if (!UsernameRegex.IsMatch(username))
                throw new ValidationException("BR-USR-001",
                    "Username must contain only letters, numbers, and underscores.");
        }

        // ── BR-USR-002 ──────────────────────────────────────────────────────────
        /// <summary>Validates email format and length.</summary>
        public static void ValidateEmailFormat(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ValidationException("BR-USR-002", "Email is required.");

            if (email.Length > 100)
                throw new ValidationException("BR-USR-002",
                    "Email cannot exceed 100 characters.");

            // Basic RFC 5322 check — MailAddress throws on invalid format
            try { _ = new System.Net.Mail.MailAddress(email); }
            catch
            {
                throw new ValidationException("BR-USR-002", "Invalid email format.");
            }
        }

        // ── BR-USR-003 ──────────────────────────────────────────────────────────
        /// <summary>Validates password complexity (8–128 chars, upper + lower + digit).</summary>
        public static void ValidatePasswordComplexity(string password, string username)
        {
            if (string.IsNullOrWhiteSpace(password))
                throw new ValidationException("BR-USR-003", "Password is required.");

            if (password.Length < 8 || password.Length > 128)
                throw new ValidationException("BR-USR-003",
                    "Password must be 8–128 characters long.");

            if (!PasswordComplexityRegex.IsMatch(password))
                throw new ValidationException("BR-USR-003",
                    "Password must include at least one uppercase letter, one lowercase letter, and one digit.");

            if (password.Equals(username, StringComparison.OrdinalIgnoreCase))
                throw new ValidationException("BR-USR-003",
                    "Password must not be the same as your username.");
        }

        // ── BR-USR-005 ──────────────────────────────────────────────────────────
        /// <summary>Validates profile fields: Bio, FullName, DateOfBirth.</summary>
        public static void ValidateProfileFields(string? fullName, string? bio, DateOnly? dateOfBirth)
        {
            if (fullName is not null && fullName.Length > 100)
                throw new ValidationException("BR-USR-005",
                    "Full name cannot exceed 100 characters.");

            if (bio is not null && bio.Length > 300)
                throw new ValidationException("BR-USR-005",
                    "Bio cannot exceed 300 characters.");

            if (dateOfBirth.HasValue)
            {
                var today = DateOnly.FromDateTime(DateTime.Today);
                var minBirthDate = today.AddYears(-MinAge);
                if (dateOfBirth.Value > minBirthDate)
                    throw new ValidationException("BR-USR-005",
                        $"You must be at least {MinAge} years old to register.");
            }
        }

        /// <summary>Validates avatar file: type and size.</summary>
        public static void ValidateAvatarFile(IFormFile? avatar)
        {
            if (avatar == null || avatar.Length == 0) return; // avatar is optional

            var extension = Path.GetExtension(avatar.FileName);
            if (!AllowedAvatarExtensions.Contains(extension))
                throw new ValidationException("BR-USR-005",
                    "Avatar must be an image file (jpg, jpeg, png, gif, webp).");

            if (avatar.Length > MaxAvatarSizeBytes)
                throw new ValidationException("BR-USR-005",
                    "Avatar file size cannot exceed 5MB.");
        }

        // ── BR-USR-006 ──────────────────────────────────────────────────────────
        /// <summary>Ensures the requesting user owns the profile they are modifying.</summary>
        public static void EnsureProfileOwnership(int requestingUserId, int targetUserId)
        {
            if (requestingUserId != targetUserId)
                throw new ProfileOwnershipException();
        }

        // ── Convenience: full register validation ───────────────────────────────
        /// <summary>Runs all format checks for a RegisterViewModel before hitting the database.</summary>
        public static void ValidateRegistration(RegisterViewModel model)
        {
            ValidateUsernameFormat(model.Username.Trim());
            ValidateEmailFormat(model.Email.Trim());
            ValidatePasswordComplexity(model.Password, model.Username);
            ValidateProfileFields(model.FullName, bio: null, dateOfBirth: null);
        }
    }
}
