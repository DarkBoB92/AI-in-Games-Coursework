using System;
using System.IO;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] float speed = 1;
    [SerializeField] float zoomSpeed = 1;
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
            if (cam.orthographicSize >= 0)
            {
                gameObject.transform.position += Vector3.up * Time.deltaTime * speed;
            }
            else if (cam.orthographicSize < 0)
            {
                gameObject.transform.position -= Vector3.up * Time.deltaTime * speed;
            }
        }

        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S))
        {
            if (cam.orthographicSize >= 0)
            {
                gameObject.transform.position += Vector3.down * Time.deltaTime * speed;
            }
            else if (cam.orthographicSize < 0)
            {
                gameObject.transform.position -= Vector3.down * Time.deltaTime * speed;
            }
        }

        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D))
        {
            if (cam.orthographicSize >= 0)
            {
                gameObject.transform.position += Vector3.right * Time.deltaTime * speed;
            }
            else if (cam.orthographicSize < 0)
            {
                gameObject.transform.position -= Vector3.right * Time.deltaTime * speed;
            }
        }

        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A))
        {
            if (cam.orthographicSize >= 0)
            {
                gameObject.transform.position += Vector3.left * Time.deltaTime * speed;
            }
            else if (cam.orthographicSize < 0)
            {
                gameObject.transform.position -= Vector3.left * Time.deltaTime * speed;
            }
        }

        float scroll = Input.GetAxis("Mouse ScrollWheel");

        if (scroll != 0f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
        }

        if (Input.GetKey(KeyCode.Q))
        {
            cam.orthographicSize -= Time.deltaTime * zoomSpeed;
        }

        if (Input.GetKey(KeyCode.E))
        {
            cam.orthographicSize += Time.deltaTime * zoomSpeed;
        }
    }
}
