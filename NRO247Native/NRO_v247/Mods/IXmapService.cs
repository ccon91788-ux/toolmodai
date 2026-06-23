namespace NRO_v247.Mods
{
    public interface IXmapService
    {
        bool IsXmaping();
        void StartGoToMapFromPanel(int mapId);
        void StopFromPanel();
        string GetState();
    }
}
