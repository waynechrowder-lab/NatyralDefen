using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Gameplay.Script.Gameplay;

public class PlantsContTest : MonoBehaviour
{
    // Start is called before the first frame update
    void OnEnable()
    {
        gameObject.layer = LayerMask.NameToLayer("Plant");
        GetComponent<PlantBehaviour>().Drop();
    }

    // Update is called once per frame
    void Update()
    {

    }
}
