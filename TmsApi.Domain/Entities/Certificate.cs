using System;
using TmsApi.Domain.Entities;
namespace TmsApi.Domain.Entities;

public class Certificate
{
    public int Id { get; set; }
    public required string SerialNumber { get; set; }
    public DateTime IssuedAt { get; set; } = DateTime.UtcNow;

    public bool IsRevoked { get; set; } = false;
    public string? RevokedReason { get; set; }
    public DateTime? RevokedAt { get; set; }
 
    public int StudentId { get; set; }
    public int CourseId { get; set; }
    public Student Student { get; set; } = null!;
    public Course Course { get; set; } = null!;
}