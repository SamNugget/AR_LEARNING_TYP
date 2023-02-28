using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FileManagement;
using ObjectInstances;

public class ConstructorSnippet : RunnableSnippet
{
    protected override string decBlockName
    {
        get { return "Constructor"; }
    }

    protected override Block getParametersParent()
    {
        return methodSave.methodDeclaration.getSubBlock(1);
    }

    public override void run()
    {
        List<ObjectInstance> parameters = new List<ObjectInstance>();
        for (int i = snapPoints.Count - 1; i >= 0; i--)
            parameters.Add(snapPoints[i].parameter);

        ClassWindow cW = GetComponentInParent<ClassWindow>();

        CompilationManager.constructObject(cW.referenceTypeSave.name, parameters, spawnPoint);
    }
}
