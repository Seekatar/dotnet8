namespace dotnet8.Models;

// alias for any type now
using WidgetId = System.Int32;

// primary constructor means name, id required. name & id are in scope for the class.
// and can be used to initialize properties.
public class Widget(string name, WidgetId id)
{
    public int WidgetId => id;
    public string Name => name;

    // The parameterless constructor is emitted if a primary constructor is defined.
    // (struct types still get a parameterless constructor)
    // Other constructors must call the primary constructor.
    public Widget() : this("Default Widget", 0)
    {
    }

    public void DoIt2() {
        var x = (WidgetId id = 0) => id;
    }
}