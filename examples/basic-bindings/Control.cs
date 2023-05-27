using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Formatters;
using Godot;
using Godot.Community.ControlBinding;
using Godot.Community.ControlBinding.Extensions;

namespace ControlBinding;

public partial class Control : ControlViewModel
{
    private bool labelIsVisible = true;
    public bool IsAddNewPlayerEnabled
    {
        get { return labelIsVisible; }
        set { this.SetValue(ref labelIsVisible, value); }
    }

    private string longText;
    public string LongText
    {
        get { return longText; }
        set { this.SetValue(ref longText, value); }
    }

    private int spinBoxValue;
    public int SpinBoxValue
    {
        get { return spinBoxValue; }
        set { this.SetValue(ref spinBoxValue, value); }
    }

    public ObservableList<PlayerData> playerDatas { get; set; } = new(){
        new PlayerData{Health = 500},
    };

    public ObservableList<PlayerData> playerDatas2 { get; set; } = new(){
        new PlayerData{Health = 500},
    };

    private BindingMode _selectedBindingMode;
    public BindingMode SelectedBindingMode
    {
        get { return _selectedBindingMode; }
        set { this.SetValue(ref _selectedBindingMode, value); }
    }

    private ObservableList<string> _backinglistForTesting = new() { "Test" };
    public ObservableList<string> BackingListForTesting
    {
        get { return _backinglistForTesting; }
        set { this.SetValue(ref _backinglistForTesting, value); }
    }

    private string errorMessage;
    public string ErrorMessage
    {
        get { return errorMessage; }
        set { this.SetValue(ref errorMessage, value); }
    }

    BindingContainer bindingContainer {get; set;}

    public override void _Ready()
    {
        bindingContainer = new BindingContainer(this);

        // Bind root properties to UI        
        bindingContainer.BindProperty(GetNode("%Button"), nameof(Button.Disabled), nameof(IsAddNewPlayerEnabled), formatter: new ReverseBoolValueFormatter());
        GetNode("%CodeEdit").BindProperty(bindingContainer, nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);

        bindingContainer.BindProperty(GetNode("%CodeEdit2"), nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%CheckBox"), nameof(CheckBox.ButtonPressed), nameof(IsAddNewPlayerEnabled), BindingMode.TwoWay);

        bindingContainer.BindProperty(GetNode("%HSlider"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%VSlider"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%SpinBox"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay)
            .AddValidator(v => (double)v > 0f ? null : "Value must be greater than 0");

        bindingContainer.BindProperty(GetNode("%HScrollBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%VScrollBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%ProgressBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%SpinboxLabel"), nameof(Label.Text), nameof(SpinBoxValue), BindingMode.OneWay);

        // Bind to SelectedPlayerData.Health        
        bindingContainer.BindProperty(GetNode("%LineEdit"), nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay,
            new ValueFormatter()
            {
                FormatTarget = (v, p) => int.TryParse((string)v, out int value) ? value : 0,
            })
            .AddValidator(v => int.TryParse((string)v, out int value) ? null : "Health must be a number")
            .AddValidator(v => int.TryParse((string)v, out int value) && value > 0 ? null : "Health must be greater than 0")
            .AddValidationHandler((control, isValid, message) => { 
                (control as LineEdit).RightIcon = isValid ? null : (Texture2D)ResourceLoader.Load("uid://b5s5nstqwi4jh");
                (control as LineEdit).Modulate = new Color(1, 1, 1, 1) ;
            });

        // list binding
        bindingContainer.BindListProperty(GetNode("%ItemList"), nameof(playerDatas), formatter: new PlayerDataListFormatter());
        bindingContainer.BindListProperty(GetNode("%ItemList2"), nameof(playerDatas2), formatter: new PlayerDataListFormatter());

        bindingContainer.BindProperty(GetNode("%TextEdit"), nameof(TextEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.OneWayToTarget, new StringToListFormatter());
        bindingContainer.BindListProperty(GetNode("%ItemList3"), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.TwoWay);

        bindingContainer.BindEnumProperty<BindingMode>(GetNode<OptionButton>("%OptionButton"), $"{nameof(SelectedPlayerData)}.BindingMode");
        bindingContainer.BindSceneList(GetNode("%VBoxContainer"), nameof(playerDatas), "uid://die1856ftg8w8", BindingMode.TwoWay);
        bindingContainer.BindProperty(GetNode("%ErrorLabel"), nameof(Visible), $"{nameof(bindingContainer)}.{nameof(bindingContainer.HasErrors)}", BindingMode.OneWay);
        bindingContainer.BindProperty(GetNode("%ErrorLabel"), nameof(Label.Text), nameof(ErrorMessage), BindingMode.OneWay);

        bindingContainer.ControlValidationChanged += (control, propertyName, message, isValid) =>
        {
            control.Modulate = isValid ? Colors.White : Colors.Red;
            control.TooltipText = message;
            ErrorMessage = message;
        };

        base._Ready();
    }

    public void _on_item_list_item_selected(int index)
    {
        if (index == -1)
            SelectedPlayerData = null;
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
        set { this.SetValue(ref selectedPlayerData, value); }
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

    public override void SetViewModelData(object viewModelData)
    {
        throw new System.NotImplementedException();
    }
}
