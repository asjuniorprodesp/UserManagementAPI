using System.ComponentModel.DataAnnotations;

namespace UserManagementUI.Models;

public class UserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class UserFormModel
{
    public int? UserId { get; set; }

    [Required(ErrorMessage = "Informe o nome completo.")]
    [StringLength(150, MinimumLength = 2, ErrorMessage = "O nome deve ter entre 2 e 150 caracteres.")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o e-mail.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail valido.")]
    [StringLength(150, ErrorMessage = "O e-mail deve ter no maximo 150 caracteres.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o departamento.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O departamento deve ter entre 2 e 100 caracteres.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Informe o cargo.")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "O cargo deve ter entre 2 e 100 caracteres.")]
    public string Role { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public static UserFormModel FromUser(UserDto user) => new()
    {
        UserId = user.UserId,
        FullName = user.FullName,
        Email = user.Email,
        Department = user.Department,
        Role = user.Role,
        IsActive = user.IsActive
    };

    public UserRequest ToRequest() => new()
    {
        FullName = FullName.Trim(),
        Email = Email.Trim(),
        Department = Department.Trim(),
        Role = Role.Trim(),
        IsActive = IsActive
    };
}

public class UserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Department { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public bool IsActive { get; set; }
}

public class ApiProblemDetails
{
    public string? Title { get; set; }
    public string? Detail { get; set; }
    public Dictionary<string, string[]>? Errors { get; set; }
}