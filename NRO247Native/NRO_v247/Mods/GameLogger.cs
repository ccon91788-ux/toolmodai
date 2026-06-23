using System;

namespace NRO_v247.Mods
{
    public static class GameLogger
    {
        /// <summary>
        /// Gửi một dòng nhật ký (log) về Panel.
        /// </summary>
        /// <param name="type">Loại log (VD: SYSTEM, ITEM, BOSS, SKH_ME)</param>
        /// <param name="message">Nội dung log</param>
        public static void SendLog(string type, string message)
        {
            try
            {
                if (!string.IsNullOrEmpty(NRO_v247.AutoLogin.idClientSocket))
                {
                    NRO_v247.SocketGame.SendMessage($"GAME_LOG|{NRO_v247.AutoLogin.idClientSocket}|{type}|{message}");
                }
            }
            catch
            {
                // Bỏ qua lỗi nếu Socket lỗi
            }
        }
    }
}
