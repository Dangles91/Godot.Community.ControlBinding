using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.EventArgs;
using System;
using System.Collections;
using System.Collections.Generic;
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

        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            var convertedValues = eventArgs.NewItems.Cast<object>().ToList();
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

        if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
        {

        }

        if (eventArgs.Action == NotifyCollectionChangedAction.Move)
        {

        }

        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            itemList.Clear();
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
}
