using ControlBinding.Binding;
using Godot;

public partial class PlayerData : ObservableObject
{
    private int health;
    public int Health
    {
        get { return health; }
        set { health = value; OnPropertyChanged(nameof(Health)); }
    }        

    private BindingMode bindingMode;
    public BindingMode BindingMode
    {
        get { return bindingMode; }
        set { bindingMode = value; OnPropertyChanged(nameof(BindingMode));}
    }
    
}