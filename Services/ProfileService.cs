using MiniSocialNetwork.Models;
using MiniSocialNetwork.ViewModels;
using MiniSocialNetwork.Repositories;

namespace MiniSocialNetwork.Services
{
    public interface IProfileService
    {
        Task<ProfileViewModel?> GetProfileAsync(int userId);
        Task<ProfileViewModel?> GetProfileByUsernameAsync(string username);
        Task<bool> UpdateProfileAsync(ProfileViewModel model);
        Task<string?> UploadAvatarAsync(int userId, IFormFile file);
    }

    public class ProfileService : IProfileService
    {
        private readonly IUserRepository _userRepository;
        private readonly IPostRepository _postRepository;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ProfileService(
            IUserRepository userRepository,
            IPostRepository postRepository,
            IWebHostEnvironment webHostEnvironment)
        {
            _userRepository = userRepository;
            _postRepository = postRepository;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<ProfileViewModel?> GetProfileAsync(int userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                return null;

            return await MapToProfileViewModelAsync(user);
        }

        public async Task<ProfileViewModel?> GetProfileByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            if (user == null)
                return null;

            return await MapToProfileViewModelAsync(user);
        }

        public async Task<bool> UpdateProfileAsync(ProfileViewModel model)
        {
            var user = await _userRepository.GetByIdAsync(model.UserId);
            if (user == null)
                return false;

            // Check if username is being changed and already exists
            if (user.Username != model.Username)
            {
                if (await _userRepository.UsernameExistsAsync(model.Username))
                    return false;
            }

            // Check if email is being changed and already exists
            if (user.Email != model.Email)
            {
                if (await _userRepository.EmailExistsAsync(model.Email))
                    return false;
            }

            user.Username = model.Username;
            user.Email = model.Email;
            user.FullName = model.FullName;
            user.Bio = model.Bio;
            user.DateOfBirth = model.DateOfBirth;

            if (!string.IsNullOrEmpty(model.AvatarPath))
            {
                user.AvatarPath = model.AvatarPath;
            }

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<string?> UploadAvatarAsync(int userId, IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return null;

            // Create uploads directory if it doesn't exist
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "avatars");
            if (!Directory.Exists(uploadsFolder))
            {
                Directory.CreateDirectory(uploadsFolder);
            }

            // Generate unique filename
            var fileName = $"{userId}_{DateTime.Now:yyyyMMddHHmmss}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Delete old avatar if exists
            var user = await _userRepository.GetByIdAsync(userId);
            if (user != null && !string.IsNullOrEmpty(user.AvatarPath))
            {
                var oldFilePath = Path.Combine(_webHostEnvironment.WebRootPath, user.AvatarPath.TrimStart('/'));
                if (File.Exists(oldFilePath))
                {
                    File.Delete(oldFilePath);
                }
            }

            // Save new avatar
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/avatars/{fileName}";
        }

        private async Task<ProfileViewModel> MapToProfileViewModelAsync(User user)
        {
            return new ProfileViewModel
            {
                UserId = user.UserId,
                Username = user.Username,
                Email = user.Email,
                FullName = user.FullName,
                Bio = user.Bio,
                DateOfBirth = user.DateOfBirth,
                AvatarPath = user.AvatarPath,
                CreatedAt = user.CreatedAt,
                PostCount = await _postRepository.GetPostCountByUserAsync(user.UserId),
                FollowerCount = await _userRepository.GetFollowerCountAsync(user.UserId),
                FollowingCount = await _userRepository.GetFollowingCountAsync(user.UserId)
            };
        }
    }
}
