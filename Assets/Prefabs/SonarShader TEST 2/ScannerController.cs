using System.Collections;

using System.Collections.Generic;

using UnityEngine;



public class ScannerController : MonoBehaviour

{

    [Header("Speed")]

    public float Speed = 10f;

    public float fadeDuration = 0.8f;



    [Header("Scaling Settings")]

    private float maxScale = 0f;

    private float currentFade = 1.0f;

    private bool initialized = false;

    private Material mat;



    public void Setup(float strength)

    {

        mat = GetComponent<Renderer>().material;

        //Circle size based on voice

        maxScale = strength;



        if (mat != null) mat.SetFloat("_Fade", 1.0f);

        initialized = true;

    }



    void Update()

    {

        if (!initialized)

            return;



        float growing = Speed * Time.deltaTime;

        transform.localScale += new Vector3(growing, growing, growing);



        if (mat != null)

            mat.SetFloat("_Radius", transform.localScale.x);



        currentFade -= Time.deltaTime / fadeDuration;



        if (mat != null)

        {

            mat.SetFloat("_Fade", Mathf.Max(0, currentFade));

        }



        if (currentFade <= 0)

        {

            Destroy(gameObject);

        }

    }

}