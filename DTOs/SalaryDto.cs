using System;
using System.ComponentModel.DataAnnotations;

namespace student_app.DTOs
{
    public class SalaryDto
    {
        [Range(1, int.MaxValue, ErrorMessage = "TeacherId must be a valid teacher identifier.")]
        public int TeacherId { get; set; }

        [Range(typeof(decimal), "0.01", "999999999999.99", ErrorMessage = "BasicSalary must be greater than zero.")]
        public decimal BasicSalary { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99", ErrorMessage = "Allowances cannot be negative.")]
        public decimal Allowances { get; set; }

        [Range(typeof(decimal), "0.00", "999999999999.99", ErrorMessage = "Deductions cannot be negative.")]
        public decimal Deductions { get; set; }

        [Range(1, 12, ErrorMessage = "SalaryMonth must be between 1 and 12.")]
        public int SalaryMonth { get; set; }

        [Range(2000, 2100, ErrorMessage = "SalaryYear must be between 2000 and 2100.")]
        public int SalaryYear { get; set; }

        public DateTime? PaymentDate { get; set; }

        [MaxLength(250)]
        public string? Remarks { get; set; }
    }
}
