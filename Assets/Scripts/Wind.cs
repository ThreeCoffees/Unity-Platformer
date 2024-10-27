using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Wind : MonoBehaviour
{
    [SerializeField] private float direction = 0.0f;
    [SerializeField] private float strength = 30.0f;

    public Vector2 windForce {get; private set;}

    Material material;
    // Start is called before the first frame update
    void Start()
    {
        material = GetComponent<UnityEngine.U2D.SpriteShapeRenderer>().material;
        material.SetFloat("_Rotation", direction * Mathf.Deg2Rad);
        material.SetFloat("_Speed", strength / 30.0f);

        windForce = new Vector2(Mathf.Cos(direction * Mathf.Deg2Rad), Mathf.Sin(direction * Mathf.Deg2Rad)) * strength;
        Debug.Log(windForce);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

