
public class Course
{
    public required string Code { get; set; }
    public required string Title 
    { 
        get;
         set => field = !string.IsNullOrWhiteSpace(value) ? value
          : throw new ArgumentException("Title cannot be empty");}
    public int Capacity
    {
        get;
        set => field = value is > 0 ? value :
        throw new ArgumentOutOfRangeException(nameof(Capacity), "Capacity must be greater than 0");
    }
    public int EnrolledCount { get; set; }
}