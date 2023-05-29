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

        if (eventArgs.Action == NotifyCollectionChangedAction.Add)
        {
            int i = eventArgs.NewStartingIndex;
            foreach (var addition in eventArgs.NewItems)
            {
                var sceneItem = _bindingConfiguration.SceneFormatter.Format(addition);
                _boundControl.AddChild(sceneItem);

                // list item references are cached against a node ID so they can be removed from the
                // list of children when removed from the backing ObservableList 
                addControlCacheItem(addition, sceneItem.GetInstanceId(), i);
                i++;
            }
        }

        if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
        {
            foreach (var removedItem in eventArgs.OldItems)
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
}