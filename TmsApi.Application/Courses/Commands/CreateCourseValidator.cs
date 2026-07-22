// TmsApi.Application/Courses/Commands/CreateCourseValidator.cs

using FluentValidation;

namespace TmsApi.Application.Courses.Commands;

public class CreateCourseValidator : AbstractValidator<CreateCourseCommand>
{
    public CreateCourseValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Course code is required.")
            .MaximumLength(10).WithMessage("Course code must be 10 characters or fewer.")
            .Matches(@"^[A-Z]{2,4}-\d{3}$").WithMessage("Course code must follow the format XX-000 (e.g., CS-101).");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title must be 200 characters or fewer.");

        RuleFor(x => x.MaxCapacity)
            .GreaterThan(0).WithMessage("Max capacity must be greater than 0.");
        RuleFor(x => x.Title)
            .NotEmpty()
            .MaximumLength(200)
            .Must(title => !string.IsNullOrWhiteSpace(title))
            .WithMessage("Title cannot be whitespace only");
    }
}