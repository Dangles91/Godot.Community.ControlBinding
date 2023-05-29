using Godot.Community.ControlBinding.ControlBinders;
using Godot.Community.ControlBinding.Interfaces;
using Godot.Community.ControlBinding.Services;
using Godot.Community.ControlBinding.Utilities;
using System;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;

namespace Godot.Community.ControlBinding
{
    public enum BindingStatus
    {
        Inactive,
        Active,
        Invalid
    }

    public class Binding
    {
        public event EventHandler<BindingStatus> BindingStatusChanged;

        public delegate void ValidationFailedEventHandler(Godot.Control control, string propertyName, string message);
        public event ValidationFailedEventHandler ValidationFailed;

        public delegate void ValidationSuceededEventHandler(Godot.Control control, string propertyName);
        public event ValidationSuceededEventHandler ValidationSucceeded;

        private BindingConfiguration _bindingConfiguration;
        public BindingConfiguration BindingConfiguration
        {
            get => _bindingConfiguration;
            private set => _bindingConfiguration = value;
        }

        private readonly IControlBinder _controlBinder;
        public readonly BoundPropertySetter BoundPropertySetter;

        public Binding(BindingConfiguration bindingConfiguration,
            IControlBinder controlBinder)
        {
            BindingConfiguration = bindingConfiguration;
            _controlBinder = controlBinder;
            BoundPropertySetter = new BoundPropertySetter(bindingConfiguration.Formatter, bindingConfiguration.Validators);
        }

        public BindingStatus BindingStatus { get; set; }

        public void BindControl()
        {
            if (!_controlBinder.IsBound)
                _controlBinder.BindControl(BindingConfiguration);

            if (BindingStatus != BindingStatus.Invalid)
            {
                if (!_bindingConfiguration.BoundControl.IsAlive)
                {
                    BindingStatus = BindingStatus.Invalid;
                    return;
                }

                resolveBindingPath();
                subscribeChangeEvents();
                setInitialValue();
            }
        }

        public void UnbindControl()
        {
            if (BindingConfiguration.TargetObject.Target is IObservableObject observable)
            {
                observable.PropertyChanged -= OnPropertyChanged;
            }

            if (BindingConfiguration.TargetObject.Target is INotifyCollectionChanged observable1)
            {
                observable1.CollectionChanged -= _controlBinder.OnObservableListChanged;
                foreach (var item in observable1 as IList)
                {
                    if (item is IObservableObject oItem)
                    {
                        oItem.PropertyChanged -= OnItemListChanged;
                    }
                }
            }

            foreach (var backReference in BindingConfiguration.BackReferences)
            {
                if (backReference.ObjectReference.Target is IObservableObject observableObject)
                {
                    observableObject.PropertyChanged -= OnBackReferenceChanged;
                }
            }

            if (BindingConfiguration.BoundControl.IsAlive)
            {
                (_controlBinder as ControlBinderBase).ControlValueChanged -= OnSourcePropertyChanged;
                if (BindingConfiguration.BoundControl.Target is IObservableObject observable2)
                {
                    observable2.PropertyChanged -= OnSourcePropertyChanged;
                }
            }

            BindingConfiguration.BackReferences.Clear();
        }

        private void resolveBindingPath()
        {
            var pathNodes = BindingConfiguration.Path?.Split('.');
            var targetPropertyName = pathNodes.Last();

            var pathObjects = BackReferenceFactory.GetPathObjectsAndBuildBackReferences(pathNodes.ToList(), ref _bindingConfiguration);

            var targetObject = pathObjects.Last();

            if (targetObject is not null && targetObject is not IObservableObject && targetObject is not INotifyCollectionChanged)
            {
                GD.PrintErr($"ControlBinding: Binding from node {targetObject} on path {BindingConfiguration.Path} will not update with changes. Node is not of type ObservableObject");
            }

            BindingConfiguration.TargetObject = new WeakReference(pathObjects.Last());
            BindingConfiguration.TargetPropertyName = targetPropertyName;
        }

        private void subscribeChangeEvents()
        {
            if (BindingConfiguration.BoundControl.Target is not Godot.Control)
            {
                BindingStatus = BindingStatus.Invalid;
                return;
            }

            if (BindingStatus != BindingStatus.Active && BindingConfiguration.BoundControl.Target is Node node)
            {
                node.TreeExiting += OnBoundControlTreeExiting;
            }

            if (BindingConfiguration.TargetObject.Target is IObservableObject observable)
            {
                observable.PropertyChanged += OnPropertyChanged;
            }

            if (BindingConfiguration.TargetObject.Target is INotifyCollectionChanged observable1)
            {
                observable1.CollectionChanged += OnObservableListChanged;
            }

            // Register for changes to back references to trigger rebinding
            foreach (var backReference in BindingConfiguration.BackReferences.Select(x => x.ObjectReference.Target))
            {
                if (backReference != BindingConfiguration.TargetObject.Target &&
                    backReference is IObservableObject observable3)
                {
                    observable3.PropertyChanged += OnBackReferenceChanged;
                }
            }

            if (BindingConfiguration.BoundControl.IsAlive)
            {
                (_controlBinder as ControlBinderBase).ControlValueChanged += OnSourcePropertyChanged;
                if (BindingConfiguration.BoundControl.Target is IObservableObject observable2)
                {
                    observable2.PropertyChanged += OnSourcePropertyChanged;
                }
            }

            BindingStatus = BindingStatus.Active;
        }

