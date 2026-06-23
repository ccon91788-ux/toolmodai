namespace Panel.Models;

public class ServerInfo
{
    public int Id { get; set; }
    public string DisplayName { get; set; } = "";

    public string IP { get; set; } = "";
    public int Port { get; set; }

    public override string ToString() => DisplayName;

    public static ServerInfo[] All
    {
        get
        {
            try
            {
                if (Panel.Services.SecureDataStorage.LoadKey() == "nrovoz")
                {
                    return new[]
                    {
                        new ServerInfo { Id = 1, DisplayName = "NRSD", IP = "server.ngocrongsaoden.com", Port = 14445 }
                    };
                }
            }
            catch { }

            return new[]
            {
                new ServerInfo { Id = 1, DisplayName = "Vũ trụ 1", IP = "dragon1.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 2, DisplayName = "Vũ trụ 2", IP = "dragon2.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 3, DisplayName = "Vũ trụ 3", IP = "dragon3.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 4, DisplayName = "Vũ trụ 4", IP = "dragon4.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 5, DisplayName = "Vũ trụ 5", IP = "dragon5.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 6, DisplayName = "Vũ trụ 6", IP = "dragon6.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 7, DisplayName = "Vũ trụ 7", IP = "dragon7.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 8, DisplayName = "Vũ trụ 8", IP = "dragon10.teamobi.com", Port = 14446 },
                new ServerInfo { Id = 9, DisplayName = "Vũ trụ 9", IP = "dragon10.teamobi.com", Port = 14447 },
                new ServerInfo { Id = 10, DisplayName = "Vũ trụ 10", IP = "dragon10.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 11, DisplayName = "Vũ trụ 11", IP = "dragon11.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 12, DisplayName = "Vũ trụ 12", IP = "dragon12.teamobi.com", Port = 14445 },
                new ServerInfo { Id = 13, DisplayName = "Võ đài liên vũ trụ", IP = "dragonwar.teamobi.com", Port = 20000 },
                new ServerInfo { Id = 14, DisplayName = "Universe1", IP = "dragon.indonaga.com", Port = 14445 },
                new ServerInfo { Id = 15, DisplayName = "Naga", IP = "dragon.indonaga.com", Port = 14446 },
                new ServerInfo { Id = 16, DisplayName = "Super 1", IP = "dragon11.teamobi.com", Port = 14446 },
                new ServerInfo { Id = 17, DisplayName = "Super 2", IP = "dragonsuper.teamobi.com", Port = 17001 },
                new ServerInfo { Id = 18, DisplayName = "Vũ trụ 13", IP = "dragon13.teamobi.com", Port = 14446 },
                new ServerInfo { Id = 19, DisplayName = "VIP 2", IP = "dragon11.teamobi.com", Port = 18001 },
                new ServerInfo { Id = 20, DisplayName = "Vũ trụ 14", IP = "dragon14.teamobi.com", Port = 18001 },
                new ServerInfo { Id = 21, DisplayName = "Super 3", IP = "dragonsuper3.teamobi.com", Port = 17001 },
            };
        }
    }

    /// <summary>Lấy ID từ tên hiển thị. Trả về -1 nếu không tìm thấy.</summary>
    public static int GetId(string displayName)
    {
        foreach (var s in All)
            if (s.DisplayName == displayName) return s.Id;
        return -1;
    }

    /// <summary>Lấy tên hiển thị từ ID. Trả về chuỗi ID nếu không tìm thấy.</summary>
    public static string GetDisplayName(int id)
    {
        foreach (var s in All)
            if (s.Id == id) return s.DisplayName;
        return id.ToString();
    }
}

