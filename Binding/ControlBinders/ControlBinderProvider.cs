using System.Collections.Generic;
using System.Linq;

namespace Godot.Community.ControlBinding.ControlBinders
{
    public static class ControlBinderProvider
    {
        private static readonly List<IControlBinder> _binders = new()
        {
            new LineEditControlBinder(),
            new CheckBoxControlBinder(),
            new OptionButtonControlBinder(),
            new TextEditControlBinder(),
            new RangeControlBinder(),
            new ItemListControlBinder(),
            new GenericControlBinder(),
        };

        public static IControlBinder GetBinder(object sourceObject)
        {
            var binder = _binders
                .OrderByDescending(x => x.Priority)
                .FirstOrDefault(x => x.CanBindFor(sourceObject));

            if (binder == null)
            {
                GD.PrintErr($"Cannot find binder for {sourceObject.GetType()}");
                return null;
            }

            var binderInstance = binder.CreateInstance();
            return binderInstance;
        }
    }
}