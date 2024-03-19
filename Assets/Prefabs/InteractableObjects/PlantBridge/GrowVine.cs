using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class GrowVine : NetworkBehaviour
{
    public List<MeshRenderer> growVinesMeshes;
    public float timeToGrow = 5;
    public float refreshRate = 0.05f;
    [Range(0,1)]
    public float minGrow = 0.2f;
    [Range(0,1)]
    public float maxGrow = 0.98f;

    public List<Material> growVinesMaterials = new List<Material>();
    private bool fullyGrown;

    void Start()
    {
        for(int i=0; i<growVinesMeshes.Count; i++)
        {
            for(int j=0; j<growVinesMeshes[i].materials.Length; j++)
            {
                if(growVinesMeshes[i].materials[j].HasProperty("Grow_"))
                {
                    growVinesMeshes[i].materials[j].SetFloat("Grow_", minGrow);
                    growVinesMaterials.Add(growVinesMeshes[i].materials[j]);
                }
            }
        }
    }

    public void GrowVines(){
        for(int i=0; i<growVinesMaterials.Count; i++)
        {
            Debug.Log("starting coroutine");
            StartCoroutine(GrowVineCoroutine(growVinesMaterials[i]));
        }
    }

    IEnumerator GrowVineCoroutine (Material mat)
    {
        float growValue = mat.GetFloat("Grow_");

        if(!fullyGrown)
        {
            while(growValue < maxGrow)
            {
                growValue += 1/(timeToGrow/refreshRate);
                mat.SetFloat("Grow_", growValue);

                yield return new WaitForSeconds (refreshRate);
            }
        }
        else
        {
            while(growValue > minGrow)
            {
                growValue = 1/(timeToGrow/refreshRate);
                mat.SetFloat("Grow_", growValue);

                yield return new WaitForSeconds (refreshRate);
            }
        }

        if(growValue >= maxGrow)
            fullyGrown = true;
        else
            fullyGrown = false;
    }
}


