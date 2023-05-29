using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.ControlBinders;
using Godot.Community.ControlBinding.Factories;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;

namespace Godot.Community.ControlBinding
{
    public class BindingContext : ObservableObject
    {
        private readonly IObservableObject _bindingRoot;
        public BindingContext(IObservableObject bindingRoot)
        {
            _bindingRoot = bindingRoot;
        }

        private readonly static Dictionary<object, List<Binding>> _bindings = new();

        private void AddBinding(Binding binding)
        {
            if (!_bindings.ContainsKey(binding.BindingConfiguration.BoundControl.Target))
            {
                _bindings.Add(binding.BindingConfiguration.BoundControl.Target, new List<Binding>());
            }

            binding.ValidationFailed += OnPropertyValidationFailed;
            binding.ValidationSucceeded += OnPropertyValidationSucceeded;
            binding.BindingStatusChanged += OnBindingStatusChanged;

            _bindings[binding.BindingConfiguration.BoundControl.Target].Add(binding);
        }

        private void OnBindingStatusChanged(object sender, BindingStatus e)
        {
            if (sender is Binding binding)
            {
                if (e == BindingStatus.Invalid)
                {
                    _bindings[binding.BindingConfiguration.BoundControl.Target].Remove(binding);
                    if (_bindings[binding.BindingConfiguration.BoundControl.Target].Count == 0)
                    {
                        _bindings.Remove(binding.BindingConfiguration.BoundControl.Target);
                    }
                }
            }
        }

        /// <summary>
        /// Bind a control property to an object property
        /// </summary>
        /// <param name="node">The node to bind to</param>
        /// <param name="sourceProperty">The property of the Godot control to bind to</param>
        /// <param name="path">The path of the property to bind to. Relative to this object</param>
        /// <param name="bindingMode">The binding mode to use</param>
        /// <param name="formatter">The <see cref="ControlBinding.Formatters.IValueFormatter" /> to use to format the Control property and target property</param>
        public BindingBuilder BindProperty(
            Node node,
            string sourceProperty,
            string path,
            BindingMode bindingMode = BindingMode.OneWay,
            IValueFormatter formatter = null
            )
        {
            if (ControlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = bindingMode,
                    BoundPropertyName = sourceProperty,
                    Path = path,
                    BoundControl = new WeakReference(node),
                    Formatter = formatter,
                    Owner = _bindingRoot,
                };

                var binding = new Binding(bindingConfiguration, binder);
                binding.BindControl();
                AddBinding(binding);
                return new BindingBuilder(binding);
            }
            return null;
        }

        /// <summary>
        /// Bind a list control to an IObservableList or IList property
        /// Note: list controls include OptionButton and ItemList
        /// </summary>
        /// <param name="controlPath">The path of the Godot control in the scene.</param>
        /// <param name="path">The path of the property to bind to. Relative to this object.</param>
        /// <param name="bindingMode">The binding mode to use</param>
        /// <param name="formatter">The IValueFormatter to use to format the list item and target property. Return a <see cref="ControlBinding.Collections.ListItem"/> for greater formatting control.</param>
        public void BindListProperty(
            Node node,
            string path,
            BindingMode bindingMode = BindingMode.OneWay,
            IValueFormatter formatter = null
            )
        {
            if (ControlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = bindingMode,
                    BoundControl = new WeakReference(node),
                    Formatter = formatter,
                    IsListBinding = true,
                    Owner = _bindingRoot,
                    Path = path
                };

                var binding = new Binding(bindingConfiguration, binder);
                binding.BindControl();
                AddBinding(binding);
            }
        }

        public void BindSceneList(
            Node node,
            string path,
            string scenePath,
            BindingMode bindingMode = BindingMode.OneWay)
        {
            var binder = new GenericControlBinder();
            var bindingConfiguration = new BindingConfiguration
            {
                BindingMode = bindingMode,
                BoundControl = new WeakReference(node),
                SceneFormatter = new SceneFormatter
                {
                    ScenePath = scenePath
                },
                IsListBinding = true,
                Owner = _bindingRoot,
                Path = path
            };

            var binding = new Binding(bindingConfiguration, binder);
            binding.BindControl();
            AddBinding(binding);
        }

        /// <summary>
        /// Binds an emum to an OptionButton control with optional path for the selected value
        /// </summary>
        /// <param name="controlPath">The path of the Godot control in the scene.</param>
        /// <param name="selectedItemPath">The path of the property to bind to. Relative to this object.</param>
        /// <typeparam name="T">The enum type to bind the OptionButton to</typeparam>
        public void BindEnumProperty<T>(OptionButton node, string selectedItemPath = null) where T : Enum
        {
            ObservableCollection<T> targetObject = new();
            foreach (var entry in Enum.GetValues(typeof(T)))
            {
                targetObject.Add((T)entry);
            }

            // bind the list items (static list binding - enums won't change at runtime)
            if (ControlBinderProvider.GetBinder(node) is IControlBinder binder)
            {
                var bindingConfiguration = new BindingConfiguration
                {
                    BindingMode = BindingMode.OneWay,
                    TargetObject = new WeakReference(targetObject),
                    BoundControl = new WeakReference(node),
                    IsListBinding = true,
                    Path = string.Empty,
                    Owner = _bindingRoot,
                    Formatter = new ValueFormatter
                    {
                        FormatControl = (v, _) =>
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
                AddBinding(binding);
            }
            if (!string.IsNullOrEmpty(selectedItemPath))
            {
                BindProperty(node, "Selected", selectedItemPath, BindingMode.TwoWay, new ValueFormatter
                {
                    FormatTarget = (v, _) =>
                    {
                        if (v is null || targetObject is null)
                            return null;

                        return targetObject[(int)v == -1 ? 0 : (int)v];
                    },
                    FormatControl = (v, _) =>
                    {
                        if (v is null || targetObject is null)
                            return null;

                        return targetObject.IndexOf((T)v);
                    }
                });
            }
        }

        private readonly Dictionary<ulong, List<string>> _validationErrors = new();
        public void OnPropertyValidationFailed(Control control, string targetPropertyName, string message)
        {
            var instanceId = control.GetInstanceId();
            if (!_validationErrors.ContainsKey(instanceId))
            {
                _validationErrors.Add(instanceId, new());
            }

            _validationErrors[instanceId].Clear();
            _validationErrors[instanceId].Add(message);

            HasErrors = true;
            ControlValidationChanged?.Invoke(control, targetPropertyName, message, false);
        }

        public void OnPropertyValidationSucceeded(Godot.Control control, string propertyName)
        {
            var instanceId = control.GetInstanceId();
            if (_validationErrors.ContainsKey(instanceId))
            {
                // raise validation changed
                _validationErrors.Remove(instanceId);
                ControlValidationChanged?.Invoke(control, propertyName, null, true);
            }

            if (!_validationErrors.Any() && HasErrors)
                HasErrors = false;
        }

        private bool _hasErrors;

        public bool HasErrors
        {
            get => _hasErrors;

            private set
            {
                _hasErrors = value;
                OnPropertyChanged();
            }
        }

        public List<string> GetValidationMessages()
        {
            return _validationErrors.SelectMany(x => x.Value).ToList();
        }

        public event ValidationChangedEventHandler ControlValidationChanged;
    }
}