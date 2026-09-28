using Application.Dto;
using Application.Interface;
using Domain.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace TrainTicketBooking.Controllers;

[ApiController]
[Authorize]
[Route("api/[controller]")]
public class BookingsController(IBookingRepository bookings) : ControllerBase
{
    private int UserId => int.Parse(User.FindFirst("sub")!.Value);
    // POST api/bookings
    [HttpPost]
    public async Task<IActionResult> Create(CreateBookingRequest request)
    {
        if (request.FromStationId == request.ToStationId)
            return BadRequest(new { error = "Source and destination must be different." });
        try
        {
            var result = await bookings.CreateAsync(UserId, request);
            return CreatedAtAction(nameof(GetByPnr), new { pnr = result.Pnr }, result);
        }
        catch (BusinessRuleException ex)
        {
            return ToResult(ex);
        }
    }
    // GET api/bookings   (my bookings)
    [HttpGet]
    public async Task<IActionResult> GetMine() => Ok(await bookings.GetByUserAsync(UserId));
    // GET api/bookings/1234567890
    [HttpGet("{pnr:length(10)}")]
    public async Task<IActionResult> GetByPnr(string pnr)
    {
        var booking = await bookings.GetByPnrAsync(pnr, UserId);
        return booking is null ? NotFound(new { error = "Booking not found." }) : Ok(booking);
    }
    // POST api/bookings/1234567890/cancel
    [HttpPost("{pnr:length(10)}/cancel")]
    public async Task<IActionResult> Cancel(string pnr)
    {
        try
        {
            await bookings.CancelAsync(pnr, UserId);
            return Ok(new { message = "Booking cancelled." });
        }
        catch (BusinessRuleException ex)
        {
            return ToResult(ex);
        }
    }

    private IActionResult ToResult(BusinessRuleException ex) => ex.ErrorNumber switch
    {
        50020 => NotFound(new { error = ex.Message }),
        50015 => Conflict(new { error = ex.Message }),       // sold out
        _ => BadRequest(new { error = ex.Message })
    };
}
