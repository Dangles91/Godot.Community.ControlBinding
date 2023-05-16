using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding;
using ControlBinding.Binding.Interfaces;
using Godot;

namespace ControlBinding
{
    public class SceneInstancer
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
}