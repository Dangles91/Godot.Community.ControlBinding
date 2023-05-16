using System;
using Godot;
using System.Collections.Generic;
using System.Linq;
using ControlBinding.Binding.Utilities;
using ControlBinding.Binding.Services;
using ControlBinding.Interfaces;
using ControlBinding.Binding.Interfaces;

namespace ControlBinding.Binding
{
    public partial class ObservableObject : Node, IObservableObject
    {
        private List<Binding> _controlBindings = new List<Binding>();
        private ControlBinderProvider _controlBinderProvider = new ControlBinderProvider();
        private Timer _cleanUpTimer;

        [Signal]
        public delegate void PropertyChangedEventHandler(GodotObject owner, string propertyName);

        public override void _Ready()
        {
            startBindingCleanupTimer();
            base._Ready();
        }

        public void OnPropertyChanged(string name)
        {
            EmitSignal(SignalName.PropertyChanged, this, name);

            lock (_controlBindings)
            {
                var bindings = _controlBindings.Where(
                    x => x.GetTargetObject() == this && x.GetTargetPropertyName() == name);

                foreach (var binding in bindings)
                {
                    if (IsInstanceValid(binding.GetBoundControl()) && IsInstanceValid(binding.GetTargetObject()))
                        binding.OnPropertyChanged(this, name);
                }
            }
        }

        public void BindProperty(
            string controlPath,
            string sourceProperty,
            string path,
            BindingMode bindingMode = BindingMode.OneWay,
            IValueFormatter formatter = null
            )
        {
            var node = GetNode<Godot.Control>(controlPath);
            if (node == null)
            {
                GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
                return;
            }

            if (_controlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = bindingMode,
                    BoundPropertyName = sourceProperty,
                    Path = path,
                    BoundControl = new WeakReference(node),
                    Formatter = formatter,
                    Owner = this,
                };

                var binding = new Binding(bindingConfiguration, binder);
                binding.BindControl();
                _controlBindings.Add(binding);
            }
        }

        public void BindListProperty<T>(
            string controlPath,
            ObservableListBase targetObject,
            BindingMode bindingMode = BindingMode.OneWay,
            IValueFormatter formatter = null)
        {
            var node = GetNode<Godot.Control>(controlPath);
            if (node == null)
            {
                GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
                return;
            }

            if (_controlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = bindingMode,
                    TargetObject = targetObject,
                    BoundControl = new WeakReference(node),
                    Formatter = formatter,
                    IsListBinding = true,
                    Owner = this,
                    Path = string.Empty
                };

                var binding = new Binding(bindingConfiguration, binder);
                binding.BindControl();
                _controlBindings.Add(binding);
            }
        }

        public void BindEnumProperty<T>(string controlPath, string selectedItemPath = null) where T : Enum
        {
            var node = GetNode<Godot.Control>(controlPath);
            if (node == null)
            {
                GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
                return;
            }

            if (node is not OptionButton)
            {
                GD.PrintErr($"DataBinding: Enum property binding must be backed by an OptionButton");
                return;
            }

            ObservableList<T> targetObject = new ObservableList<T>();
            foreach (var entry in Enum.GetValues(typeof(T)))
            {
                targetObject.Add((T)entry);
            }

            // bind the list items (static binding)
            if (_controlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = BindingMode.OneWay,
                    TargetObject = targetObject,
                    BoundControl = new WeakReference(node),
                    IsListBinding = true,
                    Path = string.Empty,
                    Formatter = new ValueFormatter
                    {
                        FormatControl = (v) =>
                        {
                            var enumValue = (T)v;
                            return new ListItem
                            {
                                DisplayValue = enumValue.ToString(),
                                Id = (int)Enum.Parse(typeof(T), v.ToString())
                            };
                        }
                    }
                };

                var binding = new Binding(bindingConfiguration, binder);
                binding.BindControl();
                _controlBindings.Add(binding);
            }
            if (!string.IsNullOrEmpty(selectedItemPath))
            {
                BindProperty(controlPath, "Selected", selectedItemPath, BindingMode.TwoWay, new ValueFormatter
                {
                    FormatTarget = (v) => {                        
                        return targetObject[(int)v];
                    },
                    FormatControl = (v) => {                        
                        return targetObject.IndexOf((T)v);
                    }
                });
            }
        }

        #region Private methods
        private void startBindingCleanupTimer()
        {
            if (_cleanUpTimer == null || !IsInstanceValid(_cleanUpTimer))
            {
                _cleanUpTimer = new Timer()
                {
                    WaitTime = 20,
                    OneShot = false,
                    Autostart = true
                };

                _cleanUpTimer.Timeout += () =>
                {
                    cleanUpBindings();
                };

                if (Engine.GetMainLoop() is SceneTree sceneTree)
                {
                    sceneTree.CurrentScene.AddChild(_cleanUpTimer);
                }
            }
        }

        private void cleanUpBindings()
        {
            lock (_controlBindings)
            {
                var bindingsToRemove = new List<Binding>();
                foreach (var binding in _controlBindings)
                {
                    if (!IsInstanceValid(binding.GetBoundControl())
                        || !IsInstanceValid(binding.GetTargetObject()))
                    {
                        bindingsToRemove.Add(binding);
                    }
                }

                if (bindingsToRemove.Any())
                {
                    foreach (var binding in bindingsToRemove)
                    {
                        _controlBindings.Remove(binding);
                    }
                }
            }
        }

        #endregion

    }
}
