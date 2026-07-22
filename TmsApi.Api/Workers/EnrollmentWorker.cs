using TmsApi.Application.Interfaces;
public class EnrollmentWorker

{
   // private readonly IEnrollmentService _enrollmentService;
     private readonly IServiceScopeFactory _factory;

    public EnrollmentWorker(IServiceScopeFactory factory)
    {
        _factory = factory;
    }
    public void ProcessBatch()
    {
        using var scope = _factory.CreateScope();
        var svc = scope.ServiceProvider.GetRequiredService<IEnrollmentService>();
       // var record = svc.GetAllAsync().Result;
        //Console.WriteLine($"EnrollmentWorker: Processed {record.Count}records");
    }

   
}