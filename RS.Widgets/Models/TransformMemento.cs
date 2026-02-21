using RS.Widgets.Models;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 变换状态快照
    /// </summary>
    public class TransformMemento
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public double Angle { get; set; }

        public static TransformMemento Capture(TransformData data)
        {
            if (data == null) return null;
            return new TransformMemento
            {
                X = data.X,
                Y = data.Y,
                Width = data.Width,
                Height = data.Height,
                Angle = data.Angle
            };
        }

        public void Apply(TransformData data)
        {
            if (data == null) return;
            data.X = X;
            data.Y = Y;
            data.Width = Width;
            data.Height = Height;
            data.Angle = Angle;
        }
    }
}
