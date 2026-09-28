namespace Domain.Models;

public class TrainSearchResult
{
    public int TrainId { get; set; }
    public string TrainNumber { get; set; } = "";
    public string TrainName { get; set; } = "";
    public string TrainType { get; set; } = "";
    public TimeSpan DepartureTime { get; set; }
    public TimeSpan ArrivalTime { get; set; }
    public int DaysTaken { get; set; }
    public int DistanceKm { get; set; }
    public List<ClassAvailability> Classes { get; set; } = [];
}

public class ClassAvailability
{
    public int ClassId { get; set; }
    public string ClassCode { get; set; } = "";
    public string ClassName { get; set; } = "";
    public decimal Fare { get; set; }
    public int AvailableSeats { get; set; }
    public string Status => AvailableSeats > 0 ? $"AVL {AvailableSeats}" : "WL";
}