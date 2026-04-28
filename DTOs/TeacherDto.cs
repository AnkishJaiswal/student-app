using System.ComponentModel.DataAnnotations;

namespace student_app.DTOs
{
    public class TeacherDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [Range(18, 100)]
        public int Age { get; set; }

        [MaxLength(100)]
        public string? Subject { get; set; }
    }
}
