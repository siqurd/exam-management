using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exam.Api.Models;

public sealed class ExamResult : IValidatableObject
{
    [Required, StringLength(3, MinimumLength = 3)]
    public string LessonCode { get; set; } = string.Empty;

    [Range(1, 99999)]
    public int StudentNumber { get; set; }

    public DateOnly ExamDate { get; set; }

    [Range(0, 9)]
    public int Grade { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (LessonCode.Trim().Length != 3 || Encoding.UTF8.GetByteCount(LessonCode) != 3)
            yield return new ValidationResult("Dərsin kodu 3 simvol və 3 baytdan ibarət olmalıdır.", [nameof(LessonCode)]);

        if (ExamDate == default)
            yield return new ValidationResult("İmtahan tarixi qeyd olunmalıdır.", [nameof(ExamDate)]);
    }
}
