using FinitiGlossary.Application.DTOs.Request;
using FinitiGlossary.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/admin/[controller]")]
[Authorize]
public class AdminGlossaryController : ControllerBase
{
    private readonly IAdminGlossaryService _service;

    public AdminGlossaryController(IAdminGlossaryService service)
    {
        _service = service;
    }

    [HttpGet("all")]
    public async Task<IActionResult> GetAll(
      [FromQuery] int offset = 0,
      [FromQuery] int limit = 50,
      [FromQuery] string sort = "dateDesc",
      [FromQuery] string? search = null,
      [FromQuery] string tab = "all")
    {
        try
        {
            var result = await _service.GetAllForAdminAsync(User, offset, limit, sort, search, tab);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("create")]
    public async Task<IActionResult> Create(CreateGlossaryRequest request)
    {
        try
        {
            var result = await _service.CreateAsync(request, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("publish/{id:int}")]
    public async Task<IActionResult> Publish(int id)
    {
        try
        {
            var result = await _service.PublishAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPut("update/{id:int}")]
    public async Task<IActionResult> Update(int id, UpdateGlossaryRequest request)
    {
        try
        {
            var result = await _service.UpdateAsync(id, request, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("archive/{id:int}")]
    public async Task<IActionResult> Archive(int id)
    {
        try
        {
            var result = await _service.ArchiveAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpPost("restore/{stableId:guid}/{version:int}")]
    public async Task<IActionResult> Restore(Guid stableId, int version)
    {
        try
        {
            var result = await _service.RestoreAsync(stableId, version, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpGet("history/{stableId:guid}")]
    public async Task<IActionResult> History(Guid stableId)
    {
        try
        {
            var result = await _service.GetHistoryAsync(stableId, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }

    [HttpDelete("delete/{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var result = await _service.DeleteAsync(id, User);
            return Ok(result);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An unexpected server error occurred.", error = ex.Message });
        }
    }
}