using System;
using System.Collections.Generic;
using System.Linq;

namespace ControlBinding.Services;

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

            for (int i = 0; i < pathNodes.Length; i++)
            {
                // TODO: make this easier to read
                object pathObject = null;
                if (i == 0)
                {
                    pathObject = root;
                    pathObjects.Add(pathObject);
                }
                else
                {
                    var pathNode = pathNodes[i - 1];
                    pathObject = ReflectionService.GetPropertyInfo(root, pathNode).GetValue(root);
                    pathObjects.Add(pathObject);
                    if (!bindingConfiguration.IsListBinding && i + 1 > pathNodes.Length - 1)
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

        if (bindingConfiguration.IsListBinding)
        {
            pathNodes = pathNodes.Where(x => !string.IsNullOrEmpty(x)).ToArray();
            if (pathNodes.Length == 1)
            {
                pathObjects.Add(bindingConfiguration.Owner);
            }

            // Support enum bindings
            if (pathNodes.Length == 0 && bindingConfiguration.TargetObject != null)
            {
                pathObjects.Add(bindingConfiguration.TargetObject);
            }

            if (pathObjects.Last() is ObservableObject observableObject)
            {
                var list = ReflectionService.GetPropertyInfo(observableObject, pathNodes.Last()).GetValue(observableObject);
                pathObjects.Add(list);

                bindingConfiguration.BackReferences.Add(new WeakBackReference
                {
                    ObjectReference = new WeakReference(pathObjects[pathObjects.Count - 2]),
                    PropertyName = pathNodes.Last()
                });
            }
        }

        return pathObjects;
    }
}