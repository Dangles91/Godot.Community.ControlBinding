using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Godot.Community.ControlBinding.Factories
{
    public class BindingFactory
    {
        private readonly Binding _binding;
        public BindingFactory(Binding binding)
        {
            _binding = binding;
        }

        public BindingFactory AddValidator(Func<object, string> validator)
        {
            _binding.BoundPropertySetter.Validators.Add(validator);
            return this;
        }
    }
}