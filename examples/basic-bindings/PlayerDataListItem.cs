using ControlBinding;
using Godot;
using Godot.Community.ControlBinding;

public partial class PlayerDataListItem : ObservableNode
{

    private PlayerData ViewModelData { get; set; }

    public override void SetViewModelData(object viewModelData)
    {
        ViewModelData = viewModelData as PlayerData;
        base.SetViewModelData(viewModelData);
    }

    // Called when the node enters the scene tree for the first time.
    public override void _Ready()
    {
        BindProperty("%TextEdit", "Text", "ViewModelData.Health", BindingMode.TwoWay);
        GetNode<Button>("%Button").Pressed += () =>
        {
            this.QueueFree();
        };
        base._Ready();
    }

    // Called every frame. 'delta' is the elapsed time since the previous frame.
    public override void _Process(double delta)
    {
    }
}
