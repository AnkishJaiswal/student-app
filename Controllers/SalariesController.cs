using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using student_app.Data;
using student_app.DTOs;
using student_app.Model;

namespace student_app.Controllers
{
    /// <summary>
    /// Controller that exposes endpoints to manage teacher salaries.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class SalariesController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="SalariesController"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public SalariesController(AppDbContext db) => _db = db;

        /// <summary>
        /// Retrieves all salary records ordered by salary period.
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var salaries = await _db.Salaries
                .Include(s => s.Teacher)
                .OrderByDescending(s => s.SalaryYear)
                .ThenByDescending(s => s.SalaryMonth)
                .ThenByDescending(s => s.CreatedAt)
                .ToListAsync();

            return Ok(salaries);
        }

        /// <summary>
        /// Retrieves a single salary record by identifier.
        /// </summary>
        [HttpGet("GetSalary/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var salary = await _db.Salaries
                .Include(s => s.Teacher)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (salary == null) return NotFound();
            return Ok(salary);
        }

        /// <summary>
        /// Retrieves salary records for a teacher.
        /// </summary>
        [HttpGet("GetByTeacher/{teacherId:int}")]
        public async Task<IActionResult> GetByTeacher(int teacherId)
        {
            var teacherExists = await _db.Teachers.AnyAsync(t => t.Id == teacherId);
            if (!teacherExists) return NotFound(new { message = "Teacher not found." });

            var salaries = await _db.Salaries
                .Where(s => s.TeacherId == teacherId)
                .OrderByDescending(s => s.SalaryYear)
                .ThenByDescending(s => s.SalaryMonth)
                .ToListAsync();

            return Ok(salaries);
        }

        /// <summary>
        /// Creates a new salary record from the provided DTO.
        /// </summary>
        [HttpPost("Save")]
        public async Task<IActionResult> Create([FromBody] SalaryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var validationError = await ValidateSalaryDto(dto);
            if (validationError != null) return validationError;

            var salary = new Salary
            {
                TeacherId = dto.TeacherId,
                BasicSalary = dto.BasicSalary,
                Allowances = dto.Allowances,
                Deductions = dto.Deductions,
                NetSalary = CalculateNetSalary(dto),
                SalaryMonth = dto.SalaryMonth,
                SalaryYear = dto.SalaryYear,
                PaymentDate = dto.PaymentDate,
                Remarks = NormalizeOptional(dto.Remarks)
            };

            _db.Salaries.Add(salary);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueSalaryPeriodViolation(ex))
            {
                return Conflict(new { message = "Salary already exists for this teacher, month, and year." });
            }

            return CreatedAtAction(nameof(Get), new { id = salary.Id }, salary);
        }

        /// <summary>
        /// Updates an existing salary record identified by <paramref name="id"/>.
        /// </summary>
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] SalaryDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var salary = await _db.Salaries.FindAsync(id);
            if (salary == null) return NotFound();

            var validationError = await ValidateSalaryDto(dto);
            if (validationError != null) return validationError;

            salary.TeacherId = dto.TeacherId;
            salary.BasicSalary = dto.BasicSalary;
            salary.Allowances = dto.Allowances;
            salary.Deductions = dto.Deductions;
            salary.NetSalary = CalculateNetSalary(dto);
            salary.SalaryMonth = dto.SalaryMonth;
            salary.SalaryYear = dto.SalaryYear;
            salary.PaymentDate = dto.PaymentDate;
            salary.Remarks = NormalizeOptional(dto.Remarks);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueSalaryPeriodViolation(ex))
            {
                return Conflict(new { message = "Salary already exists for this teacher, month, and year." });
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a salary record by identifier.
        /// </summary>
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var salary = await _db.Salaries.FindAsync(id);
            if (salary == null) return NotFound();

            _db.Salaries.Remove(salary);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private async Task<IActionResult?> ValidateSalaryDto(SalaryDto dto)
        {
            var teacherExists = await _db.Teachers.AnyAsync(t => t.Id == dto.TeacherId);
            if (!teacherExists)
            {
                return BadRequest(new { message = "TeacherId does not match an existing teacher." });
            }

            if (dto.Deductions > dto.BasicSalary + dto.Allowances)
            {
                return BadRequest(new { message = "Deductions cannot be greater than BasicSalary plus Allowances." });
            }

            return null;
        }

        private static decimal CalculateNetSalary(SalaryDto dto) =>
            dto.BasicSalary + dto.Allowances - dto.Deductions;

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsUniqueSalaryPeriodViolation(DbUpdateException ex)
        {
            if (ex.InnerException is not SqlException sqlEx)
            {
                return false;
            }

            return sqlEx.Number == 2601 || sqlEx.Number == 2627;
        }
    }
}
