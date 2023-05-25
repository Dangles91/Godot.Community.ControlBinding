namespace Godot.Community.ControlBinding.Collections;
/// <summary>
/// A ListItem can be used to format items in an ItemList control when bound to a list.
/// Return a ListItem from an <see cref="ControlBinding.Formatters.IValueFormatter" /> to format the list item.
/// </summary>
public class ListItem
{
    public string DisplayValue { get; set; }
    public int Id { get; set; } = -1;
    public Texture2D Icon { get; set; }

    public string ScenePath { get; set; }
    public string Tooltip { get; set; }
    public Variant Metadata { get; set; }
    public bool? Disabled { get; set; }

    // Background color is not stateful. If it is set it will not be automatically reset.
    // NOTE: This would be possible if a ListItem backing is stored in cache and re-referenced. 
    // However, ItemList doesn't support getting the Godot default colors, only the custom colors.
    public Color? BackgroundColor { get; set; }

    // Background color is not stateful. If it is set it will not be automatically reset.
    public Color? ForegroundColor { get; set; }
    public string Language { get; set; }
    public Rect2? IconRegion { get; set; }
    public Color? IconModulate { get; set; }
    public bool? Selectable { get; set; }
    public Godot.Control.TextDirection TextDirection { get; set; }
    public bool? IconTransposed { get; set; }
}
