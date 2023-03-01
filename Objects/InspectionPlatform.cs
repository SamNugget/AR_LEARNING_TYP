using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using ObjectInstances;

public class InspectionPlatform : MonoBehaviour, SnapListener
{
    private static InspectionPlatform inspectionPlatform;

    [SerializeField] private TextMeshProUGUI screenText;
    [SerializeField] private float printSpeed = 1f;
    [SerializeField] private string defaultMessage;

    public void onSnap(Transform snapped)
    {
        ObjectInstance oI = snapped.GetComponent<ObjectInstance>();

        if (oI == null)
        {
            Debug.Log("Err: Snapped object has no ObjectInstance component.");
            return;
        }

        Log(oI.getInspectText());
    }

    public void onUnsnap()
    {
        Log(defaultMessage);
    }

    protected IEnumerator printText(string text)
    {
        screenText.text = "";

        float timePerLetter = printSpeed / text.Length;

        for (int i = 0; i < text.Length; i++)
        {
            screenText.text += text[i];
            yield return new WaitForSeconds(timePerLetter);
        }
    }

    public static void Log(string message)
    {
        inspectionPlatform.StopAllCoroutines();
        inspectionPlatform.StartCoroutine(inspectionPlatform.printText(message));
    }

    void Awake()
    {
        inspectionPlatform = this;

        screenText.text = defaultMessage;
    }
}
