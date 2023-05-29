using Godot.Community.ControlBinding.Collections;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;
public partial class ItemListControlBinder : ControlBinderBase
{
    public override void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        if (_bindingConfiguration.BoundControl == null)
        {
            GD.PrintErr("OptionButtonControlBinder: BoundControl is not set");
            return;
        }

        ItemList itemList = _bindingConfiguration.BoundControl.Target as ItemList;

        // Add new items
        // This also handles inserts, but the index is ignored as Godot does support re-ordering of items
        // We could possibly support this by removing all items and re-adding them in the correct order
        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            AddListItems(itemList, eventArgs.NewItems, eventArgs.NewStartingIndex);
            if (eventArgs.NewStartingIndex != itemList.ItemCount - 1)
            {
                // this is an insert event
                // we need to move the item to the correct position
                RedrawListItems();
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
            AddListItems(itemList, eventArgs.NewItems, eventArgs.NewStartingIndex);

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

            if(newIndex > items.Count -1)
                return;

            // fake a move by updating the items?
            RedrawListItems();
            UpdateSelections(newIndex, eventArgs.OldStartingIndex);
        }

        // Clear the list
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            itemList.Clear();
        }
    }

    private void UpdateSelections(int newIndex, int oldIndex)
    {
        ItemList itemList = _bindingConfiguration.BoundControl.Target as ItemList;
        for(int i = 0; i < itemList.ItemCount; i++)
        {
            bool isSelected = itemList.IsSelected(i);
            if(!isSelected)
                continue;
            
            if (i >= oldIndex && i < newIndex)
            {
                itemList.Deselect(i);
                itemList.Select(i + 1);
                itemList.EmitSignal(ItemList.SignalName.ItemSelected, i + 1);
            }
            else if (i > newIndex && i <= oldIndex)
            {
                itemList.Deselect(i);
                itemList.Select(i - 1);
                itemList.EmitSignal(ItemList.SignalName.ItemSelected, i - 1);
            }
        }
    }

    private void RedrawListItems()
    {
        IList items = _bindingConfiguration.TargetObject.Target as IList;
        ItemList itemList = _bindingConfiguration.BoundControl.Target as ItemList;
        // fake a move by updating the items?
        for (int i = 0; i < items.Count; i++)
        {
            var selectedItems = itemList.GetSelectedItems();

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
                itemList.SetItemText(i, stringValue);

            if (item is ListItem listItem)
            {
                SetItemValues(itemList, i, listItem);
            }
        }
    }

    public override void OnListItemChanged(object entry)
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
            SetItemValues(itemList, changedIndex, listItem);
        }
        else
        {
            itemList.SetItemText(changedIndex, convertedVal.ToString());
        }
    }

    private static void SetItemValues(ItemList itemList, int index, ListItem listItem)
    {
        itemList.SetItemText(index, listItem.DisplayValue);
        if (listItem.Icon != null)
            itemList.SetItemIcon(index, listItem.Icon);
        if (listItem.Disabled.HasValue)
            itemList.SetItemDisabled(index, listItem.Disabled.Value);
        if (listItem.Metadata.VariantType != Variant.Type.Nil)
            itemList.SetItemMetadata(index, listItem.Metadata);
        if (listItem.BackgroundColor.HasValue)
            itemList.SetItemCustomBgColor(index, listItem.BackgroundColor.Value);
        if (listItem.ForegroundColor.HasValue)
            itemList.SetItemCustomFgColor(index, listItem.ForegroundColor.Value);
        if (!string.IsNullOrEmpty(listItem.Language))
            itemList.SetItemLanguage(index, listItem.Language);
        if (listItem.IconRegion.HasValue)
            itemList.SetItemIconRegion(index, listItem.IconRegion.Value);
        if (listItem.Selectable.HasValue)
            itemList.SetItemSelectable(index, listItem.Selectable.Value);
        if (listItem.IconTransposed.HasValue)
            itemList.SetItemIconTransposed(index, listItem.IconTransposed.Value);
        if (listItem.IconModulate.HasValue)
            itemList.SetItemIconModulate(index, listItem.IconModulate.Value);

        itemList.SetItemTooltip(index, listItem.Tooltip);
        itemList.SetItemTooltipEnabled(index, !string.IsNullOrEmpty(listItem.Tooltip));
        itemList.SetItemTextDirection(index, listItem.TextDirection); // defaults to auto    
    }

    public override IControlBinder CreateInstance()
    {
        return new ItemListControlBinder();
    }

    public override bool CanBindFor(object control)
    {
        return control is ItemList;
    }

    public override void ClearEventBindings()
    {
        throw new NotImplementedException();
    }

    private void AddListItems(ItemList itemList, IList newItems, int newIndex)
    {
        var convertedValues = newItems.Cast<object>().ToList();
        if (_bindingConfiguration.Formatter != null)
        {
            convertedValues = convertedValues.ConvertAll(x => _bindingConfiguration.Formatter.FormatControl(x, null));
        }
        foreach (var item in convertedValues)
        {
            if (item is string stringValue)
                itemList.AddItem(stringValue);

            if (item is ListItem listItem)
            {
                itemList.AddItem(listItem.DisplayValue);
                SetItemValues(itemList, itemList.ItemCount - 1, listItem);
            }
        }
    }
}
