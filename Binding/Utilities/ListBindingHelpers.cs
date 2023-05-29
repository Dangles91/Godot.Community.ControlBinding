using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Formatters;

namespace Godot.Community.ControlBinding.Utilities
{
    internal static class ListBindingHelper
    {
        # region ItemList
        public static void RedrawItems(this ItemList itemList, IList items, IValueFormatter formatter)
        {
            // fake a move by updating the items?
            for (int i = 0; i < items.Count; i++)
            {
                object item = string.Empty;
                if (formatter != null)
                {
                    item = formatter.FormatControl(items[i], null);
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

        public static void SetItemValues(this ItemList itemList, int index, ListItem listItem)
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

        public static void AddListItems(this ItemList itemList, IList items, IValueFormatter formatter)
        {
            var convertedValues = items.Cast<object>().ToList();
            if (formatter != null)
            {
                convertedValues = convertedValues.ConvertAll(x => formatter.FormatControl(x, null));
            }
            foreach (var item in convertedValues)
            {
                if (item is string stringValue)
                    itemList.AddItem(stringValue);

                if (item is ListItem listItem)
                {
                    itemList.AddItem(listItem.DisplayValue);
                    itemList.SetItemValues(itemList.ItemCount - 1, listItem);
                }
            }
        }

        public static void UpdateSelections(this ItemList itemList, int newIndex, int oldIndex)
        {
            for (int i = 0; i < itemList.ItemCount; i++)
            {
                bool isSelected = itemList.IsSelected(i);
                if (!isSelected)
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
        #endregion

        #region OptionButton
        public static void RedrawItems(this OptionButton optionButton, IList items, IValueFormatter formatter)
        {
            // fake a move by updating the items
            for (int i = 0; i < items.Count; i++)
            {
                object item = string.Empty;
                if (formatter != null)
                {
                    item = formatter.FormatControl(items[i], null);
                }
                else
                {
                    item = items[i].ToString();
                }

                if (item is string stringValue)
                    optionButton.SetItemText(i, stringValue);

                if (item is ListItem listItem)
                {
                    optionButton.SetItemValues(i, listItem);
                }
            }
        }
        public static void SetItemValues(this OptionButton optionButton, int index, ListItem listItem)
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

        public static void AddListItems(this OptionButton optionButton, IList items, IValueFormatter formatter)
        {
            List<object> convertedValues = items.Cast<object>().ToList();
            if (formatter != null)
            {
                convertedValues = convertedValues.ConvertAll(x => formatter.FormatControl(x, null));
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
                    optionButton.SetItemValues(optionButton.ItemCount - 1, listItem);
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

        public static void UpdateSelections(this OptionButton optionButton, int newIndex, int oldIndex)
        {
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
        #endregion
    }
}