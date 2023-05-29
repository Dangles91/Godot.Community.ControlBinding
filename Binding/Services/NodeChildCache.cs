using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Godot.Community.ControlBinding.Services
{
    internal class NodeChildCache
    {
        private readonly Dictionary<object, ulong> _controlChildCache = new();
        private readonly Dictionary<ulong, object> _controlChildCacheReverseLookup = new();
        private readonly Dictionary<ulong, int> _controlChildIndexes = new();

        public void Add(object listItem, ulong sceneInstanceId, int index)
        {
            _controlChildCache.Add(listItem, sceneInstanceId);
            _controlChildCacheReverseLookup.Add(sceneInstanceId, listItem);
            _controlChildIndexes.Add(sceneInstanceId, index);
        }

        public void Remove(object listItem, ulong sceneInstanceId)
        {
            _controlChildCache.Remove(listItem);
            _controlChildCacheReverseLookup.Remove(sceneInstanceId);
            var index = _controlChildIndexes[sceneInstanceId];
            _controlChildIndexes.Remove(sceneInstanceId);
            foreach (var itemIndex in _controlChildIndexes)
            {
                if (itemIndex.Value > index)
                {
                    _controlChildIndexes[itemIndex.Key]--;
                }
            }
        }

        public void Insert(ulong sceneInstanceId, int newIndex)
        {
            _controlChildIndexes[sceneInstanceId] = newIndex;

            foreach (var itemIndex in _controlChildIndexes)
            {
                if (itemIndex.Value >= newIndex && itemIndex.Key != sceneInstanceId)
                {
                    _controlChildIndexes[itemIndex.Key]++;
                }
            }
        }

        public void Move(ulong sceneInstanceId, int newIndex)
        {
            var oldIndex = _controlChildIndexes[sceneInstanceId];
            foreach (var itemIndex in _controlChildIndexes)
            {
                if (itemIndex.Value > oldIndex && itemIndex.Value <= newIndex)
                {
                    _controlChildIndexes[itemIndex.Key]--;
                }
                else if (itemIndex.Value > newIndex && itemIndex.Value <= oldIndex)
                {
                    _controlChildIndexes[itemIndex.Key]++;
                }
            }
            _controlChildIndexes[sceneInstanceId] = newIndex;
        }

        public void Clear()
        {
            _controlChildCache.Clear();
            _controlChildCacheReverseLookup.Clear();
            _controlChildIndexes.Clear();
        }

        public int GetControlIndex(ulong sceneInstanceId)
        {
            return _controlChildIndexes[sceneInstanceId];
        }

        public object GetControlListItem(ulong sceneInstanceId)
        {
            return _controlChildCacheReverseLookup[sceneInstanceId];
        }

        public bool TryGetControlListValue(ulong sceneInstanceId, out object listItem)
        {
            return _controlChildCacheReverseLookup.TryGetValue(sceneInstanceId, out listItem);
        }

        public bool TryGetListItemControlValue(object item, out ulong sceneInstanceId)
        {
            return _controlChildCache.TryGetValue(item, out sceneInstanceId);
        }

    }
}