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

    protected override void run()
    {
        object[] args = new object[snapPoints.Count];
        for (int i = snapPoints.Count - 1; i >= 0; i--)
        {
            ObjectInstance oI = snapPoints[i].parameter;
            args[i] = (oI == null ? null : oI.inMemory);
        }
        CompilationManager.executeSnippet(snippetName, args, transform.position + new Vector3(0f, 1f, 0f));
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
        string returnType = methodSave.methodDeclaration.getSubBlock(1).getBlockText(false);
        src.Append(returnType + ' ');
        string parameters = getParametersParent().getBlockText(true);
        src.AppendLine(snippetName + '(' + parameters + ')');
        src.AppendLine("{\n" + methodSave.methodBodyMaster.getBlockText(true) + "\n}");

        return src.ToString();
    }
}
