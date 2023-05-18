using Godot.Community.ControlBinding.EventArgs;
using Godot;
using System;
using System.Collections.Generic;

namespace Godot.Community.ControlBinding.ControlBinders;

public partial class GenericControlBinder : ControlBinderBase
{
    public new int Priority => 0;
    internal Godot.Control _boundControl;
    private Dictionary<object, ulong> _controlChildCache = new();

    public override void BindControl(BindingConfiguration bindingConfiguration)
    {
        base.BindControl(bindingConfiguration);
    }

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
        throw new NotImplementedException();
    }

    public override void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
    {       
        // this should only be used to manage control children using a scene formatter            
        if(_bindingConfiguration.SceneFormatter == null)
        {
            return;
        }

        if(_boundControl == null)
            _boundControl = _bindingConfiguration.BoundControl.Target as Godot.Control;

        // list item references are cached against a node ID so they can be removed from the
        // list of children when removed from the backing ObservableList 
        if(eventArgs.ChangeType == ObservableListChangeType.Add)
        {
            foreach(var addition in eventArgs.ChangedEntries)
            {
                var sceneItem = _bindingConfiguration.SceneFormatter.Format(addition);                
                _boundControl.AddChild(sceneItem);
                _controlChildCache.Add(addition, sceneItem.GetInstanceId());
            }
            
        }

        if(eventArgs.ChangeType == ObservableListChangeType.Remove)
        {
            
        }

        if(eventArgs.ChangeType == ObservableListChangeType.Clear)
        {
            
        }
    }
}