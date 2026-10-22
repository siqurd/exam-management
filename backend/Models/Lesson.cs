using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exam.Api.Models;

public sealed class Lesson : IValidatableObject
{
    [Required, StringLength(3, MinimumLength = 3)]
    public string Code { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Class { get; set; }

    [Required, StringLength(20)]
    public string TeacherFirstName { get; set; } = string.Empty;

    [Required, StringLength(20)]
    public string TeacherLastName { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (Code.Trim().Length != 3)
            yield return new ValidationResult("Dərsin kodu 3 simvoldan ibarət olmalıdır.", [nameof(Code)]);

        // SQL Server varchar limits are measured in bytes under UTF-8 collation.
        (string Field, string Value, int Limit)[] fields =
        [
            (nameof(Code), Code, 3),
            (nameof(Name), Name, 30),
            (nameof(TeacherFirstName), TeacherFirstName, 20),
            (nameof(TeacherLastName), TeacherLastName, 20)
        ];

        foreach (var (field, value, limit) in fields)
        {
            if (Encoding.UTF8.GetByteCount(value) > limit)
                yield return new ValidationResult($"Sahə UTF-8 formatında {limit} baytdan artıq ola bilməz.", [field]);
        }
    }
}
