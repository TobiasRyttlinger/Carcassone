using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
public class CameraMovement : MonoBehaviour
{
    private float panBorderThickness = 10f;
    private float panSpeed = -50f;
    public Vector2 panLimit;
    public CinemachineVirtualCamera Camera;
    private float cameraFOV = 50f;
    private float zoomSpeed = 5f;
    private float maxZoom = 50f;
    private float minY = 20f;
    private float maxY = 120f;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        panSpeed =  -Vector3.Distance(transform.position, Camera.transform.position)/((maxZoom-cameraFOV+15)*0.01f);
      //  Debug.Log("cameraFOV: " + cameraFOV);
        //zoomSpeed /= Vector3.Distance(transform.position, Camera.transform.position)*0.5f;
        Vector3 pos = Vector3.zero;

        if (Input.GetKey("w") || Input.mousePosition.y >= Screen.height - panBorderThickness)
        {
            pos.z -= 1f;
        }
        if (Input.GetKey("s") || Input.mousePosition.y <= panBorderThickness)
        {
             pos.z += 1f;
        }
        if (Input.GetKey("a") || Input.mousePosition.x >= Screen.width - panBorderThickness)
        {
              pos.x -= 1f;
        }
        if (Input.GetKey("d") || Input.mousePosition.x <= panBorderThickness)
        {
              pos.x += 1f;
        }

        Vector3 MoveDir = transform.forward * pos.z + transform.right * pos.x;
        transform.position += pos * panSpeed*0.5f *Time.deltaTime;

        zoomFunc();
        cameraRotateFunc();

    }

    private void zoomFunc()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (Input.mouseScrollDelta.y > 0)
        {
            cameraFOV += zoomSpeed;
        }
        if (Input.mouseScrollDelta.y < 0)
        {
            cameraFOV -= zoomSpeed;
        }
        cameraFOV = Mathf.Clamp(cameraFOV,3,maxZoom);
        Mathf.Lerp(Camera.m_Lens.FieldOfView, cameraFOV, 10f * Time.deltaTime);
        Camera.m_Lens.FieldOfView = cameraFOV;

    }

    private void cameraRotateFunc()
    {
        float rotateDir = 0f;

        if (Input.GetKey("q")) rotateDir += 1f;

        if (Input.GetKey("e")) rotateDir -= 1f;
        float rotationSpeed = 300f;

        transform.eulerAngles += new Vector3(0, rotateDir * rotationSpeed * Time.deltaTime, 0);
    }

  
}
