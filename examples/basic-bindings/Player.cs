using ControlBinding.Collections;

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

    private ObservableList<string> _listOfThings;
    public ObservableList<string> ListOfThings
    {
        get { return _listOfThings; }
        set { _listOfThings = value; OnPropertyChanged(nameof(ListOfThings));}
    }
}