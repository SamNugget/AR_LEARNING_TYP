using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;
using ActionManagement;

public class EditButtonManager : ButtonManager2D
{
    [SerializeField] private Transform blockButtonsParent;

    public override void distributeButtons()
    {
        deleteButtons(blockButtonsParent);



        int noOfVariants = BlockManager.placeableVariants;

        string[] buttonLabels = new string[noOfVariants];
        int[] actions = new int[noOfVariants];
        object[] data = new object[noOfVariants];

        for (int i = 0; i < noOfVariants; i++)
        {
            int variantIndex = i + (BlockManager.getNoOfDefaultBlockVariants() - noOfVariants);

            BlockManager.BlockVariant bV = BlockManager.getBlockVariant(variantIndex);
            buttonLabels[i] = bV.getName();
            actions[i] = ActionManager.PLACE_SELECT;
            data[i] = variantIndex;
        }

        distributeFit(buttonLabels, actions, data, blockButtonsParent);
    }
}
