using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;

namespace MovieBooking.Api.Services;

/// <summary>
/// Defines core transactional capabilities for handling seat inventory reservations and lifecycle confirmations.
/// </summary>
public interface IBookingService
{
    /// <summary>
    /// Attempts to temporarily lock a set of seats for a specific show using transaction-level isolation.
    /// </summary>
    Task<Booking?> CreateReservationAsync(int showId, List<int> seatIds, string userEmail);

    /// <summary>
    /// Confirms a pending seat reservation and finalizes the inventory lease change.
    /// </summary>
    Task<bool> ConfirmBookingAsync(int bookingId);
}

/// <summary>
/// Coordinates seat allocation routines using EF Core transactions backed by RowVersion concurrency tokens.
/// </summary>
public class BookingService : IBookingService
{
    private readonly AppDbContext _context;
    private const int HoldTimeMinutes = 10; // Time window before a temporary seat reservation expires

    public BookingService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> CreateReservationAsync(int showId, List<int> seatIds, string userEmail)
    {
        // Explicitly open an isolated database transaction block to guarantee atomicity.
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Query the requested seat allocations targeting this show.
            var seats = await _context.ShowSeats
                .Where(s => s.ShowId == showId && seatIds.Contains(s.Id))
                .ToListAsync();

            // Guard Clause: Verify all requested seats exist and are explicitly marked 'Available'.
            // If even one seat was taken or is invalid, abort immediately to prevent partial booking fragmentation.
            if (seats.Count != seatIds.Count || seats.Any(s => s.Status != SeatStatus.Available))
            {
                return null; 
            }

            // Flag all targeted items as temporarily locked. 
            // EF Core change tracking registers this mutation as an optimized column UPDATE command state.
            foreach (var seat in seats)
            {
                seat.Status = SeatStatus.Reserved;
            }

            var totalAmount = seats.Sum(s => s.Price);
            
            // Build the tracking ledger registry ticket instance.
            var booking = new Booking
            {
                ShowId = showId,
                UserEmail = userEmail,
                Status = BookingStatus.Pending,
                TotalAmount = totalAmount,
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(HoldTimeMinutes),
                BookedSeats = seats
            };

            _context.Bookings.Add(booking);

            // Save basic entity updates. If a concurrent execution thread modified any of 
            // these same rows since our select read statement, a DbUpdateConcurrencyException breaks here.
            await _context.SaveChangesAsync();
            
            // Irreversibly commit the data engine ledger mutations across all tracking boundaries.
            await transaction.CommitAsync();

            return booking;
        }
        catch (DbUpdateConcurrencyException)
        {
            // Specifically intercept simultaneous update collisions, rollback cleanly, and isolate the thread crack.
            await transaction.RollbackAsync();
            return null;
        }
        catch (Exception)
        {
            // Safety generic fallback sequence handler.
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<bool> ConfirmBookingAsync(int bookingId)
    {
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Eagerly pull the reservation context including the complete cluster of mapped held seats.
            var booking = await _context.Bookings
                .Include(b => b.BookedSeats)
                .FirstOrDefaultAsync(b => b.Id == bookingId);

            // Validation Check: Verify booking status is still pending and check if the hold deadline TTL has passed.
            if (booking == null || booking.Status != BookingStatus.Pending || DateTime.UtcNow > booking.ExpiresAt)
            {
                return false; 
            }

            // Transition states to absolute finality.
            booking.Status = BookingStatus.Confirmed;
            foreach (var seat in booking.BookedSeats)
            {
                seat.Status = SeatStatus.Booked;
            }

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();
            return true;
        }
        catch (Exception)
        {
            await transaction.RollbackAsync();
            return false;
        }
    }
}
