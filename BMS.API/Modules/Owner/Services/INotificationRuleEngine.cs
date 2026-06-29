using System.Threading.Tasks;
using BMS.API.Modules.Shared.Models;

namespace BMS.API.Modules.Owner.Services
{
    public interface INotificationRuleEngine
    {
        Task ProcessBookingCreatedAsync(Booking booking);
        Task ProcessBookingPaymentUpdatedAsync(Booking booking);
    }
}
