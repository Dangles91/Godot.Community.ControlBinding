using Godot.Community.ControlBinding.Collections;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;
public partial class OptionButtonControlBinder : ControlBinderBase
{
    private readonly List<string> _allowedTwoBindingProperties = new(){
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
        OnControlValueChanged(_bindingConfiguration.BoundControl.Target as Godot.Control, "Selected");
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

    public override void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (_bindingConfiguration.BoundControl == null)
        {
            GD.PrintErr("OptionButtonControlBinder: BoundControl is not set");
            return;
        }

        OptionButton optionButton = (OptionButton)_bindingConfiguration.BoundControl.Target;

        // Add new items
        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            AddListItems(optionButton, eventArgs.NewItems, eventArgs.NewStartingIndex);
            if (eventArgs.NewStartingIndex != optionButton.ItemCount - 1)
            {
                // this is an insert event
                // we need to move the item to the correct position
                RedrawListItems();
                optionButton.EmitSignal(ItemList.SignalName.ItemSelected, eventArgs.NewStartingIndex);
            }
        }

        // Replace an existing item
        if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
        {
            bool itemWasSelected = optionButton.Selected == eventArgs.NewStartingIndex;

            optionButton.RemoveItem(eventArgs.NewStartingIndex);
            AddListItems(optionButton, eventArgs.NewItems, eventArgs.NewStartingIndex);

            if (itemWasSelected)
            {
                optionButton.Select(eventArgs.NewStartingIndex);
                optionButton.EmitSignal(ItemList.SignalName.ItemSelected, eventArgs.NewStartingIndex);
            }
        }

        // Remove an item
        if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
        {
            bool itemsSelected = optionButton.Selected != -1;
            optionButton.RemoveItem(eventArgs.OldStartingIndex);
            optionButton.Select(-1);

            if (itemsSelected && optionButton.ItemCount > 0)
            {
                var newIndex = eventArgs.OldStartingIndex - 1 <= 0 ? 0 : eventArgs.OldStartingIndex - 1;
                optionButton.Select(newIndex);
                optionButton.EmitSignal(OptionButton.SignalName.ItemSelected, newIndex);
            }
            else
            {
                optionButton.Select(-1);
                optionButton.EmitSignal(OptionButton.SignalName.ItemSelected, -1);
            }
        }


        // Move an item
        if (eventArgs.Action == NotifyCollectionChangedAction.Move)
        {
            IList items = _bindingConfiguration.TargetObject.Target as IList;
            int newIndex = eventArgs.NewStartingIndex;

            if(newIndex > items.Count -1)
                return;

            // fake a move by updating the items?
            RedrawListItems();
            UpdateSelections(newIndex, eventArgs.OldStartingIndex);
        }

        // Clear the list
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            optionButton.Clear();
        }
    }

    public override void OnListItemChanged(object entry)
    {
        var observableList = _bindingConfiguration.TargetObject.Target as IList;
        OptionButton itemList = _bindingConfiguration.BoundControl.Target as OptionButton;

        var changedIndex = observableList.IndexOf(entry);
        object convertedVal = entry;
        if (_bindingConfiguration.Formatter != null)
        {
            convertedVal = _bindingConfiguration.Formatter.FormatControl(entry, null);
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

    private static void SetItemValues(OptionButton optionButton, int index, ListItem listItem)
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

    private void AddListItems(OptionButton optionButton, IList newItems, int newIndex)
    {
        List<object> convertedValues = newItems.Cast<object>().ToList();
        if (_bindingConfiguration.Formatter != null)
        {
            convertedValues = convertedValues.ConvertAll(x => _bindingConfiguration.Formatter.FormatControl(x, null));
        }

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

    private void UpdateSelections(int newIndex, int oldIndex)
    {
        OptionButton optionButton = _bindingConfiguration.BoundControl.Target as OptionButton;
        for (int i = 0; i < optionButton.ItemCount; i++)
        {
            bool isSelected = optionButton.Selected == i;
            if (!isSelected)
                continue;

            if (i >= oldIndex && i < newIndex)
            {
                optionButton.Select(i + 1);
                optionButton.EmitSignal(ItemList.SignalName.ItemSelected, i + 1);
            }
            else if (i > newIndex && i <= oldIndex)
            {
                optionButton.Select(i - 1);
                optionButton.EmitSignal(ItemList.SignalName.ItemSelected, i - 1);
            }
        }
    }

    private void RedrawListItems()
    {
        IList items = _bindingConfiguration.TargetObject.Target as IList;
        OptionButton optionButton = _bindingConfiguration.BoundControl.Target as OptionButton;
        // fake a move by updating the items?
        for (int i = 0; i < items.Count; i++)
        {
            object item = string.Empty;
            if (_bindingConfiguration.Formatter != null)
            {
                item = _bindingConfiguration.Formatter.FormatControl(items[i], null);
            }
            else
            {
                item = items[i].ToString();
            }

            if (item is string stringValue)
                optionButton.SetItemText(i, stringValue);

            if (item is ListItem listItem)
            {
                SetItemValues(optionButton, i, listItem);
            }
        }
    }
}