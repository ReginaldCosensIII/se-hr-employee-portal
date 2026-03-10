using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeHrCertificationPortal.Data;
using SeHrCertificationPortal.Models;
using System.ComponentModel.DataAnnotations;

namespace SeHrEmployeePortal.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;

    public IndexModel(ApplicationDbContext context)
    {
        _context = context;
    }

    [BindProperty]
    public CertificationRequestInput Input { get; set; } = new();

    public List<string> ExistingEmployeeNames { get; set; } = new();

    public SelectList Agencies { get; set; } = default!;
    public SelectList Certifications { get; set; } = default!;

    public async Task OnGetAsync()
    {
        await LoadDataAsync();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            await LoadDataAsync();
            return Page();
        }

        // Auto-registry logic
        var employee = await _context.Employees
            .FirstOrDefaultAsync(e => e.DisplayName.ToLower() == Input.EmployeeName.ToLower());

        if (employee == null)
        {
            employee = new Employee
            {
                DisplayName = Input.EmployeeName,
                EmployeeIdNumber = Input.EmployeeIdNumber,
                Role = Input.Role,
                Department = Input.Department,
                IsActive = true
            };
            _context.Employees.Add(employee);
            await _context.SaveChangesAsync();
        }

        var request = new CertificationRequest
        {
            EmployeeId = employee.Id,
            ManagerName = Input.ManagerName,
            RequestType = Input.RequestType,
            AgencyId = Input.AgencyId,
            CustomAgencyName = Input.CustomAgencyName,
            CertificationId = Input.CertificationId,
            CustomCertificationName = Input.CustomCertificationName,
            Status = RequestStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        _context.CertificationRequests.Add(request);
        await _context.SaveChangesAsync();

        TempData["SuccessMessage"] = "Your certification request has been successfully submitted!";
        return RedirectToPage("./Index");
    }

    private async Task LoadDataAsync()
    {
        ExistingEmployeeNames = await _context.Employees
            .Where(e => e.IsActive)
            .Select(e => e.DisplayName)
            .ToListAsync();

        Agencies = new SelectList(await _context.Agencies.Where(a => a.IsActive).ToListAsync(), "Id", "Abbreviation");
        Certifications = new SelectList(await _context.Certifications.Where(c => c.IsActive).ToListAsync(), "Id", "Name");
    }

    public class CertificationRequestInput
    {
        [Required(ErrorMessage = "Employee Name is required")]
        [Display(Name = "Your Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Display(Name = "Employee ID")]
        public string? EmployeeIdNumber { get; set; }

        [Display(Name = "Role (Optional)")]
        public string? Role { get; set; }

        [Display(Name = "Department (Optional)")]
        public string? Department { get; set; }

        [Required(ErrorMessage = "Manager Name is required")]
        [Display(Name = "Manager's Name")]
        public string ManagerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Request Type is required")]
        [Display(Name = "Request Type")]
        public RequestType RequestType { get; set; }

        [Display(Name = "Agency")]
        public int? AgencyId { get; set; }

        [Display(Name = "Custom Agency (if not listed)")]
        public string? CustomAgencyName { get; set; }

        [Display(Name = "Certification")]
        public int? CertificationId { get; set; }

        [Display(Name = "Custom Certification (if not listed)")]
        public string? CustomCertificationName { get; set; }
    }
}
