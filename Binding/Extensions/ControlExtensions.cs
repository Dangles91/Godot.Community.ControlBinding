using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlBinding.Binding.Extensions
{
    public static class ControlExtensions
    {
        public static void BindProperty(
            this Godot.Control control, 
            string propertyName, 
            ObservableObject targetObject, 
            string targetPropertyName, 
            BindingMode bindingMode = BindingMode.OneWay )
        {
            
        }
    }
}