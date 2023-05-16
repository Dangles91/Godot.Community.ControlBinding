using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ControlBinding.Binding;
using ControlBinding.Binding.Services;

namespace ControlBinding.Services
{
    public static class BackReferenceFactory
    {        
        public static List<object> GetPathObjectsAndBuildBackReferences(string[] pathNodes, ref BindingConfiguration bindingConfiguration)
        {            
            if (bindingConfiguration.BackReferences == null)
                bindingConfiguration.BackReferences = new();

            List<object> pathObjects = new();
            pathObjects.Add(bindingConfiguration.Owner);

            if (pathNodes.Length > 1)
            {
                object root = bindingConfiguration.Owner;

                for (int i = 0; i < pathNodes.Length; i++) // -1, we don't want to store the property value
                {
                    // TODO: make this easier to read
                    object pathObject = null;
                    var pathNode = pathNodes[i];
                    if (i == 0)
                    {
                        pathObject = root;
                        pathObjects.Add(pathObject);
                    }
                    else
                    {
                        pathNode = pathNodes[i - 1];
                        pathObject = ReflectionService.GetPropertyInfo(root, pathNode).GetValue(root);
                        pathObjects.Add(pathObject);
                        if (i + 1 > pathNodes.Length - 1)
                            continue;
                    }

                    if (!bindingConfiguration.BackReferences.Any(x => x.ObjectReference.Target == pathObject && x.PropertyName == pathNodes[i]))
                    {
                        bindingConfiguration.BackReferences.Add(new WeakBackReference
                        {
                            ObjectReference = new WeakReference(pathObject),
                            PropertyName = pathNodes[i],
                        });
                    }

                    root = pathObject;
                }
            }

            return pathObjects;
        }
    }
}