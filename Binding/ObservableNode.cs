using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.ControlBinders;
using Godot.Community.ControlBinding.Factories;
using Godot.Community.ControlBinding.Formatters;
using Godot.Community.ControlBinding.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Godot.Community.ControlBinding;

public partial class ObservableNode : Node, IObservableNode, IObservableObject
{
    public event PropertyChangedEventHandler PropertyChanged;
    public event ValidationChangedEventHandler ControlValidationChanged;    

    private readonly List<Binding> _controlBindings = new();
    private readonly object cleanUpLock = 0;

    /// <inheritdoc />
    public void SetValue<T>(ref T field, T value, [CallerMemberName] string name = "not a property")
    {
        field = value;
        OnPropertyChanged(name);
    }

    /// <inheritdoc />
    public void OnPropertyChanged([CallerMemberName] string name = "not a property")
    {
        var invalidBindings = _controlBindings.Where(x => x.BindingStatus == BindingStatus.Invalid);
        if (invalidBindings.Any())
        {
            System.Threading.Tasks.Task.Run(() =>
            {
                lock (cleanUpLock)
                {
                    var invalidBindings = _controlBindings.Where(x => x.BindingStatus == BindingStatus.Invalid).ToList();
                    foreach (var binding in invalidBindings)
                    {
                        _controlBindings.Remove(binding);
                    }
                }
            });
        }

        PropertyChanged?.Invoke(this, name);
    }

    public virtual void SetViewModelData(object viewModelData)
    {
        // no default implementation. Intended to be overridden for use with scene formatters. 
    }

    /// <summary>
    /// Bind a control property to an object property
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene</param>
    /// <param name="sourceProperty">The property of the Godot control to bind to</param>
    /// <param name="path">The path of the property to bind to. Relative to this object</param>
    /// <param name="bindingMode">The binding mode to use</param>
    /// <param name="formatter">The <see cref="ControlBinding.Formatters.IValueFormatter" /> to use to format the the Control property and target property</param>
    public BindingBuilder<T, TSource, TTarget> BindProperty<T, TSource, TTarget>(
        string controlPath,
        string sourceProperty,
        string path,
        BindingMode bindingMode = BindingMode.OneWay,
        IValueFormatter<TSource, TTarget> formatter = null
        ) where T : Control {
        var node = GetNode<T>(controlPath);
        if (node == null)
        {
            GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
            return null;
        }

        if (ControlBinderProvider.GetBinder(node) is IControlBinder binder)
        {
            var bindingConfiguration = new BindingConfiguration
            {
                BindingMode = bindingMode,
                BoundPropertyName = sourceProperty,
                Path = path,
                BoundControl = new WeakReference(node),
                Formatter = formatter == null ? null : 
                    new ValueFormatter<object, object> {
                        FormatTarget = (v, vv) => formatter.FormatTarget((TTarget)v, (TSource)vv), 
                        FormatControl = (v, vv) => formatter.FormatControl((TSource)v, (TTarget)vv), 
                    },
                Owner = this,
            };

            var binding = new Binding(bindingConfiguration, binder);
            binding.BindControl();
            _controlBindings.Add(binding);
            return new BindingBuilder<T, TSource, TTarget>(binding);
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
    /// <param name="formatter">The IValueFormatter to use to format the the list item and target property. Return a <see cref="ControlBinding.Collections.ListItem"/> for greater formatting control.</param>
    public BindingBuilderBase BindListProperty<T, TSource, TTarget>(
        string controlPath,
        string path,
        BindingMode bindingMode = BindingMode.OneWay,
        IValueFormatter<TSource, TTarget> formatter = null)
        where T : Control 
    {
        var node = GetNode<T>(controlPath);
        if (node == null)
        {
            GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
            return null;
        }

        if (ControlBinderProvider.GetBinder(node) is IControlBinder binder)
        {
            var bindingConfiguration = new BindingConfiguration
            {
                BindingMode = bindingMode,
                BoundControl = new WeakReference(node),
                Formatter = formatter == null ? null : 
                    new ValueFormatter<object, object> {
                        FormatTarget = (v, vv) => formatter.FormatTarget((TTarget)v, (TSource)vv), 
                        FormatControl = (v, vv) => formatter.FormatControl((TSource)v, (TTarget)vv), 
                    },
                IsListBinding = true,
                Owner = this,
                Path = path
            };

            var binding = new Binding(bindingConfiguration, binder);
            binding.BindControl();
            _controlBindings.Add(binding);
            return new BindingBuilderBase(binding);
        }

        return null;
    }

    /// <summary>
    /// Binds an emum to an OptionButton control with optional path for the selected value
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene.</param>
    /// <param name="selectedItemPath">The path of the property to bind to. Relative to this object.</param>
    /// <typeparam name="TEnum">The enum type to bind the OptionButton to</typeparam>
    public void BindEnumProperty<T, TEnum>(string controlPath, string selectedItemPath = null) 
        where TEnum : Enum
        where T : Control
    {
        var node = GetNode<T>(controlPath);
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

        ObservableList<TEnum> targetObject = new();
        foreach (var entry in Enum.GetValues(typeof(TEnum)))
        {
            targetObject.Add((TEnum)entry);
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
                Owner = this,
                Formatter = new ValueFormatter<object, object>
                {
                    FormatControl = (v, p) =>
                    {
                        var enumValue = (TEnum)v;
                        return new ListItem
                        {
                            DisplayValue = enumValue.ToString(),
                            Id = (int)Enum.Parse(typeof(TEnum), v.ToString())
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
            BindProperty<T, int, TEnum>(controlPath, "Selected", selectedItemPath, BindingMode.TwoWay, new ValueFormatter<int, TEnum>
            {
                // FormatTarget = (v, p) => targetObject[(int)v == -1 ? 0 : (int)v],
                // FormatControl = (v, p) => targetObject.IndexOf((TEnum)v)
            });
        }
    }

    public void BindSceneList<T>(string controlPath, string path, string scenePath, BindingMode bindingMode = BindingMode.OneWay)
        where T : Control
    {
        var node = GetNode<T>(controlPath);
        if (node == null)
        {
            GD.PrintErr($"DataBinding: Unable to find node with path '{controlPath}'");
            return;
        }

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
            Owner = this,
            Path = path
        };

        var binding = new Binding(bindingConfiguration, binder);
        binding.BindControl();
        _controlBindings.Add(binding);

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
        private set => SetValue(ref _hasErrors, value);
    }

    public List<string> GetValidationMessages()
    {
        return _validationErrors.SelectMany(x => x.Value).ToList();
    }
}
