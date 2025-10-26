using Photon.Deterministic;

namespace Quantum
{
    public partial struct SimpleInput
    {
        /// <summary>
        /// 获取归一化的方向向量，基于Left/Right/Up/Down按钮输入
        /// </summary>
        public FPVector2 Direction
        {
            get
            {
                var direction = FPVector2.Zero;
                
                if (Left.IsDown)
                {
                    direction.X -= FP._1;
                }
                if (Right.IsDown)
                {
                    direction.X += FP._1;
                }
                if (Down.IsDown)
                {
                    direction.Y -= FP._1;
                }
                if (Up.IsDown)
                {
                    direction.Y += FP._1;
                }
                
                return direction.Normalized;
            }
        }

        /// <summary>
        /// 获取水平方向输入 (-1, 0, 或 1)
        /// </summary>
        public FP DirectionX
        {
            get
            {
                var x = FP._0;
                if (Left.IsDown)
                {
                    x -= FP._1;
                }
                if (Right.IsDown)
                {
                    x += FP._1;
                }
                return x;
            }
        }

        /// <summary>
        /// 获取垂直方向输入 (-1, 0, 或 1)
        /// </summary>
        public FP DirectionY
        {
            get
            {
                var y = FP._0;
                if (Down.IsDown)
                {
                    y -= FP._1;
                }
                if (Up.IsDown)
                {
                    y += FP._1;
                }
                return y;
            }
        }
    }
}