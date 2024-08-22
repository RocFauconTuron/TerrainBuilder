using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CubeInfo : MonoBehaviour
{
    public List<neightborCubeInfo> neightborList;
    public int indexXCube;
    public int indexZCube;
    public Terrain terrain;
    public GameObject neightBoorLeft;
    public GameObject neightBoorRight;
    public GameObject neightBoorUp;
    public GameObject neightBoorDown;
    private void Start()
    {
        
    }
    public void InitializeList()
    {
        indexXCube = Mathf.RoundToInt(transform.position.x / transform.localScale.x);
        indexZCube = Mathf.RoundToInt(transform.position.z / transform.localScale.z);

        RaycastHit hit;
        LayerMask layerMask = LayerMask.GetMask("Ground");

        neightborList = new List<neightborCubeInfo>();
        neightborList.Add(new neightborCubeInfo { neightborPos = new Vector3(transform.position.x + transform.localScale.x, transform.position.y, transform.position.z), created = false, indexX = Mathf.RoundToInt((transform.position.x + transform.localScale.x)/ transform.localScale.x), indexZ = Mathf.RoundToInt(transform.position.z / transform.localScale.z) });        
        if(Physics.Raycast(transform.position, (neightborList[neightborList.Count-1].neightborPos - transform.position).normalized, out hit, transform.localScale.x, layerMask))
        {
            neightBoorUp = hit.transform.gameObject;
            neightborList[neightborList.Count-1] = new neightborCubeInfo { neightborPos = new Vector3(transform.position.x + transform.localScale.x, transform.position.y, transform.position.z), created = true, indexX = Mathf.RoundToInt((transform.position.x + transform.localScale.x) / transform.localScale.x), indexZ = Mathf.RoundToInt(transform.position.z / transform.localScale.z) };
        }

        neightborList.Add(new neightborCubeInfo { neightborPos = new Vector3(transform.position.x - transform.localScale.x, transform.position.y, transform.position.z), created = false, indexX = Mathf.RoundToInt((transform.position.x - transform.localScale.x) / transform.localScale.x), indexZ = Mathf.RoundToInt(transform.position.z / transform.localScale.z) });
        if (Physics.Raycast(transform.position, (neightborList[neightborList.Count - 1].neightborPos - transform.position).normalized, out hit, transform.localScale.x, layerMask))
        {
            neightBoorDown = hit.transform.gameObject;
            neightborList[neightborList.Count - 1] = new neightborCubeInfo { neightborPos = new Vector3(transform.position.x - transform.localScale.x, transform.position.y, transform.position.z), created = true, indexX = Mathf.RoundToInt((transform.position.x - transform.localScale.x) / transform.localScale.x), indexZ = Mathf.RoundToInt(transform.position.z / transform.localScale.z) };
        }

        neightborList.Add(new neightborCubeInfo { neightborPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z), created = false, indexX = Mathf.RoundToInt(transform.position.x / transform.localScale.x), indexZ = Mathf.RoundToInt((transform.position.z + transform.localScale.z)/ transform.localScale.z) });
        if (Physics.Raycast(transform.position, (neightborList[neightborList.Count - 1].neightborPos - transform.position).normalized, out hit, transform.localScale.x, layerMask))
        {
            neightBoorRight = hit.transform.gameObject;
            neightborList[neightborList.Count - 1] = new neightborCubeInfo { neightborPos = new Vector3(transform.position.x, transform.position.y, transform.position.z + transform.localScale.z), created = true, indexX = Mathf.RoundToInt(transform.position.x / transform.localScale.x), indexZ = Mathf.RoundToInt((transform.position.z + transform.localScale.z) / transform.localScale.z) };
        }

        neightborList.Add(new neightborCubeInfo { neightborPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z), created = false, indexX = Mathf.RoundToInt(transform.position.x / transform.localScale.x), indexZ = Mathf.RoundToInt((transform.position.z - transform.localScale.z) / transform.localScale.z) });
        if (Physics.Raycast(transform.position, (neightborList[neightborList.Count - 1].neightborPos - transform.position).normalized, out hit, transform.localScale.x, layerMask))
        {
            neightBoorLeft = hit.transform.gameObject;
            neightborList[neightborList.Count - 1] = new neightborCubeInfo { neightborPos = new Vector3(transform.position.x, transform.position.y, transform.position.z - transform.localScale.z), created = true, indexX = Mathf.RoundToInt(transform.position.x / transform.localScale.x), indexZ = Mathf.RoundToInt((transform.position.z - transform.localScale.z) / transform.localScale.z) };
        }
    }
    public struct neightborCubeInfo
    {
        public Vector3 neightborPos;
        public bool created;
        public int indexX;
        public int indexZ;
        public GameObject cubeNeightbor;
    }
}
