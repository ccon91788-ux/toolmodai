using System.Collections.Generic;

namespace NRO_v247.Mods
{
    internal sealed class AstarTele
    {
        private static AstarTele _instance;
        public static AstarTele gI() => _instance ?? (_instance = new AstarTele());

        private const int TileSize = 24;

        private long _teleportInterval = 30;
        private int _stepSize = 3;

        private List<Astar.Point> _currentPath;
        private int _pathStep;
        private long _lastMoveTime;

        private AstarTele() { }

        public void SetStepSize(int stepSize) => _stepSize = stepSize;
        public void SetDelay(long delayMs) => _teleportInterval = delayMs;

        public void StartMovement(Astar.Point start, Astar.Point goal)
        {
            _currentPath = Astar.FindPath(start, goal);
            if (_currentPath != null)
            {
                _pathStep = 0;
                _lastMoveTime = mSystem.currentTimeMillis();
            }
            // else
            // {
            //     // Yêu cầu: Nếu boss/mục tiêu chui vào tường (không nối đường bằng A* được) -> KHÔNG teleport
            //     // hoặc chỉ tìm điểm đứng an toàn sát tường nhất. Mục tiêu qua Astar.FindNearestWalkable đã là điểm ngoài tường.
            //     // Nhưng nếu đoạn đường bị đứt gãy tức là tường bao kín, ta KHÔNG teleport xuyên tường để tránh bị ban.
            // }
        }

        public void Update()
        {
            if (_currentPath == null) return;
            if (mSystem.currentTimeMillis() - _lastMoveTime < _teleportInterval) return;
            if (_pathStep >= _currentPath.Count)
            {
                Reset();
                return;
            }

            int actualStep = _pathStep + _stepSize;
            if (actualStep > _currentPath.Count - 1)
                actualStep = _currentPath.Count - 1;

            Astar.Point point = _currentPath[actualStep];
            TeleportToTile(point.x, point.y);
            _pathStep += _stepSize;
            _lastMoveTime = mSystem.currentTimeMillis();
        }

        private static void TeleportToTile(int tileX, int tileY)
        {
            int pixelX = tileX * TileSize;
            int pixelY = tileY * TileSize + TileSize;
            TeleportToPixel(pixelX, pixelY);
        }

        private static void TeleportToPixel(int pixelX, int pixelY)
        {
            Char me = Char.myCharz();
            if (me == null) return;

            me.currentMovePoint = null;
            me.cxSend = 0;
            me.cySend = 0;
            me.cx = pixelX;
            me.cy = pixelY;
            Service.gI()?.charMove();
        }

        public void Reset()
        {
            _currentPath = null;
            _pathStep = 0;
            _lastMoveTime = 0;
        }

        public bool IsMoving() =>
            _currentPath != null && _pathStep < _currentPath.Count;
    }
}