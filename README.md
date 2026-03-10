# SE HR Employee Portal

A lightweight, air-gapped submission portal for Specialized Engineering employee certification requests.

## Architecture & Security Isolation

This application runs as a standalone ASP.NET Core Razor Pages Satellite App on IIS **Port 8081** to provide a physical security boundary between general employees and the HR Admin Dashboard.

- **Admin Portal (`se-hr-certification-portal`):** Runs securely on **Port 8080**, managing administrative workflows and restricted data access.
- **Employee Portal (`se-hr-employee-portal`):** Runs on **Port 8081**, strictly facilitating the secure submission of new employee requests.

By design, this separation limits the operational attack surface while still allowing the system to seamlessly read from and write to the shared PostgreSQL database. All UI assets (Bootstrap, Lucide Icons, jQuery) are purely vendorized and stored statically in `wwwroot` to ensure complete air-gap compliance on the satellite port.

---

## Technical Stack
- `.NET 10.0`
- `Entity Framework Core`
- `PostgreSQL`
- `Bootstrap 5.3.3`

---

## Deployment Checklist (For Chris L.)

- [ ] Pull the latest `main` branch code to the production server.
- [ ] Create a dedicated App Pool in IIS for the employee portal application.
- [ ] Bind the IIS Site specifically to **Port 8081**.
- [ ] Verify the shared `DefaultConnection` PostgreSQL connection string in `appsettings.json` accurately points to the established Admin Portal database instance.
- [ ] Provide write access privileges to the physical IIS runtime folder.
- [ ] Start the application pool and verify the portal loads properly at `http://localhost:8081/`.
