# BookMySeat (BMS) API - Developer Knowledge Transfer (KT) & Documentation

## Project Description
BookMySeat (BMS) API is a comprehensive backend system designed to manage library seating, subscriptions, and user bookings. It empowers library owners to monitor analytics, broadcast notifications, and manage floor plans (areas/seats), while allowing students (users) to book seats, track plans, and receive automated notifications for things like upcoming expiries.

## Project Architecture
The project follows a **Modular Monolith** architecture pattern built on **ASP.NET Core Web API** and **Entity Framework (EF) Core**.

- **Modules/Shared**: Contains shared core Entities/Models (e.g., `Booking`, `EndUser`, `Library`, `UserNotification`), the `ApplicationDbContext`, and global Enums.
- **Modules/Owner**: Contains Controllers, DTOs, and Services specifically scoped to the Library Owner (Analytics, Broadcasts, Floor Plan Management, Rule Engine).
- **Modules/User**: Contains Controllers, DTOs, and Services specifically scoped to the Student/End-User (Booking Seats, Viewing active plans).
- **Background Services**: The system uses `IHostedService` (`BroadcastService`, `ExpiryNotificationService`) for recurring background tasks.

## Auth Flow & Token Flow
The API is secured using **JWT (JSON Web Token)** authentication.

1. **Authentication**: Users (Owners or Students) authenticate using their credentials via the Auth endpoints.
2. **Token Generation**: Upon successful verification, the `AuthService` generates a signed JWT. This token contains encrypted claims, importantly `ClaimTypes.NameIdentifier` (the User ID) and `ClaimTypes.Role` ("Owner" or "User").
3. **API Requests**: The client application must send this token in the HTTP Headers for all protected requests: 
   `Authorization: Bearer <your_jwt_token>`
4. **Token Expiry**: Tokens are configured to expire after a set time (e.g., 7 days), after which the user must re-authenticate. The secret key is stored securely in `appsettings.json`.

## Database Management & Migrations

The project uses Entity Framework Core Code-First migrations.

### How to Add a Migration
Whenever you modify, add, or delete a model class in `Modules/Shared/Models`, you must create a new migration to sync the database schema.
Open your terminal in the API directory (where the `.csproj` file lives) and run:
```bash
dotnet ef migrations add <DescriptiveMigrationName>
```

### How to Update the Database
To apply your pending migrations to your local development database:
```bash
dotnet ef database update
```

**To update the Production Database:**
When deploying to your hosting provider (e.g., MonsterASP), you must pass your production connection string so EF Core knows where to apply the changes:
```bash
dotnet ef database update --connection "Server=db57889.public.databaseasp.net; Database=db57889; User Id=db57889; Password=<YOUR_PASSWORD>; Encrypt=True; TrustServerCertificate=True; MultipleActiveResultSets=True;"
```

## Important Things to Take Care Of (Developer KT)

1. **Soft Deletes vs. Hard Deletes**: 
   - We often use soft deletes for data integrity. For example, `Booking.IsDeactivated` and `Plan.IsDeleted`. 
   - **Crucial**: Whenever you write LINQ queries (especially for Analytics or fetching active lists), always remember to filter out deactivated/deleted records (e.g., `.Where(b => !b.IsDeactivated)`).
   
2. **Booking Statuses**:
   - Cancelled bookings have `Status == BookingStatus.Cancelled`. 
   - Refunding a booking does *not* automatically soft-delete it unless explicitly programmed. Always check the `PaymentStatus` vs the actual `BookingStatus`.
   
3. **Environment Configurations**:
   - `appsettings.json`: Contains your local database connection strings (often SQLite for rapid testing or local SQL Express).
   - `appsettings.Production.json`: Contains your production SQL Server connection string. Be careful not to commit sensitive production passwords into public source control.

4. **Background Services Lifecycle**:
   - Hosted services like `BroadcastService` run as Singletons. Do not inject Scoped services (like `ApplicationDbContext`) directly into their constructor. Instead, inject `IServiceScopeFactory`, create a scope within your `ExecuteAsync` method loop, and resolve the `ApplicationDbContext` from that scope.

5. **Cross-Origin Resource Sharing (CORS)**:
   - Ensure CORS policies in `Program.cs` are appropriately configured to allow your React frontend to communicate with the API without browser blockages.
