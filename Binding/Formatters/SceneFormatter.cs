using System;
using Godot.Community.ControlBinding.Utilities;
using Godot;

namespace Godot.Community.ControlBinding.Formatters
{
    public class SceneFormatter : ISceneFormatter
    {
        public string ScenePath { get; init; }
    }
}