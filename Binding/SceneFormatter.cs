using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding.Binding
{
    public class SceneFormatter : ISceneFormatter
    {
        public string ScenePath { get; }
        public ViewModel SceneModel { get; }

        public PackedScene Format()
        {
            throw new NotImplementedException();
        }
    }
}