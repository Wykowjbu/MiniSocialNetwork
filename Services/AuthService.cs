using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;
using BCrypt.Net;

namespace MiniSocialNetwork.Services
{
    public interface IAuthService
    {
        Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model);
        Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordViewModel model);
        string HashPassword(string password);
        bool VerifyPassword(string password, string passwordHash);
    }

    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;

        public AuthService(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        public async Task<(bool Success, string Message, User? User)> RegisterAsync(RegisterViewModel model)
        {
            // Check if username already exists
            if (await _userRepository.UsernameExistsAsync(model.Username))
            {
                return (false, "Username already exists", null);
            }

            // Check if email already exists
            if (await _userRepository.EmailExistsAsync(model.Email))
            {
                return (false, "Email already exists", null);
            }

            // Create new user
            var user = new User
            {
                Username = model.Username,
                Email = model.Email,
                Password = HashPassword(model.Password),
                FullName = model.FullName,
                CreatedAt = DateTime.Now,
                IsActive = true
            };

            var createdUser = await _userRepository.CreateAsync(user);
            return (true, "Registration successful", createdUser);
        }

        public async Task<(bool Success, string Message, User? User)> LoginAsync(LoginViewModel model)
        {
            // Find user by username or email
            var user = await _userRepository.GetByUsernameOrEmailAsync(model.Username);

            if (user == null)
            {
                return (false, "Invalid username or password", null);
            }

            if (user.IsActive != true)
            {
                return (false, "Account is deactivated", null);
            }

            // Verify password
            if (!VerifyPassword(model.Password, user.Password))
            {
                return (false, "Invalid username or password", null);
            }

            return (true, "Login successful", user);
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
            {
                return false;
            }

            // Verify current password
            if (!VerifyPassword(model.CurrentPassword, user.Password))
            {
                return false;
            }

            // Update password
            user.Password = HashPassword(model.NewPassword);
            await _userRepository.UpdateAsync(user);
            return true;
        }

        public string HashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password, BCrypt.Net.BCrypt.GenerateSalt(12));
        }

        public bool VerifyPassword(string password, string passwordHash)
        {
            try
            {
                return BCrypt.Net.BCrypt.Verify(password, passwordHash);
            }
            catch
            {
                return false;
            }
        }
    }
}
