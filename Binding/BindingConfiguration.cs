using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using ControlBinding.Binding.ControlBinders;
using ControlBinding.Binding.EventArgs;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding.Binding
{
    public enum BindingMode
    {
        OneWay,
        TwoWay,
        OneWayToTarget,
    }

    public class BindingConfiguration : IDisposable
    {
        public string BoundPropertyName { get; set; }
        public string TargetPropertyName { get; set; }
        public BindingMode BindingMode { get; set; }
        public bool IsListBinding { get; set; }
        public object Owner { get; init; }
        public WeakReference BoundControl {get;set;}
        public GodotObject TargetObject {get;set;}
        public IValueFormatter Formatter {get;set;}
        public List<WeakBackReference> BackReferences {get;set;}
        public ISceneFormatter SceneFormatter {get;set;}
        public string Path { get; set; }

        public void Dispose()
        {
            if(BackReferences?.Count > 0)
                BackReferences.Clear();
            
            BoundControl = null;
            TargetObject = null;
        }
    }
}