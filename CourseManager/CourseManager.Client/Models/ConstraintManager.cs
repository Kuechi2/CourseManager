using CourseManager.Components.Canvas;

public class SimpleConstraintManager
{
    private List<GeometryConstraint> _constraints = new();

    public void AddConstraint(GeometryConstraint constraint)
    {
        _constraints.Add(constraint);
    }

    public void ApplyAllConstraints()
    {
        bool anythingChanged = true;

        foreach (var constraint in _constraints)
        {
            constraint.Enforce();
            anythingChanged = true; 
        }

        if (anythingChanged)
        {
            //CanvasManager.Instance.OnDrawablesChanged();
        }
    }

    public void ApplyConstraintsToObject(CanvasDrawable movedObject)
    {
        foreach (var constraint in _constraints)
        {
            if (constraint.GetInvolvedObjects().Contains(movedObject))
            {
                constraint.Enforce();
            }
        }
    }
}