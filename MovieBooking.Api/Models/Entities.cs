using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace MovieBooking.Api.Models;

public class Movie
{
    public int Id { get; set; }
    [Required] public string Title { get; set; } = string.Empty;
    public int DurationMinutes { get; set; }
}

public class Theater
{
    public int Id { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
    [Required] public string City { get; set; } = string.Empty;
    public List<Screen> Screens { get; set; } = [];
}

public class Screen
{
    public int Id { get; set; }
    public int TheaterId { get; set; }
    [Required] public string Name { get; set; } = string.Empty;
}

public class Show
{
    public int Id { get; set; }
    public int MovieId { get; set; }
    public int ScreenId { get; set; }
    public DateTime StartTime { get; set; }
    public Movie? Movie { get; set; }
    public Screen? Screen { get; set; }
}

public class ShowSeat
{
    public int Id { get; set; }
    public int ShowId { get; set; }
    [Required] public string SeatNumber { get; set; } = string.Empty;
    public SeatType SeatType { get; set; }
    public SeatStatus Status { get; set; }
    public decimal Price { get; set; }
    [Timestamp] public byte[]? RowVersion { get; set; }
}

public class Booking
{
    public int Id { get; set; }
    public int ShowId { get; set; }
    [Required] public string UserEmail { get; set; } = string.Empty;
    public BookingStatus Status { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAt { get; set; }
    public List<ShowSeat> BookedSeats { get; set; } = [];
}
