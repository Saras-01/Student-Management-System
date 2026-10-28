using Microsoft.Data.SqlClient;
using StudentCourseManagement.Models;

namespace StudentCourseManagement.Repository;

public class StudentRepository
{
    private readonly string _connectionString;

    public StudentRepository(string connectionString)
    {
        _connectionString = connectionString;
    }

    // 1. Add Student
    public async Task AddStudentAsync(Student student)
    {
        const string query = """
            INSERT INTO Students (Name, Email, Age)
            VALUES (@Name, @Email, @Age)
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@Name", student.Name);
        command.Parameters.AddWithValue("@Email", student.Email);
        command.Parameters.AddWithValue("@Age", student.Age);

        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
    }

    // 2. View All Students
    public async Task<List<Student>> GetAllStudentsAsync()
    {
        const string query = """
            SELECT StudentId, Name, Email, Age
            FROM Students
            ORDER BY StudentId
            """;

        var students = new List<Student>();

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            students.Add(new Student
            {
                StudentId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Age = reader.GetInt32(3)
            });
        }

        return students;
    }

    // 3. Search Student By ID
    public async Task<Student?> GetStudentByIdAsync(int id)
    {
        const string query = """
            SELECT StudentId, Name, Email, Age
            FROM Students
            WHERE StudentId = @StudentId
            """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@StudentId", id);

        await connection.OpenAsync();

        await using var reader =
            await command.ExecuteReaderAsync();

        if (await reader.ReadAsync())
        {
            return new Student
            {
                StudentId = reader.GetInt32(0),
                Name = reader.GetString(1),
                Email = reader.GetString(2),
                Age = reader.GetInt32(3)
            };
        }

        return null;
    }
    // 4. Update Student
    public async Task<bool> UpdateStudentAsync(Student student)
    {
        const string query = """
        UPDATE Students
        SET Name = @Name,
            Email = @Email,
            Age = @Age
        WHERE StudentId = @StudentId
        """;

        await using var connection =
            new SqlConnection(_connectionString);

        await using var command =
            new SqlCommand(query, connection);

        command.Parameters.AddWithValue("@StudentId", student.StudentId);
        command.Parameters.AddWithValue("@Name", student.Name);
        command.Parameters.AddWithValue("@Email", student.Email);
        command.Parameters.AddWithValue("@Age", student.Age);

        await connection.OpenAsync();

        int rowsAffected = await command.ExecuteNonQueryAsync();

        return rowsAffected > 0;
    }

    // 4. Enroll Student Using Transaction
    public async Task EnrollStudentAsync(int studentId, int courseId)
    {
        await using var connection =
            new SqlConnection(_connectionString);

        await connection.OpenAsync();

        await using var transaction =
            await connection.BeginTransactionAsync();

        try
        {
            // Insert enrollment record
            const string enrollmentSql = """
                INSERT INTO Enrollments (StudentId, CourseId)
                VALUES (@StudentId, @CourseId)
                """;

            await using (var command = new SqlCommand(
                enrollmentSql,
                connection,
                (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue(
                    "@StudentId", studentId);

                command.Parameters.AddWithValue(
                    "@CourseId", courseId);

                await command.ExecuteNonQueryAsync();
            }

            // Insert audit record
            const string auditSql = """
                INSERT INTO EnrollmentAudit
                    (StudentId, CourseId, ActionName)
                VALUES
                    (@StudentId, @CourseId, @ActionName)
                """;

            await using (var command = new SqlCommand(
                auditSql,
                connection,
                (SqlTransaction)transaction))
            {
                command.Parameters.AddWithValue(
                    "@StudentId", studentId);

                command.Parameters.AddWithValue(
                    "@CourseId", courseId);

                command.Parameters.AddWithValue(
                    "@ActionName", "ENROLLED");

                await command.ExecuteNonQueryAsync();
            }

            // Commit both operations
            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}