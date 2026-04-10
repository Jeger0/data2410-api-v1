using System.Text.Json.Serialization;

namespace data2410_api_v1.Models;

public class Student
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Course { get; set; }
    public int Marks { get; set; }
    public string? Grade { get; set; }
}
