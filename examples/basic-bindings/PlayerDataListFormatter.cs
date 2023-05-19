using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Formatters;
using Godot;
using System;

namespace ControlBinding;

public class PlayerDataListFormatter : IValueFormatter
{
    public Func<object, object> FormatControl => (v) =>
    {
        var pData = v as PlayerData;
        var listItem = new ListItem
        {
            DisplayValue = $"Health: {pData.Health}",
            Icon = ResourceLoader.Load<Texture2D>("uid://bfdb75li0y86u"),
            Disabled = pData.Health < 1,
            Tooltip = pData.Health == 0 ? "Health must be greater than 0" : null,

        };
        return listItem;
    };

    public Func<object, object> FormatTarget => throw new NotImplementedException();
}