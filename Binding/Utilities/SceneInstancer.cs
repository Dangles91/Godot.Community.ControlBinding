using Godot;

namespace ControlBinding.Utilities;

public static class SceneInstancer
{
    public static Node CreateSceneInstance(string scenePath, object viewModelData)
    {
        var scene = (Godot.PackedScene)ResourceLoader.Load(scenePath);        
        var node = scene.Instantiate<ObservableObject>();        

        node.SetViewModelData(viewModelData);
        return node;
    }
}