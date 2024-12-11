using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WindmillGenerator : MonoBehaviour
{
    [SerializeField] private int platformsNum = 5;
    [SerializeField] private float radius = 5.0f;
    [SerializeField] private GameObject gO;
    [SerializeField] private GameObject empty;

    private GameObject[] platforms;
    private GameObject[] points;
    private Vector2[] positions;

    // Start is called before the first frame update
    void Awake()
    {
        platforms = new GameObject[platformsNum];
        positions = new Vector2[platformsNum];
        points = new GameObject[platformsNum];

        for(int i = 0; i < platformsNum; i++){
            float angle = (float)i/platformsNum * 2 * Mathf.PI;
            float y = transform.position.y + radius * Mathf.Sin(angle);
            float x = transform.position.x + radius * Mathf.Cos(angle);
            positions[i] = new Vector2(x, y);
        }

        for(int i = 0; i < platformsNum; i++){
            points[i] = new GameObject("point" + i);
            points[i].transform.position = positions[i];
        }

        for(int i = 0; i < platformsNum; i++){
            platforms[i] = Instantiate(gO, positions[i], Quaternion.identity);
            platforms[i].GetComponent<MovingPlatform>().SetPath(points);
            platforms[i].GetComponent<MovingPlatform>().SetIndex(i);
        }

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
