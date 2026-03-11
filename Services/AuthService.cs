using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;
using MiniSocialNetwork.BusinessRules;

namespace MiniSocialNetwork.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model, string? ipAddress = null);
        Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model, string? ipAddress = null);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IRateLimiter _rateLimiter;

        public AuthService(IUserRepository userRepository, IRateLimiter rateLimiter)
        {
            _userRepository = userRepository;
            _rateLimiter = rateLimiter;
        }

        /// <summary>
        /// Registers a new user after applying:
        /// BR-USR-001, BR-USR-002, BR-USR-003, BR-SEC-004
        /// </summary>
        public async Task<(bool Success, string Message, User? User)> RegisterAsync(
            RegisterViewModel model, string? ipAddress = null)
        {
            try
            {
                // ── BR-SEC-004: Registration rate limit ─────────────────────────
                if (ipAddress is not null)
                    RateLimitPolicies.EnforceRegisterRateLimit(_rateLimiter, ipAddress);

                // ── BR-USR-001 / BR-USR-002 / BR-USR-003: Format validation ─────
                UserValidator.ValidateRegistration(model);

                // ── BR-USR-001: Unique username ──────────────────────────────────
                if (await _userRepository.UsernameExistsAsync(model.Username))
                    return (false, "Username already exists.", null);

                // ── BR-USR-002: Unique email ─────────────────────────────────────
                if (await _userRepository.EmailExistsAsync(model.Email))
                    return (false, "Email already registered.", null);

                // ── BR-SEC-005: Hash password before saving ──────────────────────
                var user = new User
                {
                    Username = model.Username.Trim(),
                    Email = model.Email.Trim().ToLowerInvariant(),
                    Password = HashPassword(model.Password),
                    FullName = model.FullName?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true
                };

                var createdUser = await _userRepository.CreateAsync(user);
                return (true, "Registration successful.", createdUser);
            }
            catch (BusinessException ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// Authenticates a user after applying:
        /// BR-USR-004, BR-SEC-004
        /// </summary>
        public async Task<(bool Success, string Message, User? User)> LoginAsync(
            LoginViewModel model, string? ipAddress = null)
        {
            try
            {
                // ── BR-SEC-004: Login rate limit ─────────────────────────────────
                if (ipAddress is not null)
                    RateLimitPolicies.EnforceLoginRateLimit(_rateLimiter, ipAddress);

                var user = await _userRepository.GetByUsernameOrEmailAsync(model.Username);

                if (user == null || !VerifyPassword(model.Password, user.Password))
                    return (false, "Invalid username or password.", null);

                // ── BR-USR-004: Must be active ───────────────────────────────────
                if (user.IsActive != true)
                    return (false, "Your account has been deactivated. Please contact support.", null);

                return (true, "Login successful.", user);
            }
            catch (BusinessException ex)
            {
                return (false, ex.Message, null);
            }
        }

        /// <summary>
        /// Changes a user's password after applying:
        /// BR-USR-003, BR-USR-006
        /// </summary>
        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return false;

            if (!VerifyPassword(model.CurrentPassword, user.Password))
                return false;

            // ── BR-USR-003: Validate new password complexity ─────────────────────
            try
            {
                UserValidator.ValidatePasswordComplexity(model.NewPassword, user.Username);
            }
            catch (BusinessException)
            {
                return false;
            }

            // ── BR-SEC-005: Hash before saving ───────────────────────────────────
            user.Password = HashPassword(model.NewPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public string HashPassword(string password) =>
            BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));

        public bool VerifyPassword(string password, string passwordHash)
        {
            try { return BCrypt.Net.BCrypt.Verify(password, passwordHash); }
            catch { return false; }
        }
    }
}
