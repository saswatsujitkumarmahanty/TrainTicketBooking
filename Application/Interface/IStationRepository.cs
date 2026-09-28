using Domain.Models;

namespace Application.Interface
{
    public interface IStationRepository
    {
        Task<IEnumerable<Station>> SearchAsync(string term);
        Task<Station?> GetByIdAsync(int id);
    }
}
