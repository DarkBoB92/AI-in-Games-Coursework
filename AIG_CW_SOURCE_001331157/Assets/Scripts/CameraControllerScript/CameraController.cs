using System;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] float zoomSpeed = 1;
    float minZoomSize = 0;
    [SerializeField] float maxZoomSize = 10;
    [SerializeField] Camera cam;

    private void Awake()
    {
        cam = gameObject.GetComponent<Camera>();
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W))
        {
            gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            gameObject.transform.position += Vector3.down * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            gameObject.transform.position += Vector3.right * Time.deltaTime * speed;
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            gameObject.transform.position += Vector3.left * Time.deltaTime * speed;
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomSize, maxZoomSize);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            cam.orthographicSize -= Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomSize, maxZoomSize);
        }

        if (Input.GetKey(KeyCode.E))
        {
            cam.orthographicSize += Time.deltaTime * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoomSize, maxZoomSize);
        }
    }
}
