using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using student_app.Data;
using student_app.DTOs;
using student_app.Model;

namespace student_app.Controllers
{
    /// <summary>
    /// Controller that exposes endpoints to manage teachers.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class TeachersController : ControllerBase
    {
        private readonly AppDbContext _db;

        /// <summary>
        /// Initializes a new instance of the <see cref="TeachersController"/> class.
        /// </summary>
        /// <param name="db">The application database context.</param>
        public TeachersController(AppDbContext db) => _db = db;

        /// <summary>
        /// Retrieves all teachers ordered by creation date (descending).
        /// </summary>
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAll()
        {
            var teachers = await _db.Teachers.OrderByDescending(t => t.CreatedAt).ToListAsync();
            return Ok(teachers);
        }

        /// <summary>
        /// Retrieves a single teacher by identifier.
        /// </summary>
        [HttpGet("GetTeacher/{id:int}")]
        public async Task<IActionResult> Get(int id)
        {
            var teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();
            return Ok(teacher);
        }

        /// <summary>
        /// Creates a new teacher from the provided DTO.
        /// </summary>
        [HttpPost("Save")]
        public async Task<IActionResult> Create([FromBody] TeacherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacher = new Teacher
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = NormalizeOptional(dto.Email),
                Age = dto.Age,
                Subject = NormalizeOptional(dto.Subject)
            };

            _db.Teachers.Add(teacher);

            try
            {
                await _db.SaveChangesAsync();
            }
            catch (DbUpdateException ex) when (IsUniqueEmailViolation(ex))
            {
                return Conflict(new { message = "Email is already in use." });
            }

            return CreatedAtAction(nameof(Get), new { id = teacher.Id }, teacher);
        }

        /// <summary>
        /// Updates an existing teacher identified by <paramref name="id"/> using the provided DTO.
        /// </summary>
        [HttpPut("Update/{id:int}")]
        public async Task<IActionResult> Update(int id, [FromBody] TeacherDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            teacher.FirstName = dto.FirstName;
            teacher.LastName = dto.LastName;
            teacher.Email = NormalizeOptional(dto.Email);
            teacher.Age = dto.Age;
            teacher.Subject = NormalizeOptional(dto.Subject);

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
        /// Deletes a teacher by identifier.
        /// </summary>
        [HttpDelete("Delete/{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            var teacher = await _db.Teachers.FindAsync(id);
            if (teacher == null) return NotFound();

            _db.Teachers.Remove(teacher);
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
