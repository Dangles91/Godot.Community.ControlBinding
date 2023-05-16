using System.Collections.Generic;
using ControlBinding;
using ControlBinding.Binding;
using Godot;

public partial class Control : ObservableObject
{
    // Declare member variables here. Examples:
    // private int a = 2;
    // private string b = "text";
    // Called when the node enters the scene tree for the first time.
    private PlayerData _playerData = new PlayerData();

    private string labelText = "Default from code";
    public string LabelText
    {
        get { return labelText; }
        set { labelText = value; OnPropertyChanged(nameof(LabelText)); }
    }

    private bool labelIsVisible = true;
    public bool LabelIsVisible
    {
        get { return labelIsVisible; }
        set { labelIsVisible = value; OnPropertyChanged(nameof(LabelIsVisible)); }
    }

    private string longText;
    public string LongText
    {
        get { return longText; }
        set { longText = value; OnPropertyChanged(nameof(LongText)); }
    }

    private int spinBoxValue;
    public int SpinBoxValue
    {
        get { return spinBoxValue; }
        set { spinBoxValue = value; OnPropertyChanged(nameof(SpinBoxValue)); }
    }

    public int SelectedEnumindex { get; set; }

    public ObservableList<PlayerData> PlayerDatas = new ObservableList<PlayerData>(){
        new PlayerData{Health = 500},
    };

    public ObservableList<PlayerData> PlayerDatas2 = new ObservableList<PlayerData>(){
        new PlayerData{Health = 500},
    };

    private int selectedPlayerDataIndex;
    public int SelectedPlayerDataIndex
    {
        get { return selectedPlayerDataIndex; }
        set
        {
            selectedPlayerDataIndex = value;
            OnPropertyChanged(nameof(SelectedPlayerDataIndex));
        }
    }

    private BindingMode _selectedBindingMode;
    public BindingMode SelectedBindingMode
    {
        get { return _selectedBindingMode; }
        set { _selectedBindingMode = value; OnPropertyChanged(nameof(SelectedBindingMode)); }
    }
    
    public List<string> BackingListForTesting { get; set; }

    public override void _Ready()
    {
        // Bind root properties to UI        
        BindProperty("Label", nameof(Label.Visible), nameof(LabelIsVisible));
        BindProperty("Button", nameof(Button.Text), nameof(LabelText));
        BindProperty("CheckBox", nameof(CheckBox.ButtonPressed), nameof(LabelIsVisible), BindingMode.TwoWay);
        BindProperty("TextEdit", nameof(TextEdit.Text), nameof(BackingListForTesting), BindingMode.TwoWay, new DamageAudioFormatter());
        BindProperty("CodeEdit", nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);

        BindProperty("HSlider", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("VSlider", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("SpinBox", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("HScrollBar", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("VScrollBar", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("SpinBox/Label", nameof(Label.Text), nameof(SpinBoxValue), BindingMode.OneWay);

        // Bind to SelectedPlayerData.Health        
        BindProperty("Label2", nameof(Label.Text), $"{nameof(SelectedPlayerData)}.{nameof(SelectedPlayerData.Health)}", BindingMode.TwoWay);
        BindProperty("Label", nameof(Label.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}");
        BindProperty("LineEdit", nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay);

        // list binding
        BindListProperty<PlayerData>("ItemList", PlayerDatas, formatter: new PlayerDataListFormatter());
        BindListProperty<PlayerData>("ItemList2", PlayerDatas2, formatter: new PlayerDataListFormatter());
        
        BindEnumProperty<BindingMode>("OptionButton",$"{nameof(SelectedPlayerData)}.{nameof(SelectedPlayerData.BindingMode)}");
        BindProperty("Label3", nameof(Label.Text), $"{nameof(SelectedPlayerData)}.{nameof(SelectedPlayerData.BindingMode)}");

        base._Ready();
    }

    public void _on_item_list_item_selected(int index)
    {
        if(index == -1)
            selectedPlayerData = null;
        else
            SelectedPlayerData = PlayerDatas[index];
    }

    public void _on_Button_pressed()
    {
        LabelIsVisible ^= true;

        _playerData.Health += 20;
        if (LabelIsVisible)
        {
            PlayerDatas.Add(new PlayerData
            {
                Health = _playerData.Health
            });
        }
        else
        {
            PlayerDatas.RemoveAt(0);
        }
        SelectedPlayerData.BindingMode = BindingMode.OneWayToTarget;
    }

    private PlayerData selectedPlayerData = new();
    public PlayerData SelectedPlayerData
    {
        get { return selectedPlayerData; }
        set { selectedPlayerData = value; OnPropertyChanged(nameof(SelectedPlayerData)); }
    }

    public void _on_button_2_pressed()
    {
        var node = GetNode<ItemList>("ItemList");
        var selectedIndexes = node.GetSelectedItems();
        if (selectedIndexes.Length > 0)
        {
            foreach (var selectedIndex in selectedIndexes)
            {
                var item = PlayerDatas[selectedIndex];
                PlayerDatas.Remove(item);
                PlayerDatas2.Add(item);

                if(SelectedPlayerData == item)
                    SelectedPlayerData = null;
            }
        }        
    }

    public void _on_button_3_pressed()
    {
        var node = GetNode<ItemList>("ItemList2");
        var selectedIndexes = node.GetSelectedItems();
        if (selectedIndexes.Length > 0)
        {
            foreach (var selectedIndex in selectedIndexes)
            {
                var item = PlayerDatas2[selectedIndex];
                PlayerDatas2.Remove(item);
                PlayerDatas.Add(item);
            }
        }
    }
}
