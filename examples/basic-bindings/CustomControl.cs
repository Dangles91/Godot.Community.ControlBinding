using Godot;
using Godot.Community.ControlBinding;
using Godot.Community.ControlBinding.Extensions;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;
using PropertyChanged.SourceGenerator;
using System;

namespace ControlBinding;

public partial class CustomControl : Control, IObservableObject
{
    [Notify]
    private string _input;

    private BindingContext _bindingContext;

    public override void _Ready()
    {
        _bindingContext = new BindingContext(this);

        GetNode<LineEdit>("%LineEdit").BindProperty(_bindingContext, nameof(LineEdit.Text), nameof(Input), BindingMode.OneWayToTarget,
            new ValueFormatter
            {
                FormatTarget = (v, _) => $"Input: {v}",
            });
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
