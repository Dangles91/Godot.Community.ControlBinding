using System;

namespace ControlBinding.Binding
{
    public class WeakBackReference
    {
        public WeakReference ObjectReference { get; set; }
        public string PropertyName { get; set; }  
    }
}