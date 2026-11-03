# Movie Ticket Booking System

A robust and secure **C# application** designed to streamline the movie-going experience by managing theater listings, seat allocations, ticket purchases, and user accounts.

## 🚀 Features
* **User Authentication:** Secure registration and login flows for customers and administrators.
* **Movie Catalog:** Dynamic browsing of currently running movies, showtimes, genres, and descriptions.
* **Interactive Seat Selection:** Visual interface ensuring seats cannot be double-booked in real-time.
* **Ticket Generation:** Calculates total pricing including booking fees and outputs a distinct digital ticket.
* **Admin Dashboard:** Control panel to add/remove movies, adjust pricing tiers, and monitor seating capacities.

## 🛠️ Tech Stack & Prerequisites
* **Language:** C#
* **Framework/.NET Version:** .NET 8.0 / .NET Framework 4.8 *(Choose your version)*
* **UI Framework:** Windows Forms / WPF / Console Application *(Choose your UI type)*
* **Database:** SQL Server / Entity Framework Core / Local JSON storage *(Choose your data layer)*
* **IDE:** Visual Studio 2022 or VS Code

## 📦 How to Run the Project Locally

### 1. Clone the Repository
```bash
git clone https://github.com
```

### 2. Setup Database (If applicable)
* Open the package manager console.
* Run `Update-Database` to build your local schema.

### 3. Build and Run
* Open `MovieTicketBookingSystem.sln` using **Visual Studio**.
* Press `F5` or click the **Start/Run** button to launch the application.

## 👥 Future Enhancements
* Integrate a third-party payment gateway mock API.
* Implement a barcode/QR code generation system for digital ticket scanning.
* Add email confirmation receipts upon successful booking.
