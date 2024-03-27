using UnityEngine;
using Unity.Netcode;

public class CrystalBlink : NetworkBehaviour
{
    [SerializeField] MeshRenderer mr;
    [SerializeField] float cycleDuration;
    Material mat;
    float alpha;
    Color oldC;
    Color newC;

    void Start(){
        mat = mr.material;
    }

    // Update is called once per frame
    void Update()
    {
        alpha = Mathf.PingPong(Time.time, cycleDuration) / (3 * cycleDuration);
        oldC = mat.GetColor("_BaseColor");
        newC = new Color(oldC.r, oldC.g, oldC.b, alpha);
        mat.SetColor("_BaseColor", newC);
    }
}
