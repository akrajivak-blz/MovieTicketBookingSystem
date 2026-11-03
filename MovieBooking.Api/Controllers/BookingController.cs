using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using MovieBooking.Api.Services;

namespace MovieBooking.Api.Controllers;

public record ReservationRequest(int ShowId, List<int> SeatIds, string UserEmail);

[ApiController]
[Route("api/[controller]")]
public class BookingController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    [HttpPost("reserve")]
    public async Task<IActionResult> ReserveSeats([FromBody] ReservationRequest request)
    {
        var booking = await _bookingService.CreateReservationAsync(request.ShowId, request.SeatIds, request.UserEmail);
        if (booking == null)
        {
            return BadRequest("One or more seats are unavailable or invalid.");
        }
        return Ok(booking);
    }

    [HttpPost("confirm/{id}")]
    public async Task<IActionResult> ConfirmBooking(int id)
    {
        var result = await _bookingService.ConfirmBookingAsync(id);
        if (!result)
        {
            return BadRequest("Booking confirmation failed. It may have expired.");
        }
        return Ok("Booking successfully confirmed.");
    }
}
