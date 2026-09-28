using System.ComponentModel.DataAnnotations;

namespace Application.Dto;

public class NotPastDateAttribute : ValidationAttribute
{
    public NotPastDateAttribute() : base("Journey date cannot be in the past.") { }

    public override bool IsValid(object? value)
        => value is DateOnly date && date >= DateOnly.FromDateTime(DateTime.Today);
}

public class PassengerDto
{
    [Required, StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = "";

    [Range(1, 120)]
    public int Age { get; set; }

    [Required, RegularExpression("^[MFTmft]$", ErrorMessage = "Gender must be M, F or T.")]
    public string Gender { get; set; } = "";
    public string? CoachCode { get; set; }
    public int? SeatNumber { get; set; }
    public string? BerthType { get; set; }
}

public class CreateBookingRequest
{
    [Range(1, int.MaxValue)] public int TrainId { get; set; }
    [Range(1, int.MaxValue)] public int ClassId { get; set; }
    [Range(1, int.MaxValue)] public int FromStationId { get; set; }
    [Range(1, int.MaxValue)] public int ToStationId { get; set; }

    [NotPastDate]
    public DateOnly JourneyDate { get; set; }

    [Required, MinLength(1), MaxLength(6)]
    public List<PassengerDto> Passengers { get; set; } = [];
}
public class BookingSummary
{
    public int BookingId { get; set; }
    public string Pnr { get; set; } = "";
    public string Status { get; set; } = "";
    public int? WaitlistNumber { get; set; }
    public DateOnly JourneyDate { get; set; }
    public decimal TotalFare { get; set; }
    public DateTime BookedAt { get; set; }
    public string TrainNumber { get; set; } = "";
    public string TrainName { get; set; } = "";
    public string ClassCode { get; set; } = "";
    public string ClassName { get; set; } = "";
    public string FromCode { get; set; } = "";
    public string FromName { get; set; } = "";
    public string ToCode { get; set; } = "";
    public string ToName { get; set; } = "";
}

public class BookingDetails : BookingSummary
{
    public List<PassengerDto> Passengers { get; set; } = [];
}

public class BookingCreatedResult
{
    public int BookingId { get; set; }
    public string Pnr { get; set; } = "";
    public decimal TotalFare { get; set; }
    public string Status { get; set; } = "";
    public int? WaitlistNumber { get; set; }
}