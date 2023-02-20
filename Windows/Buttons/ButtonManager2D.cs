using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using TMPro;

public abstract class ButtonManager2D : MonoBehaviour
{
    [SerializeField] protected float buttonSpacing;

    [SerializeField] private float maxWidth = 10;



    public abstract void distributeButtons();

    protected List<Transform> distributeVertically(string[] buttonLabels, int[] actions, object[] data, Transform parent = null)
    {
        if (buttonLabels.Length != actions.Length || buttonLabels.Length != data.Length)
        {
            Debug.Log("All three input parameter arrays must be the same length.");
            return null;
        }
        if (parent == null) parent = transform;



        List<Transform> buttons = new List<Transform>();
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            Transform newButton = spawnButton(buttonLabels[i], actions[i], data[i], i, parent);
            buttons.Add(newButton);
        }
        return buttons;
    }

    protected List<Transform> distributeFit(string[] buttonLabels, int[] actions, object[] data, Transform parent = null)
    {
        if (buttonLabels.Length != actions.Length || buttonLabels.Length != data.Length)
        {
            Debug.Log("All three input parameter arrays must be the same length.");
            return null;
        }
        if (parent == null) parent = transform;



        List<Transform> buttons = new List<Transform>();
        int row = 0;
        float rowWidth = 0;
        for (int i = 0; i < buttonLabels.Length; i++)
        {
            float width = buttonLabels[i].Length + (buttonSpacing / FontManager.horizontalAdvance);
            if (rowWidth != 0f && rowWidth + width > maxWidth)
            {
                row++;
                rowWidth = 0f;
            }

            // create button
            float xPos = FontManager.horizontalAdvance * rowWidth;
            //Debug.Log("label: " + buttonLabels[i] + ", row: " + row + ", rowW: " + rowWidth);
            Transform newButton = spawnButton(buttonLabels[i], actions[i], data[i], row, parent, xPos);
            buttons.Add(newButton);

            rowWidth += width;
        }
        return buttons;
    }

    protected Transform spawnButton(string buttonLabel, int action, object data, int row, Transform parent, float xPos = 0f)
    {
        Vector2 planeSize = FontManager.lettersAndLinesToVector(buttonLabel.Length, 1);
        float width = planeSize.x;
        float height = planeSize.y;
        float s = BlockManager.blockScale;

        // spawn, position and scale button
        Transform newButton = Instantiate(WindowManager.buttonFab, parent).GetComponent<Transform>();
        newButton.localPosition = new Vector3(xPos * s, (row * (-height - buttonSpacing) * s), 0f);
        newButton.localScale = new Vector3(s, s, s);

        // scale body plane
        Transform body = newButton.GetChild(0);
        body.localScale = new Vector3(width, height, 1f);

        // scale body text
        RectTransform tB = (RectTransform)newButton.GetChild(1);
        tB.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
        tB.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, height);
        tB.localPosition = new Vector3(width / 2f, -height / 2f, tB.localPosition.z);

        // set text of button
        tB.GetComponent<TextMeshPro>().text = buttonLabel;

        // configure actionbuttonscript
        ActionButton actionButtonScript = newButton.GetComponent<ActionButton>();
        actionButtonScript.setAction(action);
        actionButtonScript.setData(data);

        return newButton;
    }

    protected void removeButtonFromList(Transform toRemove, List<Transform> buttons)
    {
        int index = buttons.IndexOf(toRemove);
        if (index < 0) return;


        Transform toReplace = (index + 1 < buttons.Count ? buttons[index + 1] : null);
        if (toReplace != null)
        {
            Vector2 change = toRemove.position - toReplace.position;
            // shift all the buttons over
            for (int i = index + 1; i < buttons.Count; i++)
                buttons[i].position += (Vector3)change;
        }


        // delete button
        Destroy(toRemove.gameObject);
    }

    protected void deleteButtons(Transform parent = null)
    {
        if (parent == null) parent = transform;

        foreach (Transform child in parent)
            Destroy(child.gameObject);
    }

    public void Start()
    {
        distributeButtons();
    }
}
