using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Text;
using FileManagement;
using ObjectInstances;

public class LoneSnippet : RunnableSnippet
{
    protected override string decBlockName
    {
        get { return "Snippet Declaration"; }
    }

    protected override Block getParametersParent()
    {
        return methodSave.methodDeclaration.getSubBlock(0);
    }

    public override void run()
    {
        List<ObjectInstance> parameters = new List<ObjectInstance>();
        for (int i = snapPoints.Count - 1; i >= 0; i--)
            parameters.Add(snapPoints[i].parameter);

        CompilationManager.executeSnippet(snippetName, parameters, spawnPoint);
    }



    public static int snippetCounter = 0; // TODO: would need saved
    public string snippetName = null;

    private void initialiseName()
    {
        // 97 to 122 : a to z

        int firstDigit = (snippetCounter / 26) + 97;
        int secondDigit = (snippetCounter % 26) + 97;
        snippetName = "" + (char)firstDigit + (char)secondDigit;

        snippetCounter++;
    }
    protected override void initialise()
    {
        base.initialise();
        initialiseName();
    }

    public string getCode()
    {
        StringBuilder src = new StringBuilder();
        src.Append("public static ");
        string returnType = methodSave.methodDeclaration.getSubBlock(1).getBlockText(true);
        src.Append(returnType + ' ');
        string parameters = getParametersParent().getBlockText(true);
        src.AppendLine(snippetName + '(' + parameters + ')');
        src.AppendLine("{\n" + methodSave.methodBodyMaster.getBlockText(true) + "\n}");

        return src.ToString();
    }
}
