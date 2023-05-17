using System;
using Godot;

namespace ControlBinding.Formatters
{
    public class SceneFormatter : ISceneFormatter
    {
        public string ScenePath { get; }

        public PackedScene Format()
        {
            throw new NotImplementedException();
        }
    }
}