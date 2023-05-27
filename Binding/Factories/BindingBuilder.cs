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

    public class BindingValidatorBuilder<TBuilder, T, TSource, TTarget> : BindingBuilderBase 
        where TBuilder : BindingValidatorBuilder<TBuilder, T, TSource, TTarget> 
        where T : Control {
        public BindingValidatorBuilder(Binding binding) : base(binding)
        {
        }

        public virtual TBuilder AddValidator(Func<TTarget, string> validator)
        {
            _binding.BoundPropertySetter.Validators.Add((source) => validator((TTarget)source));
            return (TBuilder)this;
        }
    }

    public class BindingBuilderValidationHandler<TBuilder, T, TSource, TTarget> : BindingValidatorBuilder<TBuilder, T, TSource, TTarget> 
        where TBuilder : BindingBuilderValidationHandler<TBuilder, T, TSource, TTarget>
        where T : Control {
        public BindingBuilderValidationHandler(Binding binding) : base(binding)
        {
        }

        public TBuilder AddValidationHandler(Action<T, bool, string> handler)
        {
            _binding.BindingConfiguration.OnValidationChangedHandler = (control, b, arg3) => handler((T)control, b, arg3);
            return (TBuilder)this;
        }
    }

    public class BindingBuilder<T, TSource, TTarget> : BindingBuilderValidationHandler<BindingBuilder<T, TSource, TTarget>, T, TSource, TTarget> where T : Control {
        public BindingBuilder(Binding binding) : base(binding)
        {
        }
    }
}