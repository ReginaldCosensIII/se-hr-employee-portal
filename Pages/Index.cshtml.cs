using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeHrEmployeePortal.Data;
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

    public SelectList Agencies { get; set; } = default!;
    public List<CertificationDto> AllCertifications { get; set; } = new();

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
            CertificationId = Input.CertificationId,
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
        Agencies = new SelectList(await _context.Agencies.Where(a => a.IsActive).ToListAsync(), "Id", "Abbreviation");
        AllCertifications = await _context.Certifications
            .Where(c => c.IsActive && c.AgencyId > 0)
            .Select(c => new CertificationDto { Id = c.Id, Name = c.Name, AgencyId = c.AgencyId })
            .ToListAsync();
    }

    public class CertificationRequestInput
    {
        [Required(ErrorMessage = "Employee Name is required")]
        [Display(Name = "Employee Name")]
        public string EmployeeName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Manager is required")]
        [Display(Name = "Manager")]
        public string ManagerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Request Type is required")]
        [Display(Name = "Request Type")]
        public RequestType RequestType { get; set; }

        [Required(ErrorMessage = "Agency is required")]
        [Display(Name = "Agency")]
        public int AgencyId { get; set; }

        [Required(ErrorMessage = "Certification is required")]
        [Display(Name = "Certification")]
        public int CertificationId { get; set; }
    }

    public class CertificationDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int AgencyId { get; set; }
    }
}
