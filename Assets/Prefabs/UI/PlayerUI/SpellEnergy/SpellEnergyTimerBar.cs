using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellEnergyTimerBar : MonoBehaviour
{
    public Transform leftPoint;
    public GameObject leftBar;
    public Transform mainPivot;
    public Transform barPivot;
    float numSegments;
    float totalSegments = 4;

    public void SetNumSegments(int s){
        numSegments = s;
        mainPivot.transform.localScale = new Vector3(numSegments/totalSegments, 1, 1);
        leftBar.transform.position = leftPoint.transform.position;
    }

    public void SetFillAmount(float fillAmount){
        barPivot.transform.localScale = new Vector3(fillAmount, 1, 1);
        leftBar.transform.position = leftPoint.transform.position;
    }
}
