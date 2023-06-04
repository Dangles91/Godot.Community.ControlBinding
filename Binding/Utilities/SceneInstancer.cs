using Godot.Community.ControlBinding.Interfaces;

namespace Godot.Community.ControlBinding.Utilities;

internal static class SceneInstancer
{
    public static Node CreateSceneInstance(string scenePath, object viewModelData)
    {
        var scene = (Godot.PackedScene)ResourceLoader.Load(scenePath);
        var node = scene.Instantiate();
        if (node is IViewModel viewModel)
            viewModel.SetViewModelData(viewModelData);
        return node;
    }
}