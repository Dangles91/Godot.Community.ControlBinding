using System;
using System.Linq;
using ControlBinding.Services;
using Godot;
using ControlBinding.Collections;
using ControlBinding.EventArgs;
using ControlBinding.ControlBinders;
using ControlBinding.Utilities;

namespace ControlBinding
{
    public enum BindingStatus
    {
        Inactive,
        Active,
        Invalid
    }

    public class Binding
    {
        private BindingConfiguration _bindingConfiguration;
        private readonly IControlBinder _controlBinder;
        private readonly BoundPropertySetter _boundPropertySetter;

        public Binding(BindingConfiguration bindingConfiguration,
            IControlBinder controlBinder)
        {
            _bindingConfiguration = bindingConfiguration;
            _controlBinder = controlBinder;
            _boundPropertySetter = new BoundPropertySetter(bindingConfiguration.Formatter);
        }

        public BindingStatus BindingStatus { get; set; }

        public void BindControl()
        {
            if (!_controlBinder.IsBound)
                _controlBinder.BindControl(_bindingConfiguration);

            resolveBindingPath();
            subscribeChangeEvents();
            setInitialValue();
        }

        private void resolveBindingPath()
        {
            var pathNodes = _bindingConfiguration.Path?.Split('.');
            var targetPropertyName = pathNodes.Last();

            var pathObjects = BackReferenceFactory.GetPathObjectsAndBuildBackReferences(pathNodes, ref _bindingConfiguration);

            var targetObject = pathObjects.Last();

            if (targetObject is not null && targetObject is not ObservableObject && targetObject is not ObservableListBase)
            {                
                GD.PrintErr($"ControlBinding: Binding from node {targetObject} on path {_bindingConfiguration.Path} will not update with changes. Node is not of type ObservableObject");
            }

            _bindingConfiguration.TargetObject = targetObject;
            _bindingConfiguration.TargetPropertyName = targetPropertyName;
        }

        private void subscribeChangeEvents()
        {            
            if(_bindingConfiguration.BoundControl.Target is not Godot.Control)
            {
                BindingStatus = BindingStatus.Invalid;
                return;
            }
            if(BindingStatus != BindingStatus.Active)            
                (_bindingConfiguration.BoundControl.Target as Godot.Control).TreeExiting += OnBoundControlTreeExiting;

            if (_bindingConfiguration.TargetObject is ObservableObject observable)
            {
                observable.PropertyChanged += OnPropertyChanged;
            }

            if (_bindingConfiguration.TargetObject is ObservableListBase observable1)
            {
                observable1.ObservableListChanged += OnObservableListChanged;
            }

            // Register for changes to back references to trigger rebinding
            foreach (var backReference in _bindingConfiguration.BackReferences.Select(x => x.ObjectReference.Target))
            {
                if (backReference != _bindingConfiguration.TargetObject &&
                    backReference is ObservableObject observable3)
                {
                    observable3.PropertyChanged += OnBackReferenceChanged;
                }
            }            

            if (_bindingConfiguration.BoundControl.IsAlive)
            {
                (_controlBinder as ControlBinderBase).ControlValueChanged += OnSourcePropertyChanged;
            }

            BindingStatus = BindingStatus.Active;
        }

        private void setInitialValue()
        {
            if(BindingStatus != BindingStatus.Active)
                return;

            if (_bindingConfiguration.BindingMode == BindingMode.OneWay || _bindingConfiguration.BindingMode == BindingMode.TwoWay)
            {
                if (_bindingConfiguration.IsListBinding)
                    setInitialListValue(_bindingConfiguration.TargetObject);
                else
                    _boundPropertySetter.SetBoundControlValue(_bindingConfiguration.TargetObject,
                    _bindingConfiguration.TargetPropertyName,
                    _bindingConfiguration.BoundControl.Target as Godot.Control,
                    _bindingConfiguration.BoundPropertyName);
            }
            else
            {
                if (!_bindingConfiguration.IsListBinding)
                {
                    _boundPropertySetter.SetBoundPropertyValue(_bindingConfiguration.BoundControl.Target as Godot.Control,
                        _bindingConfiguration.BoundPropertyName,
                        _bindingConfiguration.TargetObject,
                        _bindingConfiguration.TargetPropertyName);
                }
            }
        }

        private void setInitialListValue(object sender)
        {
            if (sender is IObservableList list)
            {
                OnObservableListChanged(new ObservableListChangedEventArgs
                {
                    ChangeType = ObservableListChangeType.Clear,
                    ChangedEntries = list.GetBackingList(),
                    Index = 0
                });

                OnObservableListChanged(new ObservableListChangedEventArgs
                {
                    ChangeType = ObservableListChangeType.Add,
                    ChangedEntries = list.GetBackingList(),
                    Index = 0
                });
            }
        }

        public virtual void OnPropertyChanged(GodotObject sender, string propertyName)
        {
            if ((_bindingConfiguration.BindingMode == BindingMode.OneWay || _bindingConfiguration.BindingMode == BindingMode.TwoWay)
                && propertyName == _bindingConfiguration.TargetPropertyName)
            {
                _boundPropertySetter.SetBoundControlValue(_bindingConfiguration.TargetObject,
                    _bindingConfiguration.TargetPropertyName,
                    _bindingConfiguration.BoundControl.Target as Godot.Control,
                    _bindingConfiguration.BoundPropertyName);
            }
        }

        public void OnBackReferenceChanged(object sender, string propertyName)
        {
            if (_bindingConfiguration.BackReferences.Any(x => x.ObjectReference.Target == sender && x.PropertyName == propertyName))
            {
                UnbindControl();
                BindControl();
            }
        }

        private void OnBoundControlTreeExiting()
        {
            BindingStatus = BindingStatus.Invalid;
            UnbindControl();
        }
        
        public void OnSourcePropertyChanged(GodotObject sender, string propertyName)
        {
            if (_bindingConfiguration.TargetObject == null)
                return;

            if (_bindingConfiguration.BoundPropertyName != propertyName)
                return;

            if (_bindingConfiguration.BindingMode == BindingMode.TwoWay || _bindingConfiguration.BindingMode == BindingMode.OneWayToTarget)
            {
                _boundPropertySetter.SetBoundPropertyValue(_bindingConfiguration.BoundControl.Target as Godot.Control,
                        _bindingConfiguration.BoundPropertyName,
                        _bindingConfiguration.TargetObject,
                        _bindingConfiguration.TargetPropertyName);
            }
        }


        public virtual void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
        {
            _controlBinder.OnObservableListChanged(eventArgs);

            if (eventArgs.ChangeType == ObservableListChangeType.Add)
            {
                foreach (var item in eventArgs.ChangedEntries)
                {
                    if (item is ObservableObject observableObject)
                    {
                        observableObject.PropertyChanged += (s, p) => _controlBinder.OnListItemChanged(s);
                    }
                }
            }
        }

        public void UnbindControl()
        {
            if (_bindingConfiguration.TargetObject is ObservableObject observable)
            {
                observable.PropertyChanged -= OnPropertyChanged;
            }

            if (_bindingConfiguration.TargetObject is ObservableListBase observable1)
            {
                observable1.ObservableListChanged -= _controlBinder.OnObservableListChanged;
            }

            foreach (var backReference in _bindingConfiguration.BackReferences)
            {
                if (backReference.ObjectReference.Target is ObservableObject observableObject)
                {
                    observableObject.PropertyChanged -= OnBackReferenceChanged;
                }
            }
            
            _bindingConfiguration.BackReferences.Clear();
        }
    }
}