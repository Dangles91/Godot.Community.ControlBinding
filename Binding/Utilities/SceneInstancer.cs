using Godot;

namespace ControlBinding.Utilities;

public static class SceneInstancer
{
    public static Node CreateSceneInstance(string scenePath, ViewModel viewModel)
    {
        var scene = (Godot.PackedScene)ResourceLoader.Load(scenePath);
        var script = ResourceLoader.Load(viewModel.GetScriptPath());

        var node = scene.Instantiate();
        var id = node.GetInstanceId();

        node.SetScript(script);

        node = (Node)Node.InstanceFromId(id);

        return node;
    }
}