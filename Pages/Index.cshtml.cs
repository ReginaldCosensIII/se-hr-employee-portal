using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using SeHrEmployeePortal.Data;
using SeHrEmployeePortal.Services;
using SeHrCertificationPortal.Models;
using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Logging;

namespace SeHrEmployeePortal.Pages;

public class IndexModel : PageModel
{
    private readonly ApplicationDbContext _context;
    private readonly IEmailService _emailService;
    private readonly ILogger<IndexModel> _logger;

    public IndexModel(ApplicationDbContext context, IEmailService emailService, ILogger<IndexModel> logger)
    {
        _context = context;
        _emailService = emailService;
        _logger = logger;
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
            RequestTypes = Input.RequestTypes,
            AgencyId = Input.AgencyId,
            CertificationId = Input.CertificationId,
            Status = RequestStatus.Pending,
            RequestDate = DateTime.UtcNow
        };

        _context.CertificationRequests.Add(request);
        await _context.SaveChangesAsync();

        try
        {
            var certName = await _context.Certifications
                .Where(c => c.Id == Input.CertificationId)
                .Select(c => c.Name)
                .FirstOrDefaultAsync() ?? "Unknown Certification";
                
            await _emailService.SendNewRequestNotificationAsync(Input.EmployeeName, certName, Input.ManagerName);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email notification for new certification request submission.");
        }

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
        [MinLength(1, ErrorMessage = "Please select at least one Request Type.")]
        [Display(Name = "Request Type")]
        public List<RequestType> RequestTypes { get; set; } = new List<RequestType>();

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
