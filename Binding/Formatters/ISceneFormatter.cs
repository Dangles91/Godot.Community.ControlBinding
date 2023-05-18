using ControlBinding.Utilities;
using Godot;

namespace ControlBinding.Formatters;

public interface ISceneFormatter
{
    public string ScenePath { get; }
    public Node Format(object viewModelData)
    {
        return SceneInstancer.CreateSceneInstance(ScenePath, viewModelData);
    }
}