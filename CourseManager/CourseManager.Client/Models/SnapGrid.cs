using System.Drawing;

public class SnapGrid
{
    public float GridSize { get; set; } = 30f;
    public bool IsEnabled { get; set; } = true;
    public bool SnapToGrid { get; set; } = true;
    public bool ShowGrid { get; set; } = true;
    public SnapGrid(float gridSize = 30)
    {
        GridSize = gridSize;
    }
    public PointF SnapPoint(PointF point)
    {
        if (!SnapToGrid || GridSize <= 0)
            return point;

        float snappedX = (float)Math.Round(point.X / GridSize) * GridSize;
        float snappedY = (float)Math.Round(point.Y / GridSize) * GridSize;

        return new PointF(snappedX, snappedY);
    }

    public float SnapValue(float value)
    {
        if (!SnapToGrid || GridSize <= 0)
            return value;

        return (float)Math.Round(value / GridSize) * GridSize;
    }

    public RectangleF SnapRectangle(RectangleF rect)
    {
        if (!SnapToGrid)
            return rect;

        PointF snappedLocation = SnapPoint(rect.Location);
        PointF snappedEnd = SnapPoint(new PointF(rect.Right, rect.Bottom));

        return new RectangleF(
            snappedLocation.X,
            snappedLocation.Y,
            snappedEnd.X - snappedLocation.X,
            snappedEnd.Y - snappedLocation.Y
        );
    }

    public IEnumerable<PointF> GenerateGridLines(RectangleF viewport, float zoom)
    {
        if (!ShowGrid || GridSize <= 0)
            yield break;

        float effectiveGridSize = GridSize * zoom;
        float startX = (float)Math.Floor(viewport.Left / effectiveGridSize) * effectiveGridSize;
        float startY = (float)Math.Floor(viewport.Top / effectiveGridSize) * effectiveGridSize;

        // Vertikale Linien
        for (float x = startX; x <= viewport.Right; x += effectiveGridSize)
        {
            yield return new PointF(x, viewport.Top);
            yield return new PointF(x, viewport.Bottom);
        }

        // Horizontale Linien
        for (float y = startY; y <= viewport.Bottom; y += effectiveGridSize)
        {
            yield return new PointF(viewport.Left, y);
            yield return new PointF(viewport.Right, y);
        }
    }
}