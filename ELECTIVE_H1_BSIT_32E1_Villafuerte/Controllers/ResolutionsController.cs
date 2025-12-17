using Microsoft.AspNetCore.Mvc;
using ResolutionsApi.Models;
using ResolutionsApi.Services;

namespace ResolutionsApi.Controllers;

[ApiController]
[Route("api/resolutions")]
public class ResolutionsController : ControllerBase
{
    [HttpGet]
    public IActionResult GetAll([FromQuery] string? isDone, [FromQuery] string? title)
    {
        bool? doneFilter = null;

        if (isDone != null)
        {
            if (!bool.TryParse(isDone, out bool parsed))
            {
                return Error("BadRequest", "Validation failed.", "isDone must be true or false");
            }
            doneFilter = parsed;
        }

        var items = ResolutionStore.Items.AsEnumerable();

        if (doneFilter.HasValue)
            items = items.Where(r => r.IsDone == doneFilter.Value);

        if (!string.IsNullOrWhiteSpace(title))
            items = items.Where(r => r.Title.Contains(title, StringComparison.OrdinalIgnoreCase));

        return Ok(new { items });
    }

    [HttpGet("{id}")]
    public IActionResult GetById(int id)
    {
        if (id <= 0)
            return Error("BadRequest", "Validation failed.", "id must be greater than zero");

        var item = ResolutionStore.Items.FirstOrDefault(r => r.Id == id);
        if (item == null)
            return NotFound(new { error = "NotFound", message = "Resolution not found.", details = new[] { $"id: {id}" } });

        return Ok(item);
    }

    [HttpPost]
    public IActionResult Create([FromBody] CreateResolutionDto dto)
    {
        if (dto == null || string.IsNullOrWhiteSpace(dto.Title))
            return Error("BadRequest", "Validation failed.", "title is required");

        var res = new Resolution
        {
            Id = ResolutionStore.NextId++,
            Title = dto.Title.Trim(),
            IsDone = false,
            CreatedAt = DateTime.UtcNow
        };

        ResolutionStore.Items.Add(res);
        return CreatedAtAction(nameof(GetById), new { id = res.Id }, res);
    }

    [HttpPut("{id}")]
    public IActionResult Update(int id, [FromBody] UpdateResolutionDto dto)
    {
        if (id <= 0 || dto?.Id == null)
            return Error("BadRequest", "Validation failed.", "id is required");

        if (id != dto.Id)
        {
            return BadRequest(new
            {
                error = "BadRequest",
                message = "Route id does not match body id.",
                details = new[] { $"route id: {id}", $"body id: {dto.Id}" }
            });
        }

        if (string.IsNullOrWhiteSpace(dto.Title))
            return Error("BadRequest", "Validation failed.", "title is required");

        var item = ResolutionStore.Items.FirstOrDefault(r => r.Id == id);
        if (item == null)
            return NotFound(new { error = "NotFound", message = "Resolution not found.", details = new[] { $"id: {id}" } });

        item.Title = dto.Title.Trim();
        item.IsDone = dto.IsDone;
        item.UpdatedAt = DateTime.UtcNow;

        return Ok(item);
    }

    [HttpDelete("{id}")]
    public IActionResult Delete(int id)
    {
        if (id <= 0)
            return Error("BadRequest", "Validation failed.", "id must be greater than zero");

        var item = ResolutionStore.Items.FirstOrDefault(r => r.Id == id);
        if (item == null)
            return NotFound(new { error = "NotFound", message = "Resolution not found.", details = new[] { $"id: {id}" } });

        ResolutionStore.Items.Remove(item);
        return NoContent();
    }

    private IActionResult Error(string error, string message, params string[] details)
    {
        return BadRequest(new { error, message, details });
    }
}
