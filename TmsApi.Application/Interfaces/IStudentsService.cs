using TmsApi.Application.DTOs;

namespace TmsApi.Application.Interfaces;
public interface IStudentsService
{
    

    Task<PagedResponse<StudentResponseDto>> GetStudentsAsync(PagedRequest request, CancellationToken ct);
    Task<StudentResponseDto?> GetByIdAsync(int id, CancellationToken ct);
    Task<StudentResponseDto> CreateAsync(CreateStudentRequest request, CancellationToken ct);
    Task<bool> RegistrationNumberExistsAsync(string registrationNumber, CancellationToken ct);
    Task<bool> RegistrationNumberExistsUpdatesAsync(int id, string registrationNumber , CancellationToken ct);
    Task<bool?> UpdateStudentASync(int id, CreateStudentRequest request , CancellationToken ct);
    Task<bool> DeleteStudentAsync(int id, CancellationToken ct);
}
