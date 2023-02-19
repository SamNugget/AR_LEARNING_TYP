using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Reflection;

public class FontManager : MonoBehaviour
{
    public TMP_FontAsset font;
    public float approxScale = 0.001f;

    public static float lineHeight;
    public static float horizontalAdvance;

    // https://docs.unity3d.com/2019.1/Documentation/ScriptReference/TextCore.FaceInfo.html
    void Awake()
    {
        lineHeight = font.faceInfo.lineHeight * approxScale;

        // we can assume it is the same for all glyphs, as it is with this sort of font
        horizontalAdvance = font.glyphTable[0].metrics.horizontalAdvance * approxScale;
    }

    public static Vector2 lettersAndLinesToVector(int x, int y)
    {
        return new Vector2(x * horizontalAdvance, y * lineHeight);
    }

    public static int[] vectorToLettersAndLines(Vector2 v)
    {
        return new int[] { (int)(v.x / horizontalAdvance), (int)(v.y / lineHeight) };
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < 100; i++)
        {
            for (int j = 0; j < 100; j++)
            {
                Vector3 pos = new Vector3(i * horizontalAdvance * approxScale, -j * lineHeight * approxScale, 0f);
                Gizmos.DrawSphere(pos, 0.01f);
            }
        }
    }
}
