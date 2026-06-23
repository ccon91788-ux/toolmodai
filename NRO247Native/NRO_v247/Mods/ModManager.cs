using NRO_v247.Mods.Utils;

namespace NRO_v247.Mods
{
    public static class ModManager
    {
        public static AutoMod AutoMod { get; private set; }

        static ModManager()
        {
            AutoMod = new AutoMod();
        }

        public static void Init()
        {
            ModBootstrap.Init();
        }

        public static void Update()
        {
            AutoMod.Update();
            TdltController.EndFrameUpdate();
        }

        public static void OnPaint(mGraphics g)
        {
            if (ModBootstrap.AutoPickFeature != null)
                ModBootstrap.AutoPickFeature.OnPaint(g);
        }

        public static void OnPaintOverlay(mGraphics g)
        {
            if (ModBootstrap.UIFeature != null)
                ModBootstrap.UIFeature.OnPaintOverlay(g);
        }
    }
}
