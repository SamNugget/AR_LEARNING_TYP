using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ObjectInstances;

public abstract class RunnableSnippet : Snippet
{
    private static float cubeSpacing = 0.1f;

    [SerializeField] private GameObject snapPointFab;
    [SerializeField] protected Transform spawnPoint;

    protected List<SnapPoint> snapPoints = new List<SnapPoint>();

    public class SnapPoint : SnapListener
    {
        public SnapLocation snapLocation;
        public ObjectInstance parameter = null;

        public SnapPoint(SnapLocation snapLocation)
        {
            this.snapLocation = snapLocation;
            snapLocation.snapListener = this;
        }

        public override void onSnap(Transform snapped)
        {
            parameter = snapped.GetComponent<ObjectInstance>();
        }

        public override void onUnsnap()
        {
            parameter = null;
        }
    }



    private void initialiseSnapPoints()
    {
        if (methodSave == null || methodSave.methodDeclaration == null)
            return;

        Block parametersParent = getParametersParent();
        List<Block> parameterNameBlocks = parametersParent.getBlocksOfType(BlockManager.NAME);
        int noOfParameters = parameterNameBlocks.Count;

        Transform openingL = transform.Find("OpeningL");
        Transform platform = openingL.GetChild(0);

        while (snapPoints.Count != noOfParameters)
        {
            if (snapPoints.Count > noOfParameters)
            {
                // too many snap point
                int lastIndex = snapPoints.Count - 1;

                // destroy snap location
                Destroy(snapPoints[lastIndex].snapLocation.gameObject);
                
                // remove snap point object
                snapPoints.RemoveAt(0);
            }
            else
            {
                // not enough snap points
                // spawn one and move it
                Transform snapLocation = Instantiate(snapPointFab, openingL).transform;
                snapLocation.transform.localPosition = new Vector3(cubeSpacing * -(snapPoints.Count + 1), 0f, 0f);
                
                // add snap point object
                snapPoints.Add(new SnapPoint(snapLocation.GetComponentInChildren<SnapLocation>()));
            }
        }

        platform.localScale = new Vector3(cubeSpacing * (snapPoints.Count + 1), 1f, 1f);
    }
    public override void scaleWindow()
    {
        base.scaleWindow();

        Transform openingR = transform.Find("OpeningR");
        openingR.transform.localPosition = new Vector3(width, -height / 2f, 0f);

        initialiseSnapPoints();
    }

    protected abstract Block getParametersParent();

    public abstract void run();
}
