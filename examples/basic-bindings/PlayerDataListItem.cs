using ControlBinding;
using Godot;
using System;

public partial class PlayerDataListItem : ObservableObject
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
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
