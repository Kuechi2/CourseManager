// Canvas/CanvasPoint.cs
using CourseManager.Components.Canvas;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Drawing;

public class CanvasPoint : CanvasDrawable
{
    
    public int Radius { get; set; } = 8;
    public int X { get; set; }
    public int Y { get; set; }
    public override string SvgElement => "circle";

    public override Dictionary<string, object> SvgAttributes
    {
        get
        {
            var attributes = base.GetBaseSvgAttributes();
        
            attributes["cx"] = X;
            attributes["cy"] = Y;
            attributes["r"] = Radius;
            attributes["fill"] = DisplayColor;
            attributes["stroke"] = "black";
            attributes["stroke-width"] = 1;
        
            return attributes;
        }
    }
    public override void MoveBy(int deltaX, int deltaY, SnapGrid? Grid = null)
    {
        X += deltaX;
        Y += deltaY;
        if (Grid != null && Grid.IsEnabled && Grid.SnapToGrid)
        {
            X = (int)Grid.SnapValue(X);
            Y = (int)Grid.SnapValue(Y);
        }
    }
    public override void MoveTo(int X, int Y)
    {
        this.X = X;
        this.Y = Y;
    }

}