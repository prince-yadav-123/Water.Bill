# Water.Bill

Fresh Water Billing solution isolated inside the existing BPMS repository.

## Project References

```text
Water.Bill.Core
  no project references

Water.Bill.Application
  -> Water.Bill.Core

Water.Bill.Infrastructure
  -> Water.Bill.Application
  -> Water.Bill.Core

Water.Bill.API
  -> Water.Bill.Application
  -> Water.Bill.Infrastructure

Water.Bill.ConsumerPortal
  -> Water.Bill.Application
  -> Water.Bill.Infrastructure
```

The portal references Infrastructure only for dependency injection and EF-backed auth services. Controllers should continue using Application interfaces and avoid direct database logic.

## Database First Scaffolding

Run scaffolding from this `Water.Bill` folder. Keep generated database-first files in Infrastructure:

```powershell
dotnet ef dbcontext scaffold "server=localhost;port=3306;database=water_bill;user=root;password=your_password;" Pomelo.EntityFrameworkCore.MySql --project src/Water.Bill.Infrastructure --startup-project src/Water.Bill.API --context WaterBillScaffoldDbContext --context-dir Data/Scaffolded --output-dir Data/Scaffolded/Entities --force
```

The hand-written auth foundation uses Core entities and `WaterBillDbContext`. For future water billing tables, prefer scaffolding into `Data/Scaffolded` first, then promote stable domain models into `Water.Bill.Core` only when the application layer needs clean domain objects.

## First Login Seed

Execute `database/auth-schema.sql` against MySQL.

Default admin:

```text
Username: admin
Password: Admin@1234
```
