using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Godot.Community.ControlBinding.Services;

namespace Godot.Community.ControlBinding.ControlBinders;

internal partial class GenericControlBinder : ControlBinder, IListControlBinder
{
    public new static int Priority => 0;
    internal Godot.Control _boundControl;
    private readonly NodeChildCache _nodeChildCache = new();

    public event IListControlBinder.ControlChildListChangedEventHandler ControlChildListChanged;

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
        if (_nodeChildCache.TryGetControlListValue(node.GetInstanceId(), out var listItem))
        {
            int itemIndex = _nodeChildCache.GetControlIndex(node.GetInstanceId());
            _nodeChildCache.Remove(listItem, node.GetInstanceId());
            (_bindingConfiguration.TargetObject.Target as IList)?.RemoveAt(itemIndex);
        }
    }

    public override bool CanBindFor(object control)
    {
        return control is Godot.Control;
    }

    public override IControlBinder CreateInstance()
    {
        return new GenericControlBinder();
    }

    public void OnListItemChanged(object entry)
    {
        // do nothing
    }

    public void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
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
                _nodeChildCache.Insert(item.GetInstanceId(), eventArgs.NewStartingIndex);
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
            _nodeChildCache.Move(newSceneItems[0].GetInstanceId(), eventArgs.NewStartingIndex);
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
                _nodeChildCache.Move(item.GetInstanceId(), newIndex);
                oldIndex++; newIndex++;
            }
        }

        // Clear all child items
        if (eventArgs.Action == NotifyCollectionChangedAction.Reset)
        {
            _nodeChildCache.Clear();
            if (_boundControl != null)
            {
                foreach (var child in _boundControl.GetChildren())
                {
                    _boundControl.RemoveChild(child);
                    child.QueueFree();
                }
            }
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
            _nodeChildCache.Add(addition, sceneItem.GetInstanceId(), i);
            i++;
        }
        return newScenes;
    }

    private void RemoveItems(IList oldItems, int OldStartingIndex)
    {
        foreach (var removedItem in oldItems)
        {
            // get the corresponding scene item
            if (!_nodeChildCache.TryGetListItemControlValue(removedItem, out var instanceId))
            {
                return;
            }
            _nodeChildCache.Remove(removedItem, instanceId);
            var sceneItem = _boundControl.GetChildren().FirstOrDefault(x => x.GetInstanceId() == instanceId);
            if (sceneItem != null)
            {
                _boundControl.RemoveChild(sceneItem);
                sceneItem.QueueFree();
            }
        }
    }

    public void OnControlChildListChanged(Control control, NotifyCollectionChangedEventArgs args)
    {
        ControlChildListChanged?.Invoke(control, args);
    }
}