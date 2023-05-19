using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.ControlBinders;
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
    private readonly ControlBinderProvider _controlBinderProvider = new();

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

    /// <summary>
    /// Bind a list control to an IObservableList or IList property
    /// Note: list controls include OptionButton and ItemList
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene.</param>
    /// <param name="path">The path of the property to bind to. Relative to this object.</param>
    /// <param name="bindingMode">The binding mode to use</param>
    /// <param name="formatter">The IValueFormatter to use to format the the list item and target property. Return a <see cref="ControlBinding.Collections.ListItem"/> for greater formatting control.</param>
    public void BindListProperty(
        string controlPath,
        string path,
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
                BoundControl = new WeakReference(node),
                Formatter = formatter,
                IsListBinding = true,
                Owner = this,
                Path = path
            };

            var binding = new Binding(bindingConfiguration, binder);
            binding.BindControl();
            _controlBindings.Add(binding);
        }
    }

    /// <summary>
    /// Binds an emum to an OptionButton control with optional path for the selected value
    /// </summary>
    /// <param name="controlPath">The path of the Godot control in the scene.</param>
    /// <param name="selectedItemPath">The path of the property to bind to. Relative to this object.</param>
    /// <typeparam name="T">The enum type to bind the OptionButton to</typeparam>
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

        ObservableList<T> targetObject = new();
        foreach (var entry in Enum.GetValues(typeof(T)))
        {
            targetObject.Add((T)entry);
        }

        // bind the list items (static list binding - enums won't change at runtime)
        if (_controlBinderProvider.GetBinder(node) is IControlBinder binder)
        {
            var bindingConfiguration = new BindingConfiguration
            {
                BindingMode = BindingMode.OneWay,
                TargetObject = new WeakReference(targetObject),
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
                FormatTarget = (v) =>
                {
                    return targetObject[(int)v == -1 ? 0 : (int)v];
                },
                FormatControl = (v) =>
                {
                    return targetObject.IndexOf((T)v);
                }
            });
        }
    }

    public void BindSceneList(string controlPath, string path, string scenePath, BindingMode bindingMode = BindingMode.OneWay)
    {
        var node = GetNode<Godot.Control>(controlPath);
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
}
