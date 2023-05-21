using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Formatters;


namespace ControlBinding
{
    public class StringToListFormatter : IValueFormatter
    {
        public Func<object, object, object> FormatControl => (v, p) =>
        {
            if (v is not List<string> data)
                return null;
            return string.Join("\n", data);
        };

        public Func<object, object, object> FormatTarget => (v, p) =>
        {
            if (v is not string data)
                return null;
            return new ObservableList<string>(data.Split("\n").ToList());
        };
    }
}