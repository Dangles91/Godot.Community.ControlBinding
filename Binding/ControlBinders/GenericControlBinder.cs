using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;

public partial class GenericControlBinder : ControlBinderBase
{
    public new static int Priority => 0;
    internal Godot.Control _boundControl;
    private readonly Dictionary<object, ulong> _controlChildCache = new();
    private readonly Dictionary<ulong, object> _controlChildCacheReverseLookup = new();
    private readonly Dictionary<ulong, int> _controlChildIndexes = new();

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        if (bindingConfiguration.BoundControl.Target is Godot.Control controlInstance)
            _boundControl = controlInstance;

        if (bindingConfiguration.IsListBinding && bindingConfiguration.BindingMode == BindingMode.TwoWay)
        {
            _boundControl.ChildExitingTree += OnChildExitingTree;
        }
        base.BindControl(bindingConfiguration);
    }

    private void OnChildExitingTree(Node node)
    {
        if (_controlChildCacheReverseLookup.TryGetValue(node.GetInstanceId(), out var listItem))
        {
            int itemIndex = _controlChildIndexes[node.GetInstanceId()];
            removeControlCacheItem(listItem, node.GetInstanceId());
            (_bindingConfiguration.TargetObject.Target as IList)?.RemoveAt(itemIndex);
        }
    }

    private void addControlCacheItem(object listItem, ulong sceneInstanceId, int index)
    {
        _controlChildCache.Add(listItem, sceneInstanceId);
        _controlChildCacheReverseLookup.Add(sceneInstanceId, listItem);
        _controlChildIndexes.Add(sceneInstanceId, index);
    }

    private void removeControlCacheItem(object listItem, ulong sceneInstanceId)
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

    private void insertControlCacheItem(ulong sceneInstanceId, int newIndex)
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

    private void moveControlCacheItem(ulong sceneInstanceId, int newIndex)
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

    public void clearControlCache()
    {
        _controlChildCache.Clear();
        _controlChildCacheReverseLookup.Clear();
        _controlChildIndexes.Clear();
    }

    public override bool CanBindFor(object control)
    {
        return control is Godot.Control;
    }

    public override void ClearEventBindings()
    {
        if (_bindingConfiguration.BindingMode == BindingMode.TwoWay)
        {
            _boundControl.ChildExitingTree -= OnChildExitingTree;
        }
    }

    public override IControlBinder CreateInstance()
    {
        return new GenericControlBinder();
    }

    public override void OnListItemChanged(object entry)
    {
        // do nothing
    }

    public override void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
    {
        // this should only be used to manage control children using a scene formatter            
        if (_bindingConfiguration.SceneFormatter == null)
        {
            return;
        }

        // Add new child items        
        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            AddItems(eventArgs.NewItems, eventArgs.NewStartingIndex);

            if (eventArgs.NewStartingIndex != _boundControl.GetChildCount() - 1)
            {
                // this is an insert event
                // we need to move the item to the correct position
                var item = _boundControl.GetChild(_boundControl.GetChildCount() - 1);
                _boundControl.MoveChild(item, eventArgs.NewStartingIndex);
                insertControlCacheItem(item.GetInstanceId(), eventArgs.NewStartingIndex);
            }
        }

        // Remove child items
        if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
        {
            RemoveItems(eventArgs.OldItems, eventArgs.OldStartingIndex);
        }

        // Replace a child item        
        if (eventArgs.Action == NotifyCollectionChangedAction.Replace)
        {
            RemoveItems(eventArgs.OldItems, eventArgs.OldStartingIndex);
            var newSceneItems = AddItems(eventArgs.NewItems, eventArgs.NewStartingIndex);

            _boundControl.MoveChild(newSceneItems[0], eventArgs.NewStartingIndex);
            moveControlCacheItem(newSceneItems[0].GetInstanceId(), eventArgs.NewStartingIndex);
        }

        // Move a child item
        if (eventArgs.Action == NotifyCollectionChangedAction.Move)
        {
            int oldIndex = eventArgs.OldStartingIndex;
            int newIndex = eventArgs.NewStartingIndex;
            for (int i = 0; i < eventArgs.NewItems.Count; i++)
            {
                var item = _boundControl.GetChild(oldIndex);
                _boundControl.MoveChild(item, newIndex);
                moveControlCacheItem(item.GetInstanceId(), newIndex);
                oldIndex++; newIndex++;
            }
        }

        // Clear all child items
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            clearControlCache();
            if (_boundControl != null)
            {
                foreach (var child in _boundControl.GetChildren())
                {
                    _boundControl.RemoveChild(child);
                    child.QueueFree();
                }
            }
            _controlChildCache.Clear();
        }
    }

    private List<Node> AddItems(IList newItems, int newIndex)
    {
        int i = newIndex;
        var newScenes = new List<Node>();
        foreach (var addition in newItems)
        {
            var sceneItem = _bindingConfiguration.SceneFormatter.Format(addition);
            _boundControl.AddChild(sceneItem);
            newScenes.Add(sceneItem);
            // list item references are cached against a node ID so they can be removed from the
            // list of children when removed from the backing ObservableList 
            addControlCacheItem(addition, sceneItem.GetInstanceId(), i);
            i++;
        }
        return newScenes;
    }

    private void RemoveItems(IList oldItems, int OldStartingIndex)
    {
        foreach (var removedItem in oldItems)
        {
            // get the corresponding scene item
            if (!_controlChildCache.TryGetValue(removedItem, out var instanceId))
            {
                return;
            }
            removeControlCacheItem(removedItem, instanceId);
            var sceneItem = _boundControl.GetChildren().FirstOrDefault(x => x.GetInstanceId() == instanceId);
            if (sceneItem != null)
            {
                _boundControl.RemoveChild(sceneItem);
                sceneItem.QueueFree();
            }
        }
    }
}