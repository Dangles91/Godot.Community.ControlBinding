namespace ControlBinding;

public abstract partial class ViewModel : ObservableObject
{
    public ObservableObject ViewModelData { get; set; }
    public abstract void BindViewModel();

    public abstract string GetScriptPath();
    public void SetViewModelData(ObservableObject data)
    {
        ViewModelData = data;
    }
    public override void _Ready()
    {
        BindViewModel();
        base._Ready();
    }
}