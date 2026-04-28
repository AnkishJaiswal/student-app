using System.ComponentModel.DataAnnotations;

namespace student_app.DTOs
{
    public class StudentDto
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;

        [EmailAddress, MaxLength(200)]
        public string? Email { get; set; }

        [Range(3, 100)]
        public int Age { get; set; }

        [MaxLength(50)]
        public string? Grade { get; set; }

    }
}
