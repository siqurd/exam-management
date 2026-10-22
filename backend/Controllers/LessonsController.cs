using Exam.Api.Data;
using Exam.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Exam.Api.Controllers;

[ApiController]
[Route("api/lessons")]
public sealed class LessonsController(ExamDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Lesson>>> GetAll(CancellationToken cancellationToken)
    {
        return await db.Lessons.AsNoTracking().OrderBy(x => x.Code).ToListAsync(cancellationToken);
    }

    [HttpGet("{code}")]
    public async Task<ActionResult<Lesson>> GetByCode(string code, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Code == code, cancellationToken);

        return lesson is null ? NotFound() : Ok(lesson);
    }

    [HttpPost]
    public async Task<ActionResult<Lesson>> Create(Lesson lesson, CancellationToken cancellationToken)
    {
        if (await db.Lessons.AnyAsync(x => x.Code == lesson.Code, cancellationToken))
            return Conflict(new ProblemDetails { Title = "Bu kodlu dərs artıq mövcuddur." });

        db.Lessons.Add(lesson);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new ProblemDetails { Title = "Bu kodlu dərs artıq mövcuddur." });
        }

        return CreatedAtAction(nameof(GetByCode), new { code = lesson.Code }, lesson);
    }

    [HttpPut("{code}")]
    public async Task<IActionResult> Update(string code, Lesson lesson, CancellationToken cancellationToken)
    {
        if (!string.Equals(code, lesson.Code, StringComparison.OrdinalIgnoreCase))
            return BadRequest(new ProblemDetails { Title = "Ünvandakı kod ilə dərsin kodu eyni olmalıdır." });

        var existing = await db.Lessons.SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (existing is null)
            return NotFound();

        existing.Name = lesson.Name;
        existing.Class = lesson.Class;
        existing.TeacherFirstName = lesson.TeacherFirstName;
        existing.TeacherLastName = lesson.TeacherLastName;

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }

        return NoContent();
    }

    [HttpDelete("{code}")]
    public async Task<IActionResult> Delete(string code, CancellationToken cancellationToken)
    {
        var lesson = await db.Lessons.SingleOrDefaultAsync(x => x.Code == code, cancellationToken);
        if (lesson is null)
            return NotFound();

        if (await db.Exams.AnyAsync(x => x.LessonCode == code, cancellationToken))
            return Conflict(new ProblemDetails { Title = "İmtahan nəticəsi olan dərs silinə bilməz." });

        db.Lessons.Remove(lesson);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 547 })
        {
            return Conflict(new ProblemDetails { Title = "İmtahan nəticəsi olan dərs silinə bilməz." });
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }

        return NoContent();
    }
}
