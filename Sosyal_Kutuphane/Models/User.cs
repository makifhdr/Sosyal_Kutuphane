using System.ComponentModel.DataAnnotations.Schema;

namespace Sosyal_Kutuphane.Models;

public class User
{
    public int Id { get; set; }

    public string Email { get; set; }
    public string UserName { get; set; }
    public string PasswordHash { get; set; }
    
    public string? ResetToken { get; set; }
    
    public DateTime? ResetTokenExpiry { get; set; }
    
    public byte[]? Avatar { get; set; }
    
    public string? Bio { get; set; }
    
    [NotMapped]
    public List<Follow> Followers { get; set; }

    [NotMapped]
    public List<Follow> Following { get; set; }
    
    public List<UserMedia> MediaList { get; set; }
    
    public List<CustomList> CustomLists { get; set; }
}