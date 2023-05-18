using Godot;
using Godot.Community.ControlBinding;

namespace Godot.Community.ControlBinding.Utilities;

public static class SceneInstancer
{
    public static Node CreateSceneInstance(string scenePath, object viewModelData)
    {
        var scene = (Godot.PackedScene)ResourceLoader.Load(scenePath);        
        var node = scene.Instantiate<ObservableNode>();        

        node.SetViewModelData(viewModelData);
        return node;
    }
}