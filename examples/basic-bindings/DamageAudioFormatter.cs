using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding;
using Godot;

namespace ControlBinding
{
    public class DamageAudioFormatter : IValueFormatter
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
                    return data.Split("\n").ToList();                    
                };
            } 
        }
    }
}