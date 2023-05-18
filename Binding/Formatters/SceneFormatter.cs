using System;
using ControlBinding.Utilities;
using Godot;

namespace ControlBinding.Formatters
{
    public class SceneFormatter : ISceneFormatter
    {
        public string ScenePath { get; init; }
    }
}