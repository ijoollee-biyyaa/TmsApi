using Microsoft.AspNetCore.Authentication;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

builder.Services.AddAuthentication("Training").AddScheme<AuthenticationSchemeOptions,
TrainingAuthHandler>("Training", null);
builder.Services.AddAuthorization();
builder.Host.UseDefaultServiceProvider(options =>
{
    options.ValidateScopes = true;
    options.ValidateOnBuild = true;
});
builder.Services.AddSingleton<EnrollmentStore>();
builder.Services.AddSingleton<EnrollmentWorker>();
builder.Services.AddScoped<IEnrollmentService, EnrollmentService>();
builder.Services.AddOptions<PaymentOptions>()
    .BindConfiguration("Payments")
    .ValidateDataAnnotations()
    .ValidateOnStart();
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMiddleware<RequestLoggingMiddleware>();
app.UseExceptionHandler("/error");
//app.UseHttpsRedirection();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapGet("/api/assessments/results", () => Results.Ok(new
{
    courseCode = "CS-101",
    studentId  = "S-001",
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
    return record is not null ? Results.Ok(record) : Results.NotFound(new { message = $"Enrollment {id} not found" });
});

app.MapDelete("/api/enrollment/{id}", async (string id, IEnrollmentService svc) =>
{
    var deleted = await svc.DeleteAsync(id);
    return deleted ? Results.NoContent() : Results.NotFound(new { message = $"Enrollment {id} not found" });
});
app.Map("/error", () => Results.Problem("An unexpected error occurred, please try again later."));
app.MapGet("/api/enrollments/worker-smoke", (EnrollmentWorker worker) =>
{
    worker.ProcessBatch();
    return Results.Ok("processed");
});
app.MapControllers();

app.Run();
public record EnrollmentRequest(string StudentId, string CourseCode);