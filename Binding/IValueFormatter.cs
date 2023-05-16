using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ControlBinding.Binding
{
    public interface IValueFormatter
    {
        public Func<object, object> FormatControl { get; }
        public Func<object, object> FormatTarget { get; }
    }

    public class ValueFormatter : IValueFormatter
    {
        public Func<object, object> FormatControl { get; init; }
        public Func<object, object> FormatTarget { get; init; }
    }
}