public class Student
{
    public required string Id { get; set; }
    public required string Name
    {
        get ;
        set => field =! string.IsNullOrWhiteSpace(value) ? value : throw new ArgumentException("Name cannot be empty");
    }
    public int Age
    {
        get;
        set => field = value is >=16 and <=100 ? value : 
        throw new ArgumentOutOfRangeException(nameof(Age), "Age must be between 16 and 100");
    }

    public decimal GPA
    {
        get; 
        set => field = value is >=0.0m and <=4.0m ? value :
        throw new ArgumentOutOfRangeException(nameof(GPA), "GPA must be between 0.0 and 4.0");
    }
}