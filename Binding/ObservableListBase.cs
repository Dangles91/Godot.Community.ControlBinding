using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding.EventArgs;
using Godot;

namespace ControlBinding.Binding
{
    public partial class ObservableListBase : GodotObject
    {       
        [Signal]
        public delegate void PropertyChangedEventHandler(GodotObject sender);

        [Signal]
        public delegate void ObservableListChangedEventHandler(ObservableListChangedEventArgs args);
    }
}