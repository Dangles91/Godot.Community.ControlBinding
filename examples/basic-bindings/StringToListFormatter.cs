using System;
using System.Collections.Generic;
using System.Linq;
using ControlBinding.Collections;
using ControlBinding.Formatters;


namespace ControlBinding
{
    public class StringToListFormatter : IValueFormatter
    {
        public Func<object, object> FormatControl
        {
            get
            {
                return (v) =>
                {
                    var data = v as List<string>;
                    if(data is null)
                        return null;
                    return string.Join("\n", data);
                };
            } 
        }

        public Func<object, object> FormatTarget
        {
            get 
            {
                return (v) =>
                {
                    var data = v as string;
                    if(data is null)
                        return null;
                    return new ObservableList<string>(data.Split("\n").ToList());
                };
            } 
        }
    }
}