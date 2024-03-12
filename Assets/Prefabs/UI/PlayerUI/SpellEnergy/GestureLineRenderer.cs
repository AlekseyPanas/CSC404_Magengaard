using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class GestureLineRenderer : MonoBehaviour
{
    [SerializeField] LineRenderer lr;
    public Vector3 center;

    public void SetGesture(Gesture g){
        Vector2 currPoint = new Vector2(0, 0);
        Vector2 currDir = new Vector2(1, 0);
        lr.positionCount = g.Gest.Count + 1;
        lr.SetPosition(0, currPoint);
        float minX = 0;
        float maxX = 0;
        float minY = 0;
        float maxY = 0;
        for(int i = 0; i < g.Gest.Count; i++){
            currDir = Quaternion.AngleAxis(g.Gest[i].RelAng, transform.forward) * currDir.normalized * g.Gest[i].RelRatio;
            currPoint += currDir;
            minX = Math.Min(minX, currPoint.x);
            maxX = Math.Max(maxX, currPoint.x);
            minY = Math.Min(minY, currPoint.y);
            maxY = Math.Max(maxY, currPoint.y);
            lr.SetPosition(i+1, currPoint);
        }
        center = new Vector3((maxX + minX) / 2, (maxY + minY) / 2, 0);
        for(int i = 0; i < g.Gest.Count + 1; i++){
            Vector3 pos = lr.GetPosition(i);
            lr.SetPosition(i, pos - center);
        }
    }
}
