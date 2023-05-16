using System;
using System.Collections.Generic;
using System.Linq;
using ControlBinding.Binding.ControlBinders;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding.Binding.Utilities
{
    public partial class ControlBinderProvider
    {
        private List<IControlBinder> _binders = new List<IControlBinder>();
        public ControlBinderProvider()
        {
            _binders.Add(new LineEditControlBinder());
            _binders.Add(new CheckBoxControlBinder());
            _binders.Add(new OptionButtonControlBinder());
            _binders.Add(new TextEditControlBinder());
            _binders.Add(new RangeControlBinder());
            _binders.Add(new ItemListControlBinder());
            _binders.Add(new GenericControlBinder());
        }

        public IControlBinder GetBinder(object sourceObject)
        {
            var binder = _binders.FirstOrDefault(x => x.CanBindFor(sourceObject));

            if(binder == null)
            {
                GD.PrintErr($"Cannot find binder for {sourceObject.GetType()}");
                return null;
            }
                
            var binderInstance = binder.CreateInstance();
            return binderInstance;
        }
    }
}