using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] Transform targetRotate;
    [SerializeField] float cameraSens;
    float rotX = 0;
    float rotY = 0;
    Vector3 currentRot;
    Vector3 currentPos;
    Vector3 smoothVel = Vector3.zero;
    [SerializeField] float smoothTimeRot;
    [SerializeField] float smoothTimeMove;
    Vector3 anteriorMousePos = Vector3.zero;
    Camera camera;
    [SerializeField] LayerMask layer;
    [SerializeField] float moveSpeed;
    [SerializeField] float zoomSpeed;
    [SerializeField] float minZoom;
    [SerializeField] float maxZoom;

    float posX;
    float posY;
    public bool buildingState;
    public bool transitionState;
    Vector3 lastBuildingPos;

    [SerializeField] float timeTransition;
    [SerializeField] AnimationCurve curveTransition;
    GameManager gameManager;
    float doubleClickTimer = 0;
    Vector3 anteriorDoubleClickPos;

    private Vector2 lastMousePosition;

    [SerializeField] float dragPanSpeed;
    [SerializeField] float moveSpeedDrag = 50f;
    float timerCameraDrag = 0;
    private void Awake()
    {
        currentRot = targetRotate.localEulerAngles;
        currentPos = targetRotate.position;
        rotX = targetRotate.localEulerAngles.y;
        rotY = targetRotate.localEulerAngles.x;
        posX = targetRotate.position.x;
        posY = targetRotate.position.z;
        camera = GetComponent<Camera>();
        gameManager = FindObjectOfType<GameManager>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        doubleClickTimer += Time.deltaTime;
    }
    private void OnEnable()
    {
        GameEvents.current.onButtonExpandEvent += TransitionExpand;
    }
    private void OnDisable()
    {
        GameEvents.current.onButtonExpandEvent -= TransitionExpand;
    }
    private void LateUpdate()
    {

        if(gameManager._gameState != gameState.transition)
        {
            if (Input.GetMouseButton(0) && Input.GetKey(KeyCode.LeftAlt))
            {
                rotX += Input.GetAxis("Mouse X") * cameraSens;
                rotY -= Input.GetAxis("Mouse Y") * cameraSens;

                rotY = Mathf.Clamp(rotY, 20, 70);

                Vector3 nextRot = new Vector3(rotY, rotX);
                currentRot = Vector3.SmoothDamp(currentRot, nextRot, ref smoothVel, smoothTimeRot);
                targetRotate.localEulerAngles = currentRot;
            }
            if (Input.GetMouseButtonDown(1))
            {
                if (doubleClickTimer < 0.3f)
                {
                    Vector3 mousePosition = Input.mousePosition;
                    Ray mRay = camera.ScreenPointToRay(mousePosition);
                    RaycastHit hit;

                    if (Physics.Raycast(mRay, out hit, Mathf.Infinity, layer))
                    {
                        StopCoroutine(MoveCameraToCube(anteriorDoubleClickPos));
                        StartCoroutine(MoveCameraToCube(hit.transform.position));
                        anteriorDoubleClickPos = hit.transform.position;
                    }                   
                }
                doubleClickTimer = 0;
            }
            timerCameraDrag += Time.deltaTime;
            if (Input.GetMouseButton(2))
            {
                Vector3 inputDir = new Vector3(0, 0, 0);

                if (timerCameraDrag > 0.1)
                {
                    lastMousePosition = Input.mousePosition;
                }

                Vector2 mouseMovementDelta = (Vector2)Input.mousePosition - lastMousePosition;

                inputDir.x = mouseMovementDelta.x * dragPanSpeed;
                inputDir.z = mouseMovementDelta.y * dragPanSpeed;

                lastMousePosition = Input.mousePosition;

                Vector3 moveDir = targetRotate.forward * -inputDir.z + targetRotate.right * -inputDir.x;
                Debug.Log(moveDir);

                targetRotate.position += moveDir * moveSpeedDrag * Time.deltaTime;
                targetRotate.position = new Vector3(targetRotate.position.x, 0, targetRotate.position.z);

                timerCameraDrag = 0;
            }

            float scrollInput = Input.mouseScrollDelta.y;

            if (scrollInput != 0.0f)
            {
                // Calcular la nueva distancia
                float distance = Vector3.Distance(transform.position, targetRotate.position);
                distance -= scrollInput * zoomSpeed;
                distance = Mathf.Clamp(distance, minZoom, maxZoom);

                // Mover la cámara a la nueva posición
                Vector3 direction = (transform.position - targetRotate.position).normalized;
                transform.position = targetRotate.position + direction * distance;
            }
        }
       
    }
    IEnumerator MoveCameraToCube(Vector3 endPos)
    {
        Vector3 startPos = targetRotate.position;
        float time = 0;
        while (time < 0.3f)
        {
            time += Time.deltaTime;
            float percentageDuration = time / 0.3f;       
            targetRotate.position = Vector3.Lerp(startPos, endPos, curveTransition.Evaluate(percentageDuration));
            yield return new WaitForEndOfFrame();
        }
    }
    public void TransitionExpand()
    {
        lastBuildingPos = targetRotate.position;
        StartCoroutine(TransitionExpandCoroutine());

    }
    IEnumerator TransitionExpandCoroutine()
    {
        gameManager._gameState = gameState.transition;
        transitionState = true;
        float time = 0;
        Quaternion startRot = targetRotate.rotation;
        Vector3 startPos = targetRotate.position;
        Vector3 startPosCam = transform.localPosition;
        Vector3 direction = (transform.position - targetRotate.position).normalized;
        Vector3 endPosCam = new Vector3(transform.localPosition.x,transform.localPosition.y,-1700);
        while (time < timeTransition)
        {
            time += Time.deltaTime;
            float percentageDuration = time / timeTransition;
            targetRotate.localRotation = Quaternion.Lerp(startRot, Quaternion.Euler(55,-45,0), curveTransition.Evaluate(percentageDuration));
            targetRotate.position = Vector3.Lerp(startPos, Vector3.zero, curveTransition.Evaluate(percentageDuration));
            transform.localPosition = Vector3.Lerp(startPosCam, endPosCam, curveTransition.Evaluate(percentageDuration));

            yield return new WaitForEndOfFrame();
        }
        rotX = targetRotate.localEulerAngles.y;
        rotY = targetRotate.localEulerAngles.x;
        posX = targetRotate.position.x;
        posY = targetRotate.position.z;
        gameManager._gameState = gameState.expand;
    }
}
