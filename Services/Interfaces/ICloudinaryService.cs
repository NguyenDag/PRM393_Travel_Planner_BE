namespace PRM393_Travel_Planner_BE.Services.Interfaces;

public interface ICloudinaryService
{
    /// <summary>
    /// Upload một file ảnh lên Cloudinary và trả về URL công khai (secure_url).
    /// </summary>
    Task<string> UploadImageAsync(IFormFile file, string folder = "travel_planner");
}
