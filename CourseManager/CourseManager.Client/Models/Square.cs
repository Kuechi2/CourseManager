using CourseManager.Components.Canvas;

public class CanvasSquare : CanvasDrawable
{
    public CanvasPoint Center { get; set; }
    public int Size { get; set; } = 20;
    public int Roundness { get; set; } = 0;

    public override string SvgElement => "rect";

    public override Dictionary<string, object> SvgAttributes
    {
        get
        {
            var attributes = base.GetBaseSvgAttributes();
            attributes["width"] = Size;
            attributes["height"] = Size;
            attributes["x"] = Center.X-Size/2;
            attributes["y"] = Center.Y-Size/2;
            attributes["fill"] = DisplayColor;
            attributes["stroke"] = "black";
            attributes["stroke-width"] = 1;
            attributes["rx"] = Roundness;
            attributes["data-id"] = Id.ToString();
            return attributes;
        }
    }

    public override void MoveBy(int deltaX, int deltaY, SnapGrid? Grid = null)
    {
        Center.MoveBy(deltaX, deltaY, Grid);
    }
    public override void MoveTo(int X, int Y)
    {
        Center.MoveTo(X, Y);
    }
    public CanvasSquare()
    {
        Center = new CanvasPoint();
    }

}
