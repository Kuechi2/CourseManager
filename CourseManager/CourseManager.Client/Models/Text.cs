using CourseManager.Components.Canvas;
using System.Collections.Generic;

public class CanvasText : CanvasDrawable
{
    public CanvasPoint Center { get; set; }
    public string Text { get; set; } = "<LEEEEEEER>";
    public string FontFamily { get; set; } = "Arial";
    public int FontSize { get; set; } = 10;
    public string TextAnchor { get; set; } = "middle"; // "start", "middle", "end"
    public string DominantBaseline { get; set; } = "central"; // "central", "middle", "hanging"
    public string FillColor { get; set; } = "black";

    public override string SvgElement => "text";

    public override Dictionary<string, object> SvgAttributes
    {
        get
        {
            var attributes = base.GetBaseSvgAttributes();
            attributes["x"] = Center.X;
            attributes["y"] = Center.Y;
            attributes["font-family"] = FontFamily;
            attributes["font-size"] = FontSize;
            attributes["text-anchor"] = TextAnchor;
            attributes["dominant-baseline"] = DominantBaseline;
            attributes["fill"] = FillColor;
            return attributes;
        }
    }
    public override string SvgContent => Text;

    public override void MoveBy(int deltaX, int deltaY, SnapGrid? grid = null)
    {
        Center.MoveBy(deltaX, deltaY, grid);
    }

    public override void MoveTo(int x, int y)
    {
        Center.MoveTo(x, y);
    }

    public CanvasText(string caption = "LEER")
    {

        FillColor = Color; // Synchronisiere mit der Basis Color-Eigenschaft
        Text = caption;
        Center = new CanvasPoint();
    }

}