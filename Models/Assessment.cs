public interface IGradable
{
    string Title { get; }
    decimal CalculateGrade();
}

public class Quiz : IGradable
{
    public required string Title { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }

    public decimal CalculateGrade()
    {
        if (TotalQuestions == 0) return 0m;
        return (decimal)CorrectAnswers / TotalQuestions * 100m;
    }
}

public class LabAssignment : IGradable
{
    public required string Title { get; set; }
    public decimal FunctionalityScore { get; set; }
    public decimal CodeQualityScore { get; set; }

    public decimal CalculateGrade()
    {
       return (FunctionalityScore * 0.7m + CodeQualityScore *0.3m);
    }
}

public class Assessment
{
    public required string Id { get; set; }
    public required string StudentId { get; set; }
    public required string CourseCode { get; set; }
    public required string Type { get; set; } // e.g., "Quiz", "Lab"
    public decimal Grade
     {
         get;
         set => field = value is >= 0m and <= 100m ? value 
            : throw new ArgumentOutOfRangeException(nameof(Grade), "Grade must be between 0 and 100");
          }

      public DateTime CreatedAt { get; set; } = DateTime.UtcNow;    
}