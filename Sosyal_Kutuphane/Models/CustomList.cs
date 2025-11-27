namespace Sosyal_Kutuphane.Models;

public class CustomList
{
    public int Id { get; set; }

    public string Name { get; set; }

    public int UserId { get; set; }
    public User User { get; set; }

    public List<CustomListItem> Items { get; set; }
}