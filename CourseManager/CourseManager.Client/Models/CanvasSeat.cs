using CourseManager.Components.Canvas;
using CourseManager.Data;
using Microsoft.AspNetCore.Components.Web;

public enum SeatStatus
{
    Free, Occupied, Sick, Absent, Reserved, Special
}

public class CanvasSeat : CanvasDrawable
{
    public CanvasSeatData Data { get; }
    public CourseParticipant? Participant { get; private set; }
    //public Guid SeatLayoutId { get; set; }
    public List<SeatLayout>? SeatLayouts { get; set; } = new();
    // Wir speichern den Namen direkt im Drawable, damit das SVG nicht suchen muss
    public string DisplayName { get; set; } = "Leerer Platz";
    public bool IsOccupied => Data.CourseParticipantId.HasValue;

    public CanvasSeat(CanvasSeatData data)
    {
        Data = data;
        UpdateOccupantInfo(null); // Ruft die Namens-Logik auf
    }

    // Diesen hier nutzen wir, wenn wir die Schülerdaten parat haben
    public CanvasSeat(CanvasSeatData data, CourseParticipant? participant)
    {
        Data = data;
        UpdateOccupantInfo(participant);
        OccupantName = participant?.Person?.FirstName ?? "Leerer Platz";
    }

    private void UpdateOccupantInfo(CourseParticipant? participant)
    {
        // Priorität 1: Der übergebene Participant
        // Priorität 2: Der im Data-Objekt (falls vorhanden)
        var p = participant ?? Data.Participant;
        Participant = participant ?? Data.Participant;
        OccupantName = p?.Person?.FirstName ?? "Leerer Tisch";
        Color = (p != null || Data.CourseParticipantId.HasValue) ? "lightblue" : "#eeeeee";

        // Standard-Events
        OnClicked = (drawable) => { IsSelected = !IsSelected; };
    }
    // Wir nutzen die Properties aus dem Datenobjekt direkt
    public string _originalColor { get; set; } = "#ff00ff"; // Magenta als Platzhalter
    public int X => Data.PosX;
    public int Y => Data.PosY;

    private int _x;
    private int _y;
    public int Size { get; set; } = 50;
    public string SeatNumber { get; set; } = "";
    public string OccupantName { get; set; } = "Lord K";
    public double CurrentBiasScore { get; set; } = 0;
    public double CurrentColorBias {  get; set; } = 0;
    public override string SvgElement => "g";

    private SeatStatus _status = SeatStatus.Free;


    public SeatStatus Status
    {
        get => _status;
        set { _status = value; InvalidateSvgAttributes(); }
    }

    protected override Dictionary<string, object> GetBaseSvgAttributes()
    {
        var attributes = base.GetBaseSvgAttributes();
        attributes["transform"] = $"translate({Data.PosX}, {Data.PosY})";
        attributes["style"] = "cursor: move; touch-action: none;";
        attributes["pointer-events"] = "all";

        return attributes;
    }
    public override void MoveBy(int deltaX, int deltaY, SnapGrid? grid = null)
    {
        Data.PosX += deltaX;
        Data.PosY += deltaY;

        if (grid != null && grid.IsEnabled && grid.SnapToGrid)
        {
            var snappedPoint = grid.SnapPoint(new System.Drawing.PointF(Data.PosX, Data.PosY));
            Data.PosX = (int)snappedPoint.X;
            Data.PosY = (int)snappedPoint.Y;
        }
        InvalidateSvgAttributes();
    }

    public override IEnumerable<CanvasDrawable> ChildElements
    {
        get
        {
            yield return new CanvasSquareChild(Size, GetDynamicColor(), IsSelected ? SelectedColor : "#333");
            yield return new CanvasTextChild(OccupantName, "black");
            if (Data.CourseParticipantId.HasValue)
            {
                yield return new CanvasScoreTextChild(CurrentBiasScore, Size / 2 + 20);
            }
        }
    }
    private string GetDynamicColor()
    {
        if (Participant == null || !Data.CourseParticipantId.HasValue)
        {
            return "#888"; // Neutrales Grau für leere Tische
        }
        switch (CurrentColorBias)
        {
            case 7: return "#00ff00"; // Grün
            case 6: return "#85ff85"; // Dunkleres Grün
            case 5: return "#bfffbf"; // Rot
            case 4: return "#eee"; // Dunkleres Rot
            case 3: return "#ffbfbf"; // Gelb
            case 2: return "#ff9494"; // Dunkleres Gelb
            case 1: return "#ff0000"; // Hellblau
            default: return "#eee"; // Weiß
        }
    }
    public override void MoveTo(int x, int y)
    {
        _x = x;
        _y = y;
        InvalidateSvgAttributes();
    }

    internal class CanvasSquareChild : CanvasDrawable
    {
        private int _s; private string _fill; private string _stroke;
        public CanvasSquareChild(int s, string fill, string stroke) { _s = s; _fill = fill; _stroke = stroke; }
        public override string SvgElement => "rect";
        protected override Dictionary<string, object> GetBaseSvgAttributes() => new() {
            { "x", -_s/2 }, { "y", -_s/2 }, { "width", _s }, { "height", _s },
            { "fill", _fill }, { "stroke", _stroke }, { "stroke-width", "2" }, { "rx", "3" }
        };
        public override void MoveBy(int dx, int dy, SnapGrid? g) { }
        public override void MoveTo(int x, int y) { }
    }

    private class CanvasTextChild : CanvasDrawable
    {
        private string _t; private string _f;
        public CanvasTextChild(string t, string f) { _t = t; _f = f; }
        public override string SvgElement => "text";
        public override string SvgContent => _t;
        protected override Dictionary<string, object> GetBaseSvgAttributes() => new() {
            { "text-anchor", "middle" }, { "dominant-baseline", "central" },
            { "fill", _f }, { "style", "font-size: 10px; font-family: Arial; pointer-events: none;" }
        };
        public override void MoveBy(int dx, int dy, SnapGrid? g) { }
        public override void MoveTo(int x, int y) { }
    }
    private class CanvasScoreTextChild : CanvasDrawable
    {
        private double _s; private int _y;
        public CanvasScoreTextChild(double s, int y) { _s = s; _y = y; }
        public override string SvgElement => "text";
        public override string SvgContent => $"Score: {_s:F0}"; // Ganze Zahl reicht hier meist
        protected override Dictionary<string, object> GetBaseSvgAttributes() => new() {
        { "y", _y }, { "text-anchor", "middle" },
        { "style", $"font-size: 10px; font-weight: bold; fill: {(_s >= 0 ? "#28a745" : "#dc3545")};" }
    };
        public override void MoveBy(int dx, int dy, SnapGrid? g) { }
        public override void MoveTo(int x, int y) { }
    }

}
