using System.ComponentModel.DataAnnotations;
using System.Text;

namespace Exam.Api.Models;

public sealed class Student : IValidatableObject
{
    [Range(1, 99999)]
    public int Number { get; set; }

    [Required, StringLength(30)]
    public string FirstName { get; set; } = string.Empty;

    [Required, StringLength(30)]
    public string LastName { get; set; } = string.Empty;

    [Range(1, 99)]
    public int Class { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        // SQL Server varchar(30) stores at most 30 bytes under UTF-8 collation.
        if (Encoding.UTF8.GetByteCount(FirstName) > 30)
            yield return new ValidationResult("Ad UTF-8 formatında 30 baytdan artıq ola bilməz.", [nameof(FirstName)]);

        if (Encoding.UTF8.GetByteCount(LastName) > 30)
            yield return new ValidationResult("Soyad UTF-8 formatında 30 baytdan artıq ola bilməz.", [nameof(LastName)]);
    }
}
