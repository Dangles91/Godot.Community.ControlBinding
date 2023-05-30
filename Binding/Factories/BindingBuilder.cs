using System;

namespace Godot.Community.ControlBinding.Factories
{
    public class BindingBuilderBase
    {
        internal readonly Binding _binding;
        internal BindingBuilderBase(Binding binding)
        {
            _binding = binding;
        }
    }

    public class BindingValidatorBuilder<T> : BindingBuilderBase where T : BindingValidatorBuilder<T>
    {
        internal BindingValidatorBuilder(Binding binding) : base(binding)
        {
        }

        public virtual T AddValidator(Func<object, string> validator)
        {
            _binding.BoundPropertySetter.AddValidator(validator);
            return (T)this;
        }
    }

    public class BindingBuilderValidationHandler<T> : BindingValidatorBuilder<T> where T : BindingBuilderValidationHandler<T>
    {
        internal BindingBuilderValidationHandler(Binding binding) : base(binding)
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
        internal BindingBuilder(Binding binding) : base(binding)
        {
        }
    }
}