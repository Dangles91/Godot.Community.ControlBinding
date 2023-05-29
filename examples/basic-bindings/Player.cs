using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding;
using System.Collections.ObjectModel;

namespace ControlBinding;
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
        set { bindingMode = value; OnPropertyChanged(nameof(BindingMode)); }
    }

    private ObservableCollection<string> _listOfThings;
    public ObservableCollection<string> ListOfThings
    {
        get { return _listOfThings; }
        set { _listOfThings = value; OnPropertyChanged(nameof(ListOfThings));}
    }
}