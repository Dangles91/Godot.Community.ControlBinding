using Godot;
using Godot.Community.ControlBinding;
using Godot.Community.ControlBinding.Extensions;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;
using PropertyChanged.SourceGenerator;
using System.Collections.ObjectModel;

namespace ControlBinding;

public partial class Control : Godot.Control, IObservableObject
{
    [Notify]
    private bool _labelIsVisible = true;

    [Notify]
    private bool _isAddNewPlayerEnabled = true;

    [Notify]
    private string _longText;

    [Notify]
    private int _spinBoxValue;

    public ObservableCollection<PlayerData> PlayerDatas { get; set; } = new(){
        new PlayerData{Health = 500},
    };

    public ObservableCollection<PlayerData> PlayerDatas2 { get; set; } = new(){
        new PlayerData{Health = 500},
    };

    [Notify] private BindingMode _selectedBindingMode;
    [Notify] private ObservableCollection<string> _backinglistForTesting = new() { "Test" };
    [Notify] private string _errorMessage;
    [Notify] private string _customControlInput;

    private BindingContext bindingContext;

    public override void _Ready()
    {
        bindingContext = new BindingContext(this);

        // Bind root properties to UI        
        bindingContext.BindProperty(GetNode("%Button"), nameof(Button.Disabled), nameof(IsAddNewPlayerEnabled), formatter: new ReverseBoolValueFormatter());
        GetNode("%CodeEdit").BindProperty(bindingContext, nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);

        bindingContext.BindProperty(GetNode("%CodeEdit2"), nameof(CodeEdit.Text), nameof(LongText), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%CheckBox"), nameof(CheckBox.ButtonPressed), nameof(IsAddNewPlayerEnabled), BindingMode.TwoWay);

        bindingContext.BindProperty(GetNode("%HSlider"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%VSlider"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%SpinBox"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay)
            .AddValidator(v => (double)v > 0f ? null : "Value must be greater than 0");

        bindingContext.BindProperty(GetNode("%HScrollBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%VScrollBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%ProgressBar"), nameof(SpinBox.Value), nameof(SpinBoxValue), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%SpinboxLabel"), nameof(Label.Text), nameof(SpinBoxValue), BindingMode.OneWay);

        // Bind to SelectedPlayerData.Health        
        bindingContext.BindProperty(GetNode("%LineEdit"), nameof(LineEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.Health)}", BindingMode.TwoWay,
            new ValueFormatter()
            {
                FormatTarget = (v, p) => int.TryParse((string)v, out int value) ? value : 0,
            })
            .AddValidator(v => int.TryParse((string)v, out int value) ? null : "Health must be a number")
            .AddValidator(v => int.TryParse((string)v, out int value) && value > 0 ? null : "Health must be greater than 0")
            .AddValidationHandler((control, isValid, message) =>
            {
                (control as LineEdit).RightIcon = isValid ? null : (Texture2D)ResourceLoader.Load("uid://b5s5nstqwi4jh");
                (control as LineEdit).Modulate = new Color(1, 1, 1, 1);
            });

        // list binding
        bindingContext.BindListProperty(GetNode("%ItemList"), nameof(PlayerDatas), formatter: new PlayerDataListFormatter());
        bindingContext.BindListProperty(GetNode("%ItemList2"), nameof(PlayerDatas2), formatter: new PlayerDataListFormatter());

        bindingContext.BindProperty(GetNode("%TextEdit"), nameof(TextEdit.Text), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.OneWayToTarget, new StringToListFormatter());
        bindingContext.BindListProperty(GetNode("%ItemList3"), $"{nameof(SelectedPlayerData)}.{nameof(PlayerData.ListOfThings)}", BindingMode.TwoWay);

        bindingContext.BindEnumProperty<BindingMode>(GetNode<OptionButton>("%OptionButton"), $"{nameof(SelectedPlayerData)}.BindingMode");
        bindingContext.BindSceneList(GetNode("%VBoxContainer"), nameof(PlayerDatas), "uid://die1856ftg8w8", BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%ErrorLabel"), nameof(Visible), $"{nameof(bindingContext)}.{nameof(bindingContext.HasErrors)}", BindingMode.OneWay);
        bindingContext.BindProperty(GetNode("%ErrorLabel"), nameof(Label.Text), nameof(ErrorMessage), BindingMode.OneWay);

        // Binding a custom control
        bindingContext.BindProperty(GetNode<CustomControl>("%CustomControl"), nameof(CustomControl.Input), nameof(CustomControlInput), BindingMode.TwoWay);
        bindingContext.BindProperty(GetNode("%CustomInput"), nameof(Label.Text), nameof(CustomControlInput), BindingMode.OneWay);

        bindingContext.ControlValidationChanged += (control, propertyName, message, isValid) =>
        {
            control.Modulate = isValid ? Colors.White : Colors.Red;
            control.TooltipText = message;
            ErrorMessage = message;
        };

        // Connect some buttons to test collection modifications
        GetNode<Button>("%MoveUp").Pressed += () =>
        {
            if (SelectedPlayerData != null)
            {
                var index = PlayerDatas.IndexOf(SelectedPlayerData);
                if (index > 0)
                {
                    PlayerDatas.Move(index, index - 1);
                }
            }
        };

        GetNode<Button>("%MoveDown").Pressed += () =>
        {
            if (SelectedPlayerData != null)
            {
                var index = PlayerDatas.IndexOf(SelectedPlayerData);
                if (index >= 0 && index < PlayerDatas.Count - 1)
                {
                    PlayerDatas.Move(index, index + 1);
                }
            }
        };

        GetNode<Button>("%InsertAbove").Pressed += () =>
        {
            if (SelectedPlayerData != null)
            {
                var index = PlayerDatas.IndexOf(SelectedPlayerData);
                PlayerDatas.Insert(index, new PlayerData { Health = 200 });
            }
        };

        base._Ready();
    }

    public void _on_item_list_item_selected(int index)
    {
        if (index == -1)
            SelectedPlayerData = null;
        else
            SelectedPlayerData = PlayerDatas[index];
    }

    public void _on_Button_pressed()
    {
        PlayerDatas.Add(new PlayerData
        {
            Health = 100
        });
    }

    [Notify]
    private PlayerData _selectedPlayerData = new();

    public void _on_button_2_pressed()
    {
        var node = GetNode<ItemList>("%ItemList");
        var selectedIndexes = node.GetSelectedItems();
        if (selectedIndexes.Length > 0)
        {
            foreach (var selectedIndex in selectedIndexes)
            {
                var item = PlayerDatas[selectedIndex];
                PlayerDatas.Remove(item);
                PlayerDatas2.Add(item);

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
                var item = PlayerDatas2[selectedIndex];
                PlayerDatas2.Remove(item);
                PlayerDatas.Add(item);
            }
        }
    }
}