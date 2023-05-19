using Godot.Community.ControlBinding.EventArgs;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders;

public partial class GenericControlBinder : ControlBinderBase
{
    public new static int Priority => 0;
    internal Godot.Control _boundControl;
    private readonly Dictionary<object, ulong> _controlChildCache = new();

    public override bool CanBindFor(object control)
    {
        return control is Godot.Control;
    }

    public override void ClearEventBindings()
    {
        throw new NotImplementedException();
    }

    public override IControlBinder CreateInstance()
    {
        return new GenericControlBinder();
    }

    public override void OnListItemChanged(object entry)
    {
        // do nothing
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {
        // this should only be used to manage control children using a scene formatter            
        if (_bindingConfiguration.SceneFormatter == null)
        {
            return;
        }

        if (_boundControl == null)
            _boundControl = _bindingConfiguration.BoundControl.Target as Godot.Control;


        if (eventArgs.ChangeType == ObservableListChangeType.Add)
        {
            foreach (var addition in eventArgs.ChangedEntries)
            {
                var sceneItem = _bindingConfiguration.SceneFormatter.Format(addition);
                _boundControl.AddChild(sceneItem);

                // list item references are cached against a node ID so they can be removed from the
                // list of children when removed from the backing ObservableList 
                _controlChildCache.Add(addition, sceneItem.GetInstanceId());
            }
        }

        if (eventArgs.ChangeType == ObservableListChangeType.Remove)
        {
            foreach (var removedItem in eventArgs.ChangedEntries)
            {
                // get the corresponding scene item
                var instanceId = _controlChildCache.GetValueOrDefault(removedItem);
                var sceneItem = _boundControl.GetChildren().FirstOrDefault(x => x.GetInstanceId() == instanceId);
                if (sceneItem != null)
                {
                    _boundControl.RemoveChild(sceneItem);
                    sceneItem.QueueFree();
                }
                _controlChildCache.Remove(removedItem);
            }
        }

        if (eventArgs.ChangeType == ObservableListChangeType.Clear)
        {
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