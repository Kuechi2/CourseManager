// Canvas/CanvasLine.cs  
using CourseManager.Components.Canvas;

public class CanvasLine : CanvasDrawable
{
    public CanvasPoint Start { get; set; }
    public CanvasPoint End { get; set; }

    public override string SvgElement => "line";

    public override Dictionary<string, object> SvgAttributes
    {
        get
        {
            var attributes = base.GetBaseSvgAttributes();
            attributes["x1"] = Start.X;
            attributes["y1"] = Start.Y;
            attributes["x2"] = End.X;
            attributes["y2"] = End.Y;
            
            attributes["stroke"] = DisplayColor;
            attributes["stroke-width"] = 3;
            attributes["stroke-linecap"] = "round";
            attributes["data-id"] = Id.ToString();
            return attributes;
        } 
    }

    public override void MoveBy(int deltaX, int deltaY, SnapGrid? Grid = null)
    {
        Start.MoveBy(deltaX, deltaY, Grid);
        End.MoveBy(deltaX, deltaY, Grid);
    }
    public override void MoveTo(int X, int Y)
    {
        Start.MoveTo(X, Y);
        End.MoveTo(X, Y);
    }
    public CanvasLine()
    {
        Start = new CanvasPoint();
        End = new CanvasPoint();
    }
}