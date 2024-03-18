using System.Collections.Generic;
using UnityEngine;

public class InflateLine: MonoBehaviour {

    private LineRenderer _line;
    private float _inflationMultiplier = 1;
    private float _startTime;
    private float _duration = 1;
    
    void Awake() {
        _line = GetComponent<LineRenderer>();
        _startTime = Time.time;
    } 

    public void SetPoints(List<Vector3> points) {
        _line.positionCount = points.Count;
        _line.SetPositions(points.ToArray());
    }

    public void SetInflationSize(float inflationMultiplier) {
        _inflationMultiplier = inflationMultiplier;
    }

    public void SetDuration(float duration) {
        _duration = duration;
    }

    void Update() {
        

        float percentDone = (Time.time - _startTime) / _duration;
        if (percentDone > 1) { Destroy(gameObject); }

        else {
            _line.widthMultiplier = _inflationMultiplier * percentDone;
            var startCol = _line.startColor;
            var endCol = _line.endColor;
            _line.startColor = new Color(startCol.r, startCol.g, startCol.b, 0.3f - percentDone);
            _line.endColor = new Color(endCol.r, endCol.g, endCol.b, 0.3f - percentDone);
        }
    }
}
