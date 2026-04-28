using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using student_app.Data;
using student_app.DTOs;
using student_app.Model;

namespace student_app.Controllers
{
    /// <summary>
    /// Controller that exposes endpoints to manage students.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class StudentsController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="StudentsController"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public StudentsController(AppDbContext db) => _db = db;

        /// <summary>
        /// Retrieves all students ordered by creation date (descending).
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var students = await _db.Students.OrderByDescending(s => s.CreatedAt).ToListAsync();
            return Ok(students);
        }

        /// <summary>
        /// Retrieves a single student by identifier.
        /// </summary>
        [HttpGet("GetStudent/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var student = await _db.Students.FindAsync(id);
            if (student == null) return NotFound();
            return Ok(student);
        }

        /// <summary>
        /// Creates a new student from the provided DTO.
        /// </summary>
        [HttpPost("Save")]
        public async Task<IActionResult> Create([FromBody] StudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var student = new Student
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = NormalizeOptional(dto.Email),
                Age = dto.Age,
                Grade = NormalizeOptional(dto.Grade)
            };

            _db.Students.Add(student);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
            {
                return Conflict(new { message = "Email is already in use." });
            }

            return CreatedAtAction(nameof(Get), new { id = student.Id }, student);
        }

        /// <summary>
        /// Updates an existing student identified by <paramref name="id"/> using the provided DTO.
        /// </summary>
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] StudentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var student = await _db.Students.FindAsync(id);
            if (student == null) return NotFound();

            student.FirstName = dto.FirstName;
            student.LastName = dto.LastName;
            student.Email = NormalizeOptional(dto.Email);
            student.Age = dto.Age;
            student.Grade = NormalizeOptional(dto.Grade);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
            {
                return Conflict(new { message = "Email is already in use." });
            }

            return NoContent();
        }

        /// <summary>
        /// Deletes a student by identifier.
        /// </summary>
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var student = await _db.Students.FindAsync(id);
            if (student == null) return NotFound();

            _db.Students.Remove(student);
            await _db.SaveChangesAsync();
            return NoContent();
        }

        private static string? NormalizeOptional(string? value) =>
            string.IsNullOrWhiteSpace(value) ? null : value.Trim();

        private static bool IsUniqueEmailViolation(DbUpdateException ex)
        {
            if (ex.InnerException is not SqlException sqlEx)
            {
                return false;
            }

            return sqlEx.Number == 2601 || sqlEx.Number == 2627;
        }
    }
}
