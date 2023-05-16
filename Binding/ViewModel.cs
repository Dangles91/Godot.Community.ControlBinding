using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;

namespace ControlBinding.Binding
{
    public abstract partial class ViewModel : ObservableObject
    {
        public ObservableObject ViewModelData { get; set; }               
        public abstract void BindViewModel();

        public abstract string GetScriptPath();
        public void SetViewModelData(ObservableObject data)
        {
            ViewModelData = data;
        }
        public override void _Ready()
        {
            BindViewModel();
            base._Ready();
        }
    }
}