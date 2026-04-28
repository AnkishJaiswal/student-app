using System;
using System.ComponentModel.DataAnnotations;

namespace student_app.Model
{
    public class Teacher
    {
        public int Id { get; set; }

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

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
