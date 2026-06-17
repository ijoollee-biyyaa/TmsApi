using Microsoft.AspNetCore.Authentication;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddAuthentication("Training")
    .AddScheme<AuthenticationSchemeOptions, TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();

builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes  = true;
    options.ValidateOnBuild = true;
});

builder.Services.AddSingleton<EnrollmentStore>();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddSingleton<StudentStore>();
builder.Services.AddSingleton<CourseStore>();
builder.Services.AddSingleton<AssessmentStore>();
builder.Services.AddScoped<IAssessmentService, AssessmentService>();
builder.Services.AddScoped<ICourseService, CourseService>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddScoped<IStudentsService, StudentsService>();
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

var app = builder.Build();

// ── Environment toggle ───
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}
else
{
    // Production only — hide stack traces
    app.UseExceptionHandler("/api/error");
}

// ── Middleware pipeline ────
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseStatusCodePages();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();

// ── Endpoints ─
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode  = "CS-101",
    studentId   = "S-001",
    letterGrade = "A"
})).RequireAuthorization();

app.MapGet("/api/enrollment", async (IEnrollmentService svc) =>
{
    var all = await svc.GetAllAsync();
    return Results.Ok(all);
});

app.MapPost("/api/enrollment", async (EnrollmentRequest req, IEnrollmentService svc) =>
{
    var record = await svc.EnrollAsync(req.StudentId, req.CourseCode);
    return Results.Created($"/api/enrollment/{record.Id}", record);
});

app.MapGet("/api/enrollment/{id}", async (string id, IEnrollmentService svc) =>
{
    var record = await svc.GetByIdAsync(id);
    return record is not null
        ? Results.Ok(record)
        : Results.NotFound(new { message = $"Enrollment {id} not found" });
});

app.MapDelete("/api/enrollment/{id}", async (string id, IEnrollmentService svc) =>
{
    var deleted = await svc.DeleteAsync(id);
    return deleted
        ? Results.NoContent()
        : Results.NotFound(new { message = $"Enrollment {id} not found" });
});

// Error handler endpoint — returns ProblemDetails
app.Map("/api/error", (HttpContext ctx) =>
{
    return Results.Problem(
        detail: "An unexpected error occurred.",
        statusCode: 500
    );
});

// Test endpoint — throws to trigger error handler
app.MapGet("/api/test-error", () =>
{
    throw new TmsDatabaseException("Simulated database failure for ProblemDetails testing!");
});

// Worker smoke test
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});

app.MapControllers();
app.Run();

public record EnrollmentRequest(string StudentId, string CourseCode);