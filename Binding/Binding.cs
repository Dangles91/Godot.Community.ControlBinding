using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.ControlBinders;
using Godot.Community.ControlBinding.EventArgs;
using Godot.Community.ControlBinding.Interfaces;
using Godot.Community.ControlBinding.Services;
using Godot.Community.ControlBinding.Utilities;
using System;
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

            resolveBindingPath();
            subscribeChangeEvents();
            setInitialValue();
        }

        private void resolveBindingPath()
        {
            var pathNodes = BindingConfiguration.Path?.Split('.');
            var targetPropertyName = pathNodes.Last();

            var pathObjects = BackReferenceFactory.GetPathObjectsAndBuildBackReferences(pathNodes.ToList(), ref _bindingConfiguration);

            var targetObject = pathObjects.Last();

            if (targetObject is not null && targetObject is not IObservableObject && targetObject is not IObservableList)
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

            if (BindingStatus != BindingStatus.Active)
                (BindingConfiguration.BoundControl.Target as Godot.Control).TreeExiting += OnBoundControlTreeExiting;

            if (BindingConfiguration.TargetObject.Target is IObservableObject observable)
            {
                observable.PropertyChanged += OnPropertyChanged;
            }

            if (BindingConfiguration.TargetObject.Target is IObservableList observable1)
            {
                observable1.ObservableListChanged += OnObservableListChanged;
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
                    BoundPropertySetter.SetBoundControlValue(BindingConfiguration.TargetObject.Target,
                                    BindingConfiguration.TargetPropertyName,
                                    BindingConfiguration.BoundControl.Target as Godot.Control,
                                    BindingConfiguration.BoundPropertyName);
                }
            }
            else
            {
                if (!BindingConfiguration.IsListBinding)
                {
                    BoundPropertySetter.SetBoundPropertyValue(BindingConfiguration.BoundControl.Target as Godot.Control,
                        BindingConfiguration.BoundPropertyName,
                        BindingConfiguration.TargetObject.Target,
                        BindingConfiguration.TargetPropertyName);
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

        public virtual void OnPropertyChanged(object sender, string propertyName)
        {
            if ((BindingConfiguration.BindingMode == BindingMode.OneWay || BindingConfiguration.BindingMode == BindingMode.TwoWay)
                && propertyName == BindingConfiguration.TargetPropertyName)
            {
                BoundPropertySetter.SetBoundControlValue(BindingConfiguration.TargetObject.Target,
                    BindingConfiguration.TargetPropertyName,
                    BindingConfiguration.BoundControl.Target as Godot.Control,
                    BindingConfiguration.BoundPropertyName);
            }
        }

        public void OnBackReferenceChanged(object sender, string propertyName)
        {
            if (BindingConfiguration.BackReferences.Any(x => x.ObjectReference.Target == sender && x.PropertyName == propertyName))
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
            if (BindingConfiguration.TargetObject.Target == null)
                return;

            if (BindingConfiguration.BoundPropertyName != propertyName)
                return;

            if (BindingConfiguration.BindingMode == BindingMode.TwoWay || BindingConfiguration.BindingMode == BindingMode.OneWayToTarget)
            {
                try
                {
                    BoundPropertySetter.SetBoundPropertyValue(BindingConfiguration.BoundControl.Target as Godot.Control,
                            BindingConfiguration.BoundPropertyName,
                            BindingConfiguration.TargetObject.Target,
                            BindingConfiguration.TargetPropertyName);
                    BindingConfiguration.Owner.OnPropertyValidationSuceeded(BindingConfiguration.BoundControl.Target as Godot.Control, BindingConfiguration.TargetPropertyName);
                }
                catch (ValidationException vex)
                {
                    BindingConfiguration.Owner.OnPropertyValidationFailed(BindingConfiguration.BoundControl.Target as Godot.Control, BindingConfiguration.TargetPropertyName, vex.Message);
                }
            }
        }

        public virtual void OnObservableListChanged(ObservableListChangedEventArgs eventArgs)
        {
            _controlBinder.OnObservableListChanged(eventArgs);

            if (eventArgs.ChangeType == ObservableListChangeType.Add)
            {
                foreach (var item in eventArgs.ChangedEntries)
                {
                    if (item is IObservableObject observableObject)
                    {
                        observableObject.PropertyChanged += OnItemListChanged;
                    }
                }
            }

            if (eventArgs.ChangeType == ObservableListChangeType.Remove)
            {
                foreach (var item in eventArgs.ChangedEntries)
                {
                    if (item is IObservableObject observableObject)
                    {
                        observableObject.PropertyChanged -= OnItemListChanged;
                    }
                }
            }
        }

        private void OnItemListChanged(object sender, string propertyName)
        {
            _controlBinder.OnListItemChanged(sender);
        }

        public void UnbindControl()
        {
            if (BindingConfiguration.TargetObject.Target is IObservableObject observable)
            {
                observable.PropertyChanged -= OnPropertyChanged;
            }

            if (BindingConfiguration.TargetObject.Target is IObservableList observable1)
            {
                observable1.ObservableListChanged -= _controlBinder.OnObservableListChanged;
                foreach (var item in observable1.GetBackingList())
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
            }

            BindingConfiguration.BackReferences.Clear();
        }
    }
}