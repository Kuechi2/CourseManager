// Canvas/CanvasDrawable.cs
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System;
using System.Text;
using System.Text.Json.Serialization;

namespace CourseManager.Components.Canvas
{
    [JsonDerivedType(typeof(CanvasPoint), typeDiscriminator: "point")]
    [JsonDerivedType(typeof(CanvasLine), typeDiscriminator: "line")]
    [JsonDerivedType(typeof(CanvasLine), typeDiscriminator: "seat")]

    public abstract class CanvasDrawable
    {
        public Guid Id { get; } = Guid.NewGuid();


        // SVG-spezifische Properties
        [JsonIgnore]
        public virtual string SvgElement => ""; // "circle", "line", "path"
        private Dictionary<string, object>? _svgAttributes;
        public virtual Dictionary<string, object> SvgAttributes
        {
            get
            {
                _svgAttributes ??= GetBaseSvgAttributes();
                return _svgAttributes;
            }
        }
        internal void InvalidateSvgAttributes()
        {
            _svgAttributes = null;
        }
        public virtual string SvgContent => ""; // Für <text>, <tspan> etc.
        public bool isDragging = false;

        public Action<CanvasDrawable>? OnClicked { get; set; }
        public virtual IEnumerable<CanvasDrawable> ChildElements => Enumerable.Empty<CanvasDrawable>();
        // Basis-Properties (bleiben)
        public string Color { get; set; } = "#000000";
        public string SelectedColor { get; set; } = "#ff0000";

        [JsonIgnore]
        public string DisplayColor => IsSelected ? SelectedColor : Color;

        public bool IsVisible { get; set; } = true;
        public bool IsSelectable { get; set; } = true;

