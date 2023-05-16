using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Services;
using ControlBinding.Binding.EventArgs;
using ControlBinding.Binding.Interfaces;
using Godot;
using ControlBinding.Binding.ControlBinders;

namespace ControlBinding.Binding
{
    public class Binding : IDisposable
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

        public void BindControl()
        {
            if(!_controlBinder.IsBound)
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

            if (_bindingConfiguration.IsListBinding)
                pathObjects.Add(_bindingConfiguration.TargetObject);

            var targetObject = pathObjects.Last();

            if (targetObject is not ObservableObject && targetObject is not ObservableListBase)
            {
                GD.PrintErr($"Binding from node {targetObject} on path {_bindingConfiguration.Path} will not update with changes. Node is not of type ObservableObject");
            }

            _bindingConfiguration.TargetObject = (GodotObject)targetObject;
            _bindingConfiguration.TargetPropertyName = targetPropertyName;
        }

        private void subscribeChangeEvents()
        {
            // Register binding configuration to listen for OnPropertyChanged event on ObsevableObject
            if (_bindingConfiguration.TargetObject is ObservableObject observable)
            {
                observable.PropertyChanged += (s, p) => OnPropertyChanged(s, p);
            }

            if (_bindingConfiguration.TargetObject is ObservableListBase observable1)
            {
                observable1.ObservableListChanged += (e) => OnObservableListChanged(e);
            }

            // Register for changes to back references to trigger rebinding
            foreach (var backReference in _bindingConfiguration.BackReferences)
            {
                if (backReference.ObjectReference.Target != _bindingConfiguration.TargetObject &&
                    backReference.ObjectReference.Target is ObservableObject observable3)
                {                    
                    observable3.PropertyChanged += OnBackReferenceChanged;
                }
            }

            if(_bindingConfiguration.BoundControl.IsAlive)
            {
                (_controlBinder as ControlBinderBase).ControlValueChanged += OnSourcePropertyChanged;
            }
        }

        public void OnSourcePropertyChanged(GodotObject sender, string propertyName)
        {
            if (_bindingConfiguration.TargetObject == null)
                return;

            if(_bindingConfiguration.BoundPropertyName != propertyName)
                return;

            if (_bindingConfiguration.BindingMode == BindingMode.TwoWay || _bindingConfiguration.BindingMode == BindingMode.OneWayToTarget)
            {
                _boundPropertySetter.SetBoundPropertyValue(_bindingConfiguration.BoundControl.Target as Godot.Control,
                        _bindingConfiguration.BoundPropertyName,
                        _bindingConfiguration.TargetObject,
                        _bindingConfiguration.TargetPropertyName);
            }
        }

        private void setInitialValue()
        {
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

        private void setInitialListValue(GodotObject sender)
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
            if (!Node.IsInstanceValid(_bindingConfiguration.TargetObject))
                return;

            if ((_bindingConfiguration.BindingMode == BindingMode.OneWay || _bindingConfiguration.BindingMode == BindingMode.TwoWay))
            {
                if (propertyName == _bindingConfiguration.TargetPropertyName)
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
                observable.PropertyChanged -= (s, p) => OnPropertyChanged(s, p);
            }

            if (_bindingConfiguration.TargetObject is ObservableListBase observable1)
            {
                observable1.ObservableListChanged -= (e) => _controlBinder.OnObservableListChanged(e);
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


        public Godot.Control GetBoundControl()
        {
            return _bindingConfiguration?.BoundControl.Target as Godot.Control;
        }

        public GodotObject GetTargetObject()
        {
            return _bindingConfiguration?.TargetObject;
        }

        public string GetTargetPropertyName()
        {
            return _bindingConfiguration.TargetPropertyName;
        }

        public void Dispose()
        {
            try
            {
                _controlBinder.ClearEventBindings();
            }
            catch(NotImplementedException){}
            catch(Exception)
            {
                throw;
            }
        }
    }
}