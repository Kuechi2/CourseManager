// 1. Die Basisklasse für ALLE Constraints
using CourseManager.Components.Canvas;

public abstract class GeometryConstraint
{
    public string Name { get; set; } = "Unnamed Constraint";
    public abstract List<CanvasDrawable> GetInvolvedObjects();
    public abstract void Enforce();
}

// 2. Ein konkretes Beispiel: Der Horizontal-Constraint
public class HorizontalConstraint : GeometryConstraint
{
    public CanvasLine Line { get; set; }
    private int _targetY;

    public HorizontalConstraint(CanvasLine line)
    {
        Line = line;
        Name = "Horizontal";

        _targetY = (Line.Start.Y + Line.End.Y) / 2; 
        Enforce();
    }


    public override void Enforce()
    {
        if (Line == null) return;

        // Prüfe, welcher Punkt abweicht
        int startDiff = Math.Abs(Line.Start.Y - _targetY);
        int endDiff = Math.Abs(Line.End.Y - _targetY);

        if (startDiff > endDiff)
        {
            Line.End.Y = Line.Start.Y;
        }
        else if (endDiff > startDiff)
        {
            Line.Start.Y = Line.End.Y;
        }
        else
        {
            Line.Start.Y = Line.End.Y;
        }
        _targetY = Line.Start.Y; // Aktualisiere Zielwert
    }

    public override List<CanvasDrawable> GetInvolvedObjects()
    {
        return new List<CanvasDrawable> { Line.Start, Line.End };
    }
}
public class VerticalConstraint : GeometryConstraint
{
    public CanvasLine Line { get; set; }

    public VerticalConstraint(CanvasLine line)
    {
        Name = "Horizontal";
        Line = line;
    }

    public override List<CanvasDrawable> GetInvolvedObjects()
    {
        return new List<CanvasDrawable> { Line };
    }

    public override void Enforce()
    {
        Line.End.X = Line.Start.X;
    }
}