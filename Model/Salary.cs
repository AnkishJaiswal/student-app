using System;
using System.ComponentModel.DataAnnotations;

namespace student_app.Model
{
    public class Salary
    {
        public int Id { get; set; }

        [Range(1, int.MaxValue)]
        public int TeacherId { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99")]
        public decimal BasicSalary { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99")]
        public decimal Allowances { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99")]
        public decimal Deductions { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99")]
        public decimal NetSalary { get; set; }

        [Range(1, 12)]
        public int SalaryMonth { get; set; }

        [Range(2000, 2100)]
        public int SalaryYear { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(250)]
        public string? Remarks { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public Teacher? Teacher { get; set; }
    }
}
