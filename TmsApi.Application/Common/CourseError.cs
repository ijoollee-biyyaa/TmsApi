namespace TmsApi.Application.Common;
public sealed record CourseError(string Code, string Message)
{
    public static CourseError CodeAlreadyExists(string code) =>
        new("course_code_exists",
            $"Course code '{code}' already exists.");

    public static CourseError InvalidCapacity()=>
    new("invalid_capacity", $"Max Capacity must be greater than 0  ");

   

}