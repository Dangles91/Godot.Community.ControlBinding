using ControlBinding.Collections;
using ControlBinding.EventArgs;
using Godot;
using System.Collections.Generic;
using System.Linq;

namespace ControlBinding.ControlBinders;
public partial class OptionButtonControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoBindingProperties = new List<string>(){
            "Selected"
        };

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if (IsBound)
            return;

        if ((bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(bindingConfiguration.BoundPropertyName))
        {
            OptionButton boundControl = bindingConfiguration.BoundControl.Target as OptionButton;

            if (bindingConfiguration.BoundPropertyName == "Selected")
            {
                boundControl.ItemSelected += OnItemSelected;
            }
        }

        base.BindControl(bindingConfiguration);
    }

    public void OnItemSelected(long selectedValue)
    {
        EmitSignal(nameof(ControlValueChanged), _bindingConfiguration.BoundControl.Target as GodotObject, "Selected");
    }

    public override void ClearEventBindings()
    {
        if ((_bindingConfiguration.BindingMode == BindingMode.OneWayToTarget || _bindingConfiguration.BindingMode == BindingMode.TwoWay)
            && _allowedTwoBindingProperties.Contains(_bindingConfiguration.BoundPropertyName))
        {
            OptionButton boundControl = _bindingConfiguration.BoundControl.Target as OptionButton;

            if (_bindingConfiguration.BoundPropertyName == "Selected")
                boundControl.ItemSelected -= OnItemSelected;
        }
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        if (_bindingConfiguration.BoundControl == null)
        {
            GD.PrintErr("OptionButtonControlBinder: BoundControl is not set");
            return;
        }

        OptionButton optionButton = (OptionButton)_bindingConfiguration.BoundControl.Target;
        List<object> convertedValues = eventArgs.ChangedEntries.ToList();
        if (_bindingConfiguration.Formatter != null)
        {
            convertedValues = eventArgs.ChangedEntries.Select(x => _bindingConfiguration.Formatter.FormatControl(x)).ToList();
        }

        if (eventArgs.ChangeType == ObservableListChangeType.Add)
        {
            foreach (var item in convertedValues)
            {
                if (item is string stringValue)
                {
                    optionButton.AddItem(stringValue);
                }

                if (item is ListItem listItem)
                {
                    optionButton.AddItem(listItem.DisplayValue);
                    SetItemValues(optionButton, optionButton.ItemCount - 1, listItem);
                }

                if (optionButton.ItemCount == 1)
                {
                    optionButton.Select(0);
                }
                else
                {
                    optionButton.Select(optionButton.Selected);
                }
            }
        }

        if (eventArgs.ChangeType == ObservableListChangeType.Remove)
        {
            bool itemsSelected = optionButton.Selected != -1;
            optionButton.RemoveItem(eventArgs.Index);
            optionButton.Select(-1);

            if (itemsSelected && optionButton.ItemCount > 0)
            {
                var newIndex = eventArgs.Index -1 <= 0 ? 0 : eventArgs.Index -1;
                optionButton.Select(newIndex);
                optionButton.EmitSignal(OptionButton.SignalName.ItemSelected, newIndex);
            }
            else
            {
                optionButton.Select(-1);
                optionButton.EmitSignal(OptionButton.SignalName.ItemSelected, -1);
            }
        }

        if (eventArgs.ChangeType == ObservableListChangeType.Clear)
        {
            optionButton.Clear();
        }
    }

    public override void OnListItemChanged(object entry)
    {
        var observableList = _bindingConfiguration.TargetObject as IObservableList;
        OptionButton itemList = _bindingConfiguration.BoundControl.Target as OptionButton;

        var listItems = observableList.GetBackingList();

        var changedIndex = listItems.IndexOf(entry);
        object convertedVal = entry;
        if (_bindingConfiguration.Formatter != null)
        {
            convertedVal = _bindingConfiguration.Formatter.FormatControl(entry);
        }

        if (convertedVal is ListItem listItem)
        {
            SetItemValues(itemList, changedIndex, listItem);
        }
        else
        {
            itemList.SetItemText(changedIndex, convertedVal.ToString());
        }
    }

    private void SetItemValues(OptionButton optionButton, int index, ListItem listItem)
    {
        optionButton.SetItemText(index, listItem.DisplayValue);
        if (listItem.Icon != null)
            optionButton.SetItemIcon(index, listItem.Icon);
        if (listItem.Id != -1)
            optionButton.SetItemId(index, listItem.Id);
        if (listItem.Disabled.HasValue)
            optionButton.SetItemDisabled(index, listItem.Disabled.Value);
        if (listItem.Metadata.VariantType != Variant.Type.Nil)
            optionButton.SetItemMetadata(index, listItem.Metadata);
    }

    public override IControlBinder CreateInstance()
    {
        return new OptionButtonControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is OptionButton;
    }
}