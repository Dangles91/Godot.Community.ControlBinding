using ControlBinding.Collections;
using ControlBinding.Formatters;
using Godot;
using System.Collections.Generic;

namespace ControlBinding;

public partial class Control : ObservableObject
{
    private bool labelIsVisible = true;
    public bool IsAddNewPlayerEnabled
    {
        get { return labelIsVisible; }
        set { labelIsVisible = value; OnPropertyChanged(nameof(IsAddNewPlayerEnabled)); }
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

    public ObservableList<PlayerData> playerDatas {get;set;} = new(){
        new PlayerData{Health = 500},
    };

    public ObservableList<PlayerData> playerDatas2 {get;set;} = new(){
        new PlayerData{Health = 500},
    };

    private BindingMode _selectedBindingMode;
    public BindingMode SelectedBindingMode
    {
        get { return _selectedBindingMode; }
        set { _selectedBindingMode = value; OnPropertyChanged(nameof(SelectedBindingMode)); }
    }    

    private ObservableList<string> _backinglistForTesting = new ObservableList<string>{"Test"};
    public ObservableList<string> BackingListForTesting
    {
        get { return _backinglistForTesting; }
        set { _backinglistForTesting = value; OnPropertyChanged(nameof(BackingListForTesting));}
    }

    public override void _Ready()
    {
        // Bind root properties to UI        
        BindProperty("%Button", nameof(Button.Disabled), nameof(IsAddNewPlayerEnabled), formatter: new ReverseBoolValueFormatter());
        BindProperty("%CheckBox", nameof(CheckBox.ButtonPressed), nameof(IsAddNewPlayerEnabled), BindingMode.TwoWay);        
        BindProperty("%CodeEdit", nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);
        BindProperty("%CodeEdit2", nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);

        BindProperty("%HSlider", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%VSlider", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%SpinBox", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%HScrollBar", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%VScrollBar", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%ProgressBar", nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        BindProperty("%SpinboxLabel", nameof(Label.Text), nameof(SpinBoxValue), BindingMode.OneWay);

        // Bind to SelectedPlayerData.Health        
        BindProperty("%LineEdit", nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay);

        // list binding
        BindListProperty("%ItemList", nameof(playerDatas), formatter: new PlayerDataListFormatter());
        BindListProperty("%ItemList2", nameof(playerDatas2), formatter: new PlayerDataListFormatter());

        BindProperty("%TextEdit", nameof(TextEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.OneWayToTarget, new StringToListFormatter());
        BindListProperty("%ItemList3", $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.TwoWay);

        BindEnumProperty<BindingMode>("%OptionButton", $"{nameof(SelectedPlayerData)}.{nameof(SelectedPlayerData.BindingMode)}");
        

        base._Ready();
    }

    public void _on_item_list_item_selected(int index)
    {
        if (index == -1)
            selectedPlayerData = null;
        else
            SelectedPlayerData = playerDatas[index];
    }

    public void _on_Button_pressed()
    {        
        playerDatas.Add(new PlayerData
        {
            Health = 100
        });        
    }

    private PlayerData selectedPlayerData = new();
    public PlayerData SelectedPlayerData
    {
        get { return selectedPlayerData; }
        set { selectedPlayerData = value; OnPropertyChanged(nameof(SelectedPlayerData)); }
    }

    public void _on_button_2_pressed()
    {
        var node = GetNode<ItemList>("%ItemList");        
        var selectedIndexes = node.GetSelectedItems();
        if (selectedIndexes.Length > 0)
        {
            foreach (var selectedIndex in selectedIndexes)
            {
                var item = playerDatas[selectedIndex];
                playerDatas.Remove(item);
                playerDatas2.Add(item);

                if (SelectedPlayerData == item)
                    SelectedPlayerData = null;
            }
        }
    }

    public void _on_button_3_pressed()
    {
        var node = GetNode<ItemList>("%ItemList2");
        var selectedIndexes = node.GetSelectedItems();
        if (selectedIndexes.Length > 0)
        {
            foreach (var selectedIndex in selectedIndexes)
            {
                var item = playerDatas2[selectedIndex];
                playerDatas2.Remove(item);
                playerDatas.Add(item);
            }
        }
    }
}