        private bool _isSelected = false;
        [JsonIgnore]
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    SelectionChanged?.Invoke();
                }
            }
        }



        // Methoden
        public abstract void MoveBy(int deltaX, int deltaY, SnapGrid? Grid = null);
        public abstract void MoveTo(int dX, int Y);
        public virtual void Select() => IsSelected = true;
        public virtual void Deselect() => IsSelected = false;

        [JsonIgnore]
        public Action<CanvasDrawable, MouseEventArgs>? OnDragStarted { get; set; }
        public Action<CanvasDrawable, MouseEventArgs>? OnDrag { get; set; }
        public Action<CanvasDrawable, MouseEventArgs>? OnDragEnd { get; set; }
        public Action<CanvasDrawable>? OnRefresh { get; set; }  //Zeichnet Canvas neu (Grafik geändert)
        public Action? SelectionChanged { get; set; }   //  Auswahl geändert - Mitteilung

        // Geschützte Methode für die gemeinsamen Attribute
        protected virtual Dictionary<string, object> GetBaseSvgAttributes()
        {
            var attributes = new Dictionary<string, object>();
            // Füge Maus-Events nur hinzu, wenn die entsprechenden Delegaten gesetzt sind
            if (OnDrag != null)
            {
                attributes["onpointermove"] = EventCallback.Factory.Create<MouseEventArgs>(
                    this, e => OnDrag(this, e));

            }

            if (OnDragEnd != null)
            {
                attributes["onpointerup"] = EventCallback.Factory.Create<MouseEventArgs>(
                    this, e => OnDragEnd(this, e));
            }

           // if (OnClicked != null)
            {
                attributes["onclick"] = EventCallback.Factory.Create(
                    this, () => OnClicked?.Invoke(this));
                attributes["onclick:stopPropagation"] = true;
                attributes["onclick:preventDefault"] = true;
            }

            if (OnDragStarted != null)
            {
                attributes["onpointerdown"] = EventCallback.Factory.Create<MouseEventArgs>(
                    this, e => OnDragStarted?.Invoke(this, e));
                attributes["onpointerdown:stopPropagation"] = true;
                attributes["onpointerdown:preventDefault"] = true;
            }
            string transitionStyle = "transition: transform 0.1s ease-out, cx 0.1s ease-out, cy 0.1s ease-out, x 0.1s ease-out, y 0.1s ease-out;";
            attributes["style"] = $"touch-action: none; cursor: pointer; {transitionStyle}";
            return attributes;
        }
        public CanvasDrawable()
        { 
            OnClicked = HandleDrawableClick;
            OnDragStarted = HandleDragStart;
            OnDragEnd = HandleDragEnd;
            
        }

        private void HandleDrawableClick(CanvasDrawable drawable)
        {
            IsSelected = !IsSelected;
            OnRefresh?.Invoke(this);
        }
        private void HandleDragStart(CanvasDrawable drawable, MouseEventArgs e)
        {
            drawable.isDragging = true;
        }
        private void HandleDragEnd(CanvasDrawable drawable, MouseEventArgs args)
        {
            if (isDragging) {
                isDragging = false;
            }
        }
        
        // SVG-Rendering
        [JsonIgnore]


        public virtual RenderFragment SvgRenderFragment => builder =>
        {
            builder.OpenElement(0, SvgElement);

            int sequence = 1;
            foreach (var attr in SvgAttributes)
            {
                // ... (deine bestehende Logik für StopPropagation / PreventDefault)
                if (attr.Key == "onpointerdown:stopPropagation" && (bool)attr.Value)
                    builder.AddEventStopPropagationAttribute(sequence++, "onpointerdown", true);
                else if (attr.Key == "onpointerdown:preventDefault" && (bool)attr.Value)
                    builder.AddEventPreventDefaultAttribute(sequence++, "onpointerdown", true);
                else if (attr.Key == "onclick:stopPropagation" && (bool)attr.Value)
                    builder.AddEventStopPropagationAttribute(sequence++, "onclick", true);
                else if (attr.Key == "onclick:preventDefault" && (bool)attr.Value)
                    builder.AddEventPreventDefaultAttribute(sequence++, "onclick", true);
                else
                    builder.AddAttribute(sequence++, attr.Key, attr.Value);
            }

            // NEU: Content hinzufügen (für <text>...)
            if (!string.IsNullOrEmpty(SvgContent))
            {
                builder.AddContent(sequence++, SvgContent);
            }

            // NEU: Rekursiv alle Kind-Elemente rendern
            foreach (var child in ChildElements)
            {
                builder.AddContent(sequence++, child.SvgRenderFragment);
            }

            builder.CloseElement();
        };
        public virtual string ToSvgString()
        {
            var sb = new StringBuilder();

            // SVG Element öffnen
            sb.Append($"<{SvgElement}");

            // Attribute hinzufügen
            foreach (var attr in GetSvgAttributesForExport())
            {
                sb.Append($" {attr.Key}=\"{EscapeAttributeValue(attr.Value)}\"");
            }

            // Element schließen oder Inhalt hinzufügen
            if (string.IsNullOrEmpty(SvgContent) && !ChildElements.Any())
            {
                sb.Append(" />");
            }
            else
            {
                sb.Append(">");

                // Inhalt hinzufügen
                if (!string.IsNullOrEmpty(SvgContent))
                {
                    sb.Append(SvgContent);
                }

                // Child-Elemente hinzufügen
                foreach (var child in ChildElements)
                {
                    sb.Append(child.ToSvgString());
                }

                // Element schließen
                sb.Append($"</{SvgElement}>");
            }

            return sb.ToString();
        }


        protected virtual Dictionary<string, object> GetSvgAttributesForExport() //Gestripped!
        {
            Dictionary<string, object> attributes = new();
            foreach (var attr in SvgAttributes)
            {
                // Events und spezielle Blazor-Attribute überspringen
                if (attr.Key.StartsWith("on") || attr.Key.EndsWith(":stopPropagation") || attr.Key.EndsWith(":preventDefault"))
                    continue;
                attributes.Add(attr.Key, attr.Value);
            }
            return attributes;
        }
        
        // Hilfsmethode: HTML/XML-Escaping
        protected string EscapeAttributeValue(object value)
        {
            if (value == null) return "";

            var str = value.ToString();
            if (string.IsNullOrEmpty(str)) return "";

            return str
                .Replace("\"", "&quot;")
                .Replace("'", "&apos;")
                .Replace("<", "&lt;")
                .Replace(">", "&gt;")
                .Replace("&", "&amp;");
        }

    }
}