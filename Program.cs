using Microsoft.Data.SqlClient;
using StudentCourseManagement.Models;
using StudentCourseManagement.Repository;
using StudentCourseManagement.Services;

string connectionString =
@"Server=.\SQLEXPRESS;Database=StudentCourseDB;Trusted_Connection=True;TrustServerCertificate=True;";

var studentRepository = new StudentRepository(connectionString);
var courseRepository = new CourseRepository(connectionString);

var studentService = new StudentService(studentRepository);
var courseService = new CourseService(courseRepository);

while (true)
{
    Console.WriteLine("\n=== STUDENT COURSE MANAGEMENT ===");
    Console.WriteLine("1. Add Student");
    Console.WriteLine("2. View All Students");
    Console.WriteLine("3. Search Student By ID");
    Console.WriteLine("4. Add Course");
    Console.WriteLine("5. View All Courses");
    Console.WriteLine("6. Search Course By ID");
    Console.WriteLine("7. Exit");
    Console.WriteLine("8. Enroll Student in Course");
    Console.WriteLine("9. Update Student");
    Console.Write("Enter your choice: ");
string? choice = Console.ReadLine();

    try
    {
        switch (choice)
        {
            case "1":
                {
                    Console.Write("Student name: ");
                    string name = Console.ReadLine() ?? "";

                    Console.Write("Email: ");
                    string email = Console.ReadLine() ?? "";

                    Console.Write("Age: ");
                    if (!int.TryParse(Console.ReadLine(), out int age))
                    {
                        Console.WriteLine("Invalid age.");
                        break;
                    }

                    await studentService.AddStudentAsync(new Student
                    {
                        Name = name,
                        Email = email,
                        Age = age
                    });

                    Console.WriteLine("Student added successfully.");
                    break;
                }

            case "2":
                {
                    var students = await studentService.GetAllStudentsAsync();

                    if (students.Count == 0)
                    {
                        Console.WriteLine("No students found.");
                        break;
                    }

                    foreach (var s in students)
                    {
                        Console.WriteLine(
                            $"{s.StudentId} | {s.Name} | {s.Email} | {s.Age}");
                    }

                    break;
                }

            case "3":
                {
                    Console.Write("Student ID: ");

                    if (!int.TryParse(Console.ReadLine(), out int id))
                    {
                        Console.WriteLine("Invalid student ID.");
                        break;
                    }

                    var student = await studentService.GetStudentByIdAsync(id);

                    if (student is null)
                        Console.WriteLine("Student not found.");
                    else
                        Console.WriteLine(
                            $"{student.StudentId} | {student.Name} | {student.Email} | {student.Age}");

                    break;
                }

            case "4":
                {
                    Console.Write("Course name: ");
                    string courseName = Console.ReadLine() ?? "";

                    Console.Write("Duration in months: ");

                    if (!int.TryParse(Console.ReadLine(), out int duration))
                    {
                        Console.WriteLine("Invalid duration.");
                        break;
                    }

                    Console.Write("Fee: ");

                    if (!decimal.TryParse(Console.ReadLine(), out decimal fee))
                    {
                        Console.WriteLine("Invalid fee.");
                        break;
                    }

                    await courseService.AddCourseAsync(new Course
                    {
                        CourseName = courseName,
                        DurationMonths = duration,
                        Fee = fee
                    });

                    Console.WriteLine("Course added successfully.");
                    break;
                }

            case "5":
                {
                    var courses = await courseService.GetAllCoursesAsync();

                    if (courses.Count == 0)
                    {
                        Console.WriteLine("No courses found.");
                        break;
                    }

                    foreach (var course in courses)
                    {
                        Console.WriteLine(
                            $"{course.CourseId} | {course.CourseName} | {course.DurationMonths} months | {course.Fee:N2}");
                    }

                    break;
                }

            case "6":
                {
                    Console.Write("Course ID: ");

                    if (!int.TryParse(Console.ReadLine(), out int id))
                    {
                        Console.WriteLine("Invalid course ID.");
                        break;
                    }

                    var course = await courseService.GetCourseByIdAsync(id);

                    if (course is null)
                        Console.WriteLine("Course not found.");
                    else
                        Console.WriteLine(
                            $"{course.CourseId} | {course.CourseName} | {course.DurationMonths} months | {course.Fee:N2}");

                    break;
                }

            case "7":
                Console.WriteLine("Application closed.");
                return;

            case "8":
                {
                    Console.Write("Student ID: ");

                    if (!int.TryParse(Console.ReadLine(), out int studentId))
                    {
                        Console.WriteLine("Invalid student ID.");
                        break;
                    }

                    Console.Write("Course ID: ");

                    if (!int.TryParse(Console.ReadLine(), out int courseId))
                    {
                        Console.WriteLine("Invalid course ID.");
                        break;
                    }

                    await studentService.EnrollStudentAsync(studentId, courseId);

                    Console.WriteLine("Student enrolled successfully.");
                    break;
                }

            case "9":
                {
                    Console.Write("Enter the student ID to update: ");

                    if (!int.TryParse(Console.ReadLine(), out int studentId))
                    {
                        Console.WriteLine("Invalid student ID.");
                        break;
                    }

                    var existingStudent =
                        await studentService.GetStudentByIdAsync(studentId);

                    if (existingStudent is null)
                    {
                        Console.WriteLine("Student not found.");
                        break;
                    }

                    Console.WriteLine("\nCurrent student details:");
                    Console.WriteLine($"Name: {existingStudent.Name}");
                    Console.WriteLine($"Email: {existingStudent.Email}");
                    Console.WriteLine($"Age: {existingStudent.Age}");

                    Console.Write("\nEnter new name: ");
                    string newName = Console.ReadLine() ?? "";

                    Console.Write("Enter new email: ");
                    string newEmail = Console.ReadLine() ?? "";

                    Console.Write("Enter new age: ");

                    if (!int.TryParse(Console.ReadLine(), out int newAge))
                    {
                        Console.WriteLine("Invalid age.");
                        break;
                    }

                    bool updated = await studentService.UpdateStudentAsync(
                        new Student
                        {
                            StudentId = studentId,
                            Name = newName,
                            Email = newEmail,
                            Age = newAge
                        });

                    if (updated)
                        Console.WriteLine("Student updated successfully.");
                    else
                        Console.WriteLine("Student could not be updated.");

                    break;
                }

            default:
                Console.WriteLine("Invalid choice.");
                break;
        }
    }
    catch (ArgumentException ex)
    {
        Console.WriteLine($"Validation error: {ex.Message}");
    }
    catch (SqlException ex)
    {
        Console.WriteLine($"Database error: {ex.Message}");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Unexpected error: {ex.Message}");
    }
}
