using Godot.Community.ControlBinding.Utilities;
using Godot;

namespace Godot.Community.ControlBinding.Formatters;

public interface ISceneFormatter
{
    public string ScenePath { get; }
    public Node Format(object viewModelData)
    {
        return SceneInstancer.CreateSceneInstance(ScenePath, viewModelData);
    }
}