using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Godot.Community.ControlBinding.Factories
{
    public class BindingBuilderBase
    {
        protected readonly Binding _binding;
        public BindingBuilderBase(Binding binding)
        {
            _binding = binding;
        }
    }

    public class BindingValidatorBuilder<T> : BindingBuilderBase where T : BindingValidatorBuilder<T>
    {
        public BindingValidatorBuilder(Binding binding) : base(binding)
        {
        }

        public virtual T AddValidator(Func<object, string> validator)
        {
            _binding.BoundPropertySetter.Validators.Add(validator);
            return (T)this;
        }
    }

    public class BindingBuilderValidationHandler<T> : BindingValidatorBuilder<T> where T : BindingBuilderValidationHandler<T>
    {
        public BindingBuilderValidationHandler(Binding binding) : base(binding)
        {
        }

        public BindingValidatorBuilder<T> AddValidationHandler(Action<Control, bool, string> handler)
        {
            _binding.BindingConfiguration.OnValidationChangedHandler = handler;
            return (T)this;
        }
    }

    public class BindingBuilder : BindingBuilderValidationHandler<BindingBuilder>
    {
        public BindingBuilder(Binding binding) : base(binding)
        {
        }
    }
}