namespace SeHrEmployeePortal.Services;

public interface IEmailService
{
    Task SendNewRequestNotificationAsync(string employeeName, string certificationName, string managerName);
}
