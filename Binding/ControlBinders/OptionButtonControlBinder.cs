using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Utilities;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;
internal partial class OptionButtonControlBinder : ControlBinderBase
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
            optionButton.AddListItems(eventArgs.NewItems, _bindingConfiguration.Formatter);
            if (eventArgs.NewStartingIndex != optionButton.ItemCount - 1)
            {
                // this is an insert event
                // we need to move the item to the correct position
                optionButton.RedrawItems(_bindingConfiguration.TargetObject.Target as IList, _bindingConfiguration.Formatter);
                optionButton.EmitSignal(ItemList.SignalName.ItemSelected, eventArgs.NewStartingIndex);
            }
        }

        // Replace an existing item
        if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
        {
            bool itemWasSelected = optionButton.Selected == eventArgs.NewStartingIndex;

            optionButton.RemoveItem(eventArgs.NewStartingIndex);
            optionButton.AddListItems(eventArgs.NewItems, _bindingConfiguration.Formatter);
            optionButton.RedrawItems(_bindingConfiguration.TargetObject.Target as IList, _bindingConfiguration.Formatter);

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

            if (newIndex > items.Count - 1)
                return;

            // fake a move by updating the items?
            optionButton.RedrawItems(items, _bindingConfiguration.Formatter);
            optionButton.UpdateSelections(newIndex, eventArgs.OldStartingIndex);
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
        OptionButton optionButton = _bindingConfiguration.BoundControl.Target as OptionButton;

        var changedIndex = observableList.IndexOf(entry);
        object convertedVal = entry;
        if (_bindingConfiguration.Formatter != null)
        {
            convertedVal = _bindingConfiguration.Formatter.FormatControl(entry, null);
        }

        if (convertedVal is ListItem listItem)
        {
            optionButton.SetItemValues(changedIndex, listItem);
        }
        else
        {
            optionButton.SetItemText(changedIndex, convertedVal.ToString());
        }
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