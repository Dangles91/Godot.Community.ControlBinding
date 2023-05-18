using System;
using System.Collections.Generic;
using System.Linq;
using Godot.Community.ControlBinding.Collections;
using Godot.Community.ControlBinding.Interfaces;

namespace Godot.Community.ControlBinding.Services;

public static class BackReferenceFactory
{
    public static List<object> GetPathObjectsAndBuildBackReferences(List<string> pathNodes, ref BindingConfiguration bindingConfiguration)
    {
        bindingConfiguration.BackReferences?.Clear();
        bindingConfiguration.BackReferences = new();

        List<object> pathObjects = new();
        pathObjects.Add(bindingConfiguration.Owner);

        if (pathNodes.Count > 1)
        {
            object root = bindingConfiguration.Owner;

            for (int i = 0; i < pathNodes.Count; i++)
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
                    if (!bindingConfiguration.IsListBinding && i + 1 > pathNodes.Count - 1)
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
            pathNodes = pathNodes.Where(x => !string.IsNullOrEmpty(x)).ToList();
            if (pathNodes.Count == 1)
            {
                pathObjects.Add(bindingConfiguration.Owner);
            }

            // Support enum bindings
            if (pathNodes.Count == 0 && bindingConfiguration.TargetObject.Target != null)
            {
                pathObjects.Add(bindingConfiguration.TargetObject.Target);
            }

            if (pathObjects.Last() is IObservableObject observableObject && pathObjects.Last() is not IObservableList)
            {
                var list = ReflectionService.GetPropertyInfo(observableObject, pathNodes.Last()).GetValue(observableObject);
                pathObjects.Add(list);

                bindingConfiguration.BackReferences.Add(new WeakBackReference
                {
                    ObjectReference = new WeakReference(pathObjects[pathObjects.Count - 2], false),
                    PropertyName = pathNodes.Last()
                });
            }
        }

        return pathObjects.ToList();
    }
}