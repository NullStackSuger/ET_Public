namespace ET.Client;

public readonly struct SDF
{
    public struct Point
    {
        //dx,dy表示对于当前点的偏移值
        private int dx;
        private int dy;

        public int DistancePow2() { return dx*dx + dy*dy; }
        
        public Point(int dx, int dy) { this.dx = dx; this.dy = dy; }

        public static Point Zero => new Point(0, 0);
        public static Point Max => new Point(int.MaxValue, int.MaxValue);
        
        public void Push(int xOffset, int yOffset) { dx += xOffset; dy += yOffset; }
    };
    
    public readonly struct Grid
    {
        public readonly Point[,] grid;

        public Grid(int width, int height)
        {
            grid = new Point[width, height];
        }
        
        public Point Get(int x, int y)
        {
            if (x >= 0 && y >= 0 && x < grid.GetLength(0) && y < grid.GetLength(1) )
                return grid[x, y];
            return Point.Max;
        }

        public void Set(int x, int y, Point point)
        {
            grid[x, y] = point;
        }

        public void Compare(int x, int y, int xOffset, int yOffset)
        {
            Point self = Get(x, y);
            Point other = Get(x+xOffset, y+yOffset);
            other.Push(xOffset, yOffset);

            if (other.DistancePow2() < self.DistancePow2())
            {
                Set(x, y, other);
            }
        }
    };
    
    private readonly Grid gridInside;
    private readonly Grid gridOutside;

    public SDF(int width, int height)
    {
        gridInside = new Grid(width, height);
        gridOutside = new Grid(width, height);

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                if (x == 50) // TODO 读取图片像素颜色
                {
                    gridInside.Set(x, y, Point.Zero);
                    gridOutside.Set(x, y, Point.Max);
                }
                else
                {
                    gridInside.Set(x, y, Point.Max);
                    gridOutside.Set(x, y, Point.Zero);
                }
            }
        }

        GenerateSDF(gridInside);
        GenerateSDF(gridOutside);
    }

    private static void GenerateSDF(Grid grid)
    {
        for (int y = 0; y < grid.grid.GetLength(1); y++)
        {
            for (int x = 0; x < grid.grid.GetLength(0); x++)
            {
                grid.Compare(x, y, -1,  0);
                grid.Compare(x, y,  0, -1);
                grid.Compare(x, y, -1, -1);
                grid.Compare(x, y,  1, -1);
            }
            
            for (int x = grid.grid.GetLength(0)-1; x >= 0; x--)
            {
                grid.Compare(x, y, 1, 0);
            }
        }

        for (int y = grid.grid.GetLength(1) - 1; y >= 0; y--)
        {
            for (int x = grid.grid.GetLength(0) - 1; x >= 0; x--)
            {
                grid.Compare(x, y,  1,  0);
                grid.Compare(x, y,  0,  1);
                grid.Compare(x, y, -1,  1);
                grid.Compare(x, y,  1,  1);
            }
            
            for (int x = 0; x < grid.grid.GetLength(0); x++)
            {
                grid.Compare(x, y, -1, 0);
            }
        }
    }

    public float Distance(int x, int y)
    {
        float distInside = MathF.Sqrt(gridInside.Get(x, y).DistancePow2());
        float distOutside = MathF.Sqrt(gridOutside.Get(x, y).DistancePow2());
        
        return distInside - distOutside;
    }
}