using Unity.Collections;

namespace _Game.Battle
{
    public sealed class Matrix
    {
        private float cellSize;
        private int width;
        private int height;
        private Cell[,] matrix;

        public Matrix(int width, int height, float cellSize)
        {
            this.cellSize = cellSize;
            this.width = width;
            this.height = height;
            this.matrix = new Cell[width, height];
        }

        public void ResetTrigger()
        {
            for (var i = 0; i < width; i++)
            {
                for (var j = 0; j < height; j++)
                    matrix[i, j].trigger = false;
            }
        }
    }

    public struct Cell
    {
        public bool occupied;
        public bool trigger;
        public NativeList<int> agentList;
    }
}