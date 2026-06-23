namespace Panel.Models;

public class MapTemplate
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;

    // Hiển thị chuẩn trên Combobox: "[MapID: 6] Đảo Kamê"
    public override string ToString()
    {
        return $"[{Id}] {Name}";
    }
}
