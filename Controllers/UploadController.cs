using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PRM393_Travel_Planner_BE.Services.Interfaces;

namespace PRM393_Travel_Planner_BE.Controllers;

[ApiController]
[Route("api/upload")]
[Authorize]
[Produces("application/json")]
public class UploadController(ICloudinaryService cloudinaryService) : ControllerBase
{
    /// <summary>
    /// Upload một ảnh lên Cloudinary. Trả về URL công khai của ảnh.
    /// </summary>
    /// <remarks>Content-Type: multipart/form-data, field name: "file"</remarks>
    [HttpPost("image")]
    [ProducesResponseType(typeof(UploadImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Vui lòng chọn file ảnh." });

        try
        {
            var url = await cloudinaryService.UploadImageAsync(file);
            return Ok(new UploadImageResponse(url));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}

/// <summary>Response trả về sau khi upload ảnh thành công.</summary>
public record UploadImageResponse(string Url);
