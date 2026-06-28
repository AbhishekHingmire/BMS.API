using System.Threading.Tasks;
using BMS.API.Modules.Owner.DTOs;

namespace BMS.API.Modules.Owner.Services
{
    public interface IOwnerAuthService
    {
        Task<AuthResponseDto> RegisterAsync(OwnerRegisterRequestDto request);
        Task<AuthResponseDto> LoginAsync(OwnerLoginRequestDto request);
    }

    public class OwnerAuthService : IOwnerAuthService
    {
        public Task<AuthResponseDto> LoginAsync(OwnerLoginRequestDto request)
        {
            // Blueprint: Implementation details omitted
            throw new System.NotImplementedException();
        }

        public Task<AuthResponseDto> RegisterAsync(OwnerRegisterRequestDto request)
        {
            // Blueprint: Implementation details omitted
            throw new System.NotImplementedException();
        }
    }
}
