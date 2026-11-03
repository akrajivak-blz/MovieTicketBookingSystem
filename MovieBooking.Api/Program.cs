using Microsoft.EntityFrameworkCore;
using MovieBooking.Api.Data;
using MovieBooking.Api.Models;
using MovieBooking.Api.Services;
using Microsoft.Extensions.DependencyInjection;


var builder = WebApplication.CreateBuilder(args);

// Register DB Context using Sqlite
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=movie_booking.db"));

builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Enable Swagger UI for manual endpoint testing
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Database Seeding Logic
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.EnsureCreated();

    if (!context.Movies.Any())
    {
        var movie = new Movie { Title = "Inception", DurationMinutes = 148 };
        var theater = new Theater { Name = "Cineplex Downtown", City = "Vancouver" };
        var screen = new Screen { Name = "IMAX Hall 1" };
        theater.Screens.Add(screen);
        
        context.Movies.Add(movie);
        context.Theaters.Add(theater);
        context.SaveChanges();

        var show = new Show { MovieId = movie.Id, ScreenId = screen.Id, StartTime = DateTime.UtcNow.AddHours(2) };
        context.Shows.Add(show);
        context.SaveChanges();

        // Seed 5 available seats for testing
        for (int i = 1; i <= 5; i++)
        {
            context.ShowSeats.Add(new ShowSeat 
            { 
                ShowId = show.Id, 
                SeatNumber = $"A{i}", 
                SeatType = SeatType.Normal, 
                Status = SeatStatus.Available, 
                Price = 12.50m 
            });
        }
        context.SaveChanges();
    }
}

app.Run();
