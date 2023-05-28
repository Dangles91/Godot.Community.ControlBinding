using ControlBinding;
using Godot;
using Godot.Community.ControlBinding;
using Godot.Community.ControlBinding.Extensions;
using PropertyChanged.SourceGenerator;

namespace ControlBinding;

public partial class PlayerDataListItem : ControlViewModel
{
    [Notify]
    private PlayerData _viewModelData;

    public BindingContext BindingContext { get; set; }
    public override void SetViewModelData(object viewModelData)
    {
        ViewModelData = viewModelData as PlayerData;
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BindingContext = new BindingContext(this);

        GetNode<TextEdit>("%TextEdit").BindProperty(BindingContext, "Text", "ViewModelData.Health", BindingMode.TwoWay);
        GetNode<Button>("%Button").Pressed += () => this.QueueFree();
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
