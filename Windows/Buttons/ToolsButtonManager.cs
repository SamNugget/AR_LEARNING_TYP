using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ActionManagement;

public class ToolsButtonManager : ButtonManager2D
{
    [SerializeField] private Transform buttonParent;

    public override void distributeButtons()
    {
        int noOfButtons = 5;

        string[] buttonLabels = new string[noOfButtons];
        int[] actions = new int[noOfButtons];
        object[] data = new object[noOfButtons];

        buttonLabels[0] = "Cancel"; actions[0] = 0;
        buttonLabels[1] = "Delete"; actions[1] = ActionManager.DELETE_SELECT;
        buttonLabels[2] = "Insert"; actions[2] = ActionManager.INSERT_LINE;

        buttonLabels[3] = "New Class"; actions[3] = ActionManager.CREATE_FILE;
        buttonLabels[4] = "New Snippet"; actions[4] = ActionManager.CREATE_SNIPPET;

        distributeFit(buttonLabels, actions, data, buttonParent);
    }
}
