// Canvas/CanvasManager.cs (stark vereinfacht)
using CourseManager.Components.Canvas;
using Microsoft.AspNetCore.Components.Web;
using System.Numerics;

public class CanvasManager
{
    private List<CanvasDrawable> _drawables = new();
    public SnapGrid Grid { get; set; } = new(22);
    public SimpleConstraintManager _constraintManager = new();
    public CanvasDrawable? DragObject = null;
    // Events
    public event Action? DrawablesChanged;
    public event Action<string>? StatusChanged;
    public Vector2 Lmp = new();
    private float _dragStartPosX;
    private float _dragStartPosY;
    public IReadOnlyList<CanvasDrawable> Drawables => _drawables.AsReadOnly();
    public IEnumerable<CanvasDrawable> SelectedDrawables => _drawables.Where(d => d.IsSelected);

    private void StartDrag(CanvasDrawable drawable, MouseEventArgs e)
    {
        DragObject = drawable;
        Lmp.X = (int)e.OffsetX;
        Lmp.Y = (int)e.OffsetY;
        if (drawable is CanvasSeat seat)
        {
            _dragStartPosX = seat.Data.PosX;
            _dragStartPosY = seat.Data.PosY;
        }
    }
    private void EndDrag(CanvasDrawable drawable, MouseEventArgs e)     
    {
        if (DragObject == null) return;
        _constraintManager.ApplyAllConstraints();
        if(Grid!=null) drawable.MoveBy(0, 0, Grid);
        if (DragObject is CanvasSeat draggedSeat && SelectedDrawables.Count()<2)
        {
            // Wir suchen ein anderes Sitz-Objekt an der exakt gleichen (ge-snappten) Position
            var occupant = _drawables
                .OfType<CanvasSeat>()
                .FirstOrDefault(s => s != draggedSeat &&
                                s.Data.PosX == draggedSeat.Data.PosX &&
                                s.Data.PosY == draggedSeat.Data.PosY);

            if (occupant != null)
            {
                // Tausch: Der Besetzer springt dahin, wo der Dragger herkam
                occupant.Data.PosX = (int)_dragStartPosX;
                occupant.Data.PosY = (int)_dragStartPosY;

                occupant.InvalidateSvgAttributes();
                StatusChanged?.Invoke($"Plätze getauscht: {draggedSeat.OccupantName} & {occupant.OccupantName}");
            }
        }
        foreach (var selected in SelectedDrawables)
        {
            // Während des Drags kein Grid-Snap (null), damit es flüssig gleitet
            selected.MoveBy(0, 0, Grid);
        }
        DragObject = null;
        DrawablesChanged?.Invoke();
    }
    // CRUD
    public void AddDrawable(CanvasDrawable drawable)
    {
        drawable.OnDragStarted += StartDrag;
        drawable.OnDragEnd += EndDrag;
        drawable.OnRefresh += (d) => DrawablesChanged?.Invoke();
        drawable.SelectionChanged = () => DrawablesChanged?.Invoke();
        _drawables.Add(drawable);
        DrawablesChanged?.Invoke();
        StatusChanged?.Invoke($"Neu: {drawable.GetType().Name}");
    }
    public void RemoveDrawable(CanvasDrawable drawable)
    {
        _drawables.Remove(drawable);
        DrawablesChanged?.Invoke();
        StatusChanged?.Invoke($"Entfernt: {drawable.GetType().Name}");
    }

    

    public void DeselectAll()
    {
        foreach (var drawable in _drawables)
        {
            drawable.IsSelected = false;
        }
        DrawablesChanged?.Invoke();
    }


    public void AddRandomPoint()
    {
        var rnd = new Random();
        AddDrawable(new CanvasPoint
        {
            X = rnd.Next(50, 750),
            Y = rnd.Next(50, 450),
            Color = new[] { "red", "blue", "green", "orange", "purple" }[rnd.Next(5)],
        });

    }

    public void ClearAll()
    {
        _drawables.Clear();
        DrawablesChanged?.Invoke();
        StatusChanged?.Invoke("Alles gelöscht");
    }

    // Constraints
    public void AddHorizontalConstraint(CanvasLine line)
    {
        _constraintManager.AddConstraint(new HorizontalConstraint(line));
    }
}