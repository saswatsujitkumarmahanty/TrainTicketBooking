using Domain.Models;

namespace Application.Interface;

public interface ITrainRepository
{
    Task<IEnumerable<TrainSearchResult>> SearchAsync(int fromStationId, int toStationId, DateOnly journeyDate);
}