        private void setInitialValue()
        {
            if (BindingStatus != BindingStatus.Active)
                return;

            if (BindingConfiguration.BindingMode == BindingMode.OneWay || BindingConfiguration.BindingMode == BindingMode.TwoWay)
            {
                if (BindingConfiguration.IsListBinding)
                {
                    setInitialListValue(BindingConfiguration.TargetObject.Target);
                }
                else
                {
                    var target = BindingConfiguration.TargetObject.Target;
                    var source = BindingConfiguration.BoundControl.Target;

                    BoundPropertySetter.SetBoundControlValue(target,
                                    BindingConfiguration.TargetPropertyName,
                                    source as Godot.Control,
                                    BindingConfiguration.BoundPropertyName);
                }
            }
            else
            {
                if (!BindingConfiguration.IsListBinding)
                {
                    if (!BindingConfiguration.BoundControl.IsAlive)
                        return;

                    BoundPropertySetter.SetBoundPropertyValue(BindingConfiguration.BoundControl.Target as Godot.Control,
                        BindingConfiguration.BoundPropertyName,
                        BindingConfiguration.TargetObject.Target,
                        BindingConfiguration.TargetPropertyName);
                }
            }
        }

        private void setInitialListValue(object sender)
        {
            if (sender is INotifyCollectionChanged list)
            {
                OnObservableListChanged(sender, new(NotifyCollectionChangedAction.Reset));
                OnObservableListChanged(sender, new(NotifyCollectionChangedAction.Add, list as IList, 0));
            }
        }

        public virtual void OnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if ((BindingConfiguration.BindingMode == BindingMode.OneWay || BindingConfiguration.BindingMode == BindingMode.TwoWay)
                && e.PropertyName == BindingConfiguration.TargetPropertyName)
            {
                BoundPropertySetter.SetBoundControlValue(BindingConfiguration.TargetObject.Target,
                    BindingConfiguration.TargetPropertyName,
                    BindingConfiguration.BoundControl.Target as Godot.Control,
                    BindingConfiguration.BoundPropertyName);
            }
        }

        public void OnBackReferenceChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BindingConfiguration.BackReferences.Any(x => x.ObjectReference.Target == sender && x.PropertyName == e.PropertyName))
            {
                UnbindControl();
                BindControl();
            }
        }
        private void OnBoundControlTreeExiting()
        {
            BindingStatus = BindingStatus.Invalid;
            BindingStatusChanged?.Invoke(this, BindingStatus);
            if (_bindingConfiguration.BoundControl.Target is Node node)
            {
                node.TreeExiting -= OnBoundControlTreeExiting;
            }
            UnbindControl();
        }

        public void OnSourcePropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (BindingConfiguration.TargetObject.Target == null)
                return;

            if (BindingConfiguration.BoundPropertyName != e.PropertyName)
                return;

            if (BindingConfiguration.BindingMode == BindingMode.TwoWay || BindingConfiguration.BindingMode == BindingMode.OneWayToTarget)
            {
                try
                {
                    BoundPropertySetter.SetBoundPropertyValue(BindingConfiguration.BoundControl.Target as Godot.Control,
                            BindingConfiguration.BoundPropertyName,
                            BindingConfiguration.TargetObject.Target,
                            BindingConfiguration.TargetPropertyName);

                    OnPropertyValidationChanged(BindingConfiguration.BoundControl.Target as Godot.Control, BindingConfiguration.TargetPropertyName, true, null);
                }
                catch (ValidationException vex)
                {
                    OnPropertyValidationChanged(BindingConfiguration.BoundControl.Target as Godot.Control, BindingConfiguration.TargetPropertyName, false, vex.Message);
                }
            }
        }

        private void OnPropertyValidationChanged(Godot.Control control, string propertyName, bool isValid, string message)
        {
            if (isValid)
            {
                ValidationSucceeded?.Invoke(control, propertyName);
            }
            else
            {
                ValidationFailed?.Invoke(control, propertyName, message);
            }

            BindingConfiguration.OnValidationChangedHandler?.Invoke(control, isValid, message);
        }

        public virtual void OnObservableListChanged(object sender, NotifyCollectionChangedEventArgs eventArgs)
        {
            _controlBinder.OnObservableListChanged(sender, eventArgs);

            if (eventArgs.Action == NotifyCollectionChangedAction.Add)
            {
                foreach (var item in eventArgs.NewItems)
                {
                    if (item is IObservableObject observableObject)
                    {
                        observableObject.PropertyChanged += OnItemListChanged;
                    }
                }
            }

            if (eventArgs.Action == NotifyCollectionChangedAction.Remove)
            {
                foreach (var item in eventArgs.OldItems)
                {
                    if (item is IObservableObject observableObject)
                    {
                        observableObject.PropertyChanged -= OnItemListChanged;
                    }
                }
            }
        }
        private void OnItemListChanged(object sender, PropertyChangedEventArgs e)
        {
            _controlBinder.OnListItemChanged(sender);
        }
    }
}