using Exam.Api.Data;
using Exam.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Exam.Api.Controllers;

[ApiController]
[Route("api/students")]
public sealed class StudentsController(ExamDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<Student>>> GetAll(CancellationToken cancellationToken)
    {
        return await db.Students.AsNoTracking().OrderBy(x => x.Number).ToListAsync(cancellationToken);
    }

    [HttpGet("{number:int}")]
    public async Task<ActionResult<Student>> GetByNumber(int number, CancellationToken cancellationToken)
    {
        var student = await db.Students.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Number == number, cancellationToken);

        return student is null ? NotFound() : Ok(student);
    }

    [HttpPost]
    public async Task<ActionResult<Student>> Create(Student student, CancellationToken cancellationToken)
    {
        if (await db.Students.AnyAsync(x => x.Number == student.Number, cancellationToken))
            return Conflict(new ProblemDetails { Title = "Bu nömrəli şagird artıq mövcuddur." });

        db.Students.Add(student);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new ProblemDetails { Title = "Bu nömrəli şagird artıq mövcuddur." });
        }

        return CreatedAtAction(nameof(GetByNumber), new { number = student.Number }, student);
    }

    [HttpPut("{number:int}")]
    public async Task<IActionResult> Update(int number, Student student, CancellationToken cancellationToken)
    {
        if (number != student.Number)
            return BadRequest(new ProblemDetails { Title = "Ünvandakı nömrə ilə şagirdin nömrəsi eyni olmalıdır." });

        var existing = await db.Students.SingleOrDefaultAsync(x => x.Number == number, cancellationToken);
        if (existing is null)
            return NotFound();

        existing.FirstName = student.FirstName;
        existing.LastName = student.LastName;
        existing.Class = student.Class;

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

    [HttpDelete("{number:int}")]
    public async Task<IActionResult> Delete(int number, CancellationToken cancellationToken)
    {
        var student = await db.Students.SingleOrDefaultAsync(x => x.Number == number, cancellationToken);
        if (student is null)
            return NotFound();

        if (await db.Exams.AnyAsync(x => x.StudentNumber == number, cancellationToken))
            return Conflict(new ProblemDetails { Title = "İmtahan nəticəsi olan şagird silinə bilməz." });

        db.Students.Remove(student);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 547 })
        {
            return Conflict(new ProblemDetails { Title = "İmtahan nəticəsi olan şagird silinə bilməz." });
        }
        catch (DbUpdateConcurrencyException)
        {
            return NotFound();
        }

        return NoContent();
    }
}
