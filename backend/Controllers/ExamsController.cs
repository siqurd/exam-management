using Exam.Api.Data;
using Exam.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Exam.Api.Controllers;

[ApiController]
[Route("api/exams")]
public sealed class ExamsController(ExamDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<List<ExamResult>>> GetAll(CancellationToken cancellationToken)
    {
        return await db.Exams.AsNoTracking()
            .OrderByDescending(x => x.ExamDate).ThenBy(x => x.LessonCode).ThenBy(x => x.StudentNumber)
            .ToListAsync(cancellationToken);
    }

    [HttpGet("{lessonCode}/{studentNumber:int}/{examDate}")]
    public async Task<ActionResult<ExamResult>> GetByKey(
        string lessonCode, int studentNumber, DateOnly examDate, CancellationToken cancellationToken)
    {
        var exam = await db.Exams.AsNoTracking().SingleOrDefaultAsync(
            x => x.LessonCode == lessonCode && x.StudentNumber == studentNumber && x.ExamDate == examDate,
            cancellationToken);

        return exam is null ? NotFound() : Ok(exam);
    }

    [HttpPost]
    public async Task<ActionResult<ExamResult>> Create(ExamResult exam, CancellationToken cancellationToken)
    {
        if (!await db.Lessons.AnyAsync(x => x.Code == exam.LessonCode, cancellationToken))
            return BadRequest(new ProblemDetails { Title = "Dərs mövcud deyil." });

        if (!await db.Students.AnyAsync(x => x.Number == exam.StudentNumber, cancellationToken))
            return BadRequest(new ProblemDetails { Title = "Şagird mövcud deyil." });

        if (await db.Exams.AnyAsync(x => x.LessonCode == exam.LessonCode
                && x.StudentNumber == exam.StudentNumber && x.ExamDate == exam.ExamDate, cancellationToken))
            return Conflict(new ProblemDetails { Title = "Bu dərs, şagird və tarix üçün imtahan artıq mövcuddur." });

        db.Exams.Add(exam);

        try
        {
            await db.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 2601 or 2627 })
        {
            return Conflict(new ProblemDetails { Title = "Bu dərs, şagird və tarix üçün imtahan artıq mövcuddur." });
        }
        catch (DbUpdateException exception) when
            (exception.InnerException is SqlException { Number: 547 })
        {
            return Conflict(new ProblemDetails { Title = "Dərs və ya şagird artıq mövcud deyil." });
        }

        return CreatedAtAction(nameof(GetByKey), new
        {
            lessonCode = exam.LessonCode,
            studentNumber = exam.StudentNumber,
            examDate = exam.ExamDate.ToString("yyyy-MM-dd", System.Globalization.CultureInfo.InvariantCulture)
        }, exam);
    }

    [HttpPut("{lessonCode}/{studentNumber:int}/{examDate}")]
    public async Task<IActionResult> Update(string lessonCode, int studentNumber, DateOnly examDate,
        ExamResult exam, CancellationToken cancellationToken)
    {
        if (!string.Equals(lessonCode, exam.LessonCode, StringComparison.OrdinalIgnoreCase)
            || studentNumber != exam.StudentNumber || examDate != exam.ExamDate)
            return BadRequest(new ProblemDetails { Title = "Ünvandakı açar ilə imtahanın məlumatları eyni olmalıdır." });

        var existing = await db.Exams.SingleOrDefaultAsync(
            x => x.LessonCode == lessonCode && x.StudentNumber == studentNumber && x.ExamDate == examDate,
            cancellationToken);
        if (existing is null)
            return NotFound();

        existing.Grade = exam.Grade;

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

    [HttpDelete("{lessonCode}/{studentNumber:int}/{examDate}")]
    public async Task<IActionResult> Delete(
        string lessonCode, int studentNumber, DateOnly examDate, CancellationToken cancellationToken)
    {
        var exam = await db.Exams.SingleOrDefaultAsync(
            x => x.LessonCode == lessonCode && x.StudentNumber == studentNumber && x.ExamDate == examDate,
            cancellationToken);
        if (exam is null)
            return NotFound();

        db.Exams.Remove(exam);

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
}
