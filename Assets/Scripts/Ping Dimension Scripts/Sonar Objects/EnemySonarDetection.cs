using UnityEngine;
using System.Collections;

public class GhostEffect : MonoBehaviour
{
    public Material ghostMaterial;
    public float fadeSpeed = 2f;
    public float sonarDisableDelay = 5f;
    public void Start()
    {
        StartCoroutine(SonarDisableDelay());

    }
    
    IEnumerator SonarDisableDelay()
    {
        yield return new WaitForSeconds(sonarDisableDelay);
        var sonarDetection = GetComponent<SonarDetection>();

        if (sonarDetection != null && sonarDetection.enabled)
        {
            sonarDetection.enabled = false;
        }
    }
    public void CreateGhostSnapshot()
    {
        GameObject ghostObj = new GameObject("GhostSnapshot");
        ghostObj.transform.position = transform.position;
        ghostObj.transform.rotation = transform.rotation;

        MeshFilter mf = ghostObj.AddComponent<MeshFilter>();
        MeshRenderer mr = ghostObj.AddComponent<MeshRenderer>();

        Mesh bakedMesh = new Mesh();
        GetComponentInChildren<SkinnedMeshRenderer>().BakeMesh(bakedMesh);
        mf.mesh = bakedMesh;

        Material instanceMat = new Material(ghostMaterial);
        mr.material = instanceMat;

        StartCoroutine(FadeAndDestroy(ghostObj, instanceMat));
    }

    IEnumerator FadeAndDestroy(GameObject obj, Material mat)
    {
        float intensity = 1f; 

        while (intensity > 0)
        {
            intensity -= Time.deltaTime * fadeSpeed;
            mat.SetFloat("_Intensity", intensity);
            
            yield return null;
        }

        Destroy(obj);
    }
}