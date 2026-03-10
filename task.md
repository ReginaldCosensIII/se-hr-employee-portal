# se-hr-employee-portal Task List

## 1. Scaffolding & Configuration
- [x] Initialize ASP.NET Core Razor Pages project.
- [x] Add necessary NuGet packages (Npgsql.EntityFrameworkCore.PostgreSQL).
- [x] Configure `appsettings.json` and `appsettings.Development.json` with exact parity to the Admin Portal.
- [x] Configure `Program.cs` to run on port `8081` and register `ApplicationDbContext`.

## 2. Shared Data Layer Setup
- [x] Create `Data` and `Models` directories.
- [x] Copy `Models.cs` from `se-hr-certification-portal`.
- [x] Copy `ApplicationDbContext.cs` from `se-hr-certification-portal`.

## 3. Air-Gapped Asset Integration
- [x] Establish `wwwroot/lib`, `wwwroot/css`, and `wwwroot/img`.
- [x] Vendorize Bootstrap, jQuery, and Lucide icons locally from Admin Portal.
- [x] Copy `site.css` from Admin Portal.
- [x] Create `Pages/Shared/_Layout.cshtml` pointing ONLY to local resources (No CDNs).

## 4. UI Porting & Form Implementation
- [x] Create Submission Form in `Pages/Index.cshtml`.
- [x] Match Admin UI (typography, branding, success/error notifications).
- [x] Implement Dual-Layer Validation (HTML5 + C# Server-Side).
- [x] Implement Auto-Registry Logic in `Index.cshtml.cs` (warn on missing name -> auto-add on submission).

## 5. Verification Phase
- [/] Verify build succeeds.
- [ ] Perform offline air-gap test (verify network requests).
- [ ] Test submission flow and newly added automatic employee registry.
