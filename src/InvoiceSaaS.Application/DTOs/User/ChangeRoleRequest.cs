namespace InvoiceSaaS.Application.DTOs.User;

public class ChangeRoleRequest
{
    public List<Guid> RoleIds { get; set; } = new();
}
