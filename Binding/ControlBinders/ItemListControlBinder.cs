using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Utilities;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;
internal partial class ItemListControlBinder : ControlBinder, IListControlBinder
{
    public event IListControlBinder.ControlChildListChangedEventHandler ControlChildListChanged;

    public void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (_bindingConfiguration.BoundControl == null)
        {
            GD.PrintErr("OptionButtonControlBinder: BoundControl is not set");
            return;
        }

        ItemList itemList = _bindingConfiguration.BoundControl.Target as ItemList;

        // Add new items
        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            itemList.AddListItems(eventArgs.NewItems, _bindingConfiguration.Formatter);
            if (eventArgs.NewStartingIndex != itemList.ItemCount - 1)
            {
                // this is an insert event
                // we need to move the item to the correct position
                itemList.RedrawItems(_bindingConfiguration.TargetObject.Target as IList, _bindingConfiguration.Formatter);
                itemList.EmitSignal(ItemList.SignalName.ItemSelected, eventArgs.NewStartingIndex);
            }
        }

        // Replace an item
        if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
        {
            var selectedItems = itemList.GetSelectedItems();
            bool itemWasSelected = false;
            if (selectedItems.Contains(eventArgs.NewStartingIndex))
                itemWasSelected = true;

            itemList.RemoveItem(eventArgs.NewStartingIndex);
            itemList.AddListItems(eventArgs.NewItems, _bindingConfiguration.Formatter);
            itemList.RedrawItems(_bindingConfiguration.TargetObject.Target as IList, _bindingConfiguration.Formatter);

            if (itemWasSelected)
            {
                itemList.Select(eventArgs.NewStartingIndex);
                itemList.EmitSignal(ItemList.SignalName.ItemSelected, eventArgs.NewStartingIndex);
            }
        }

        // Remove items
        if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
        {
            bool itemsSelected = itemList.GetSelectedItems().Any();
            itemList.RemoveItem(eventArgs.OldStartingIndex);

            if (itemsSelected && itemList.ItemCount == 0)
            {
                itemList.DeselectAll();
            }

            if (itemList.SelectMode == ItemList.SelectModeEnum.Single)
            {
                if (itemsSelected && itemList.ItemCount > 0)
                {
                    var newIndex = eventArgs.OldStartingIndex - 1 <= 0 ? 0 : eventArgs.OldStartingIndex - 1;
                    itemList.Select(newIndex);
                    itemList.EmitSignal(ItemList.SignalName.ItemSelected, newIndex);
                }
                else
                {
                    itemList.DeselectAll();
                    itemList.EmitSignal(ItemList.SignalName.ItemSelected, -1);
                }
            }
        }

        // Move an item
        if (eventArgs.Action == NotifyCollectionChangedAction.Move)
        {
            IList items = _bindingConfiguration.TargetObject.Target as IList;
            int newIndex = eventArgs.NewStartingIndex;

            if (newIndex > items.Count - 1)
                return;

            // fake a move by updating the items
            itemList.RedrawItems(items, _bindingConfiguration.Formatter);
            itemList.UpdateSelections(newIndex, eventArgs.OldStartingIndex);
        }

        // Clear the list
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            itemList.Clear();
        }
    }

    public void OnListItemChanged(object entry)
    {
        var observableList = _bindingConfiguration.TargetObject.Target as IList;
        ItemList itemList = _bindingConfiguration.BoundControl.Target as ItemList;

        var changedIndex = observableList.IndexOf(entry);
        object convertedVal = entry;
        if (_bindingConfiguration.Formatter != null)
        {
            convertedVal = _bindingConfiguration.Formatter.FormatControl(entry, null);
        }

        if (convertedVal is ListItem listItem)
        {
            itemList.SetItemValues(changedIndex, listItem);
        }
        else
        {
            itemList.SetItemText(changedIndex, convertedVal.ToString());
        }
    }
    public override IControlBinder CreateInstance()
    {
        return new ItemListControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is ItemList;
    }

    public void OnControlChildListChanged(Control control, NotifyCollectionChangedEventArgs args)
    {
        ControlChildListChanged?.Invoke(control, args);
    }
}
