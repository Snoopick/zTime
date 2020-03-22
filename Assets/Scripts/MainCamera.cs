using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

/// <summary>
/// A simple free camera to be added to a Unity game object.
/// 
/// Keys:
///	wasd / arrows	- movement
///	q/e 			- up/down (local space)
///	r/f 			- up/down (world space)
///	pageup/pagedown	- up/down (world space)
///	hold shift		- enable fast movement mode
///	right mouse  	- enable free look
///	mouse			- free look / rotation
///     
/// </summary>

public class MainCamera : MonoBehaviour
{
    /// <summary>
    /// Normal speed of camera movement.
    /// </summary>
    public float movementSpeed = 10f;

    /// <summary>
    /// Speed of camera movement when shift is held down,
    /// </summary>
    public float fastMovementSpeed = 100f;

    /// <summary>
    /// Sensitivity for free look.
    /// </summary>
    public float freeLookSensitivity = 3f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel.
    /// </summary>
    public float zoomSensitivity = 10f;

    /// <summary>
    /// Amount to zoom the camera when using the mouse wheel (fast mode).
    /// </summary>
    public float fastZoomSensitivity = 50f;

    /// <summary>
    /// Set to true when free looking (on right mouse button).
    /// </summary>
    private bool looking = false;


    public float minX = 0;
    public float maxX = 0;
    private float minY = 2;
    private float maxY = 15;
    public float minZ = 0;
    public float maxZ = 0;
    

    void Update()
    {
        var fastMode = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        var movementSpeed = fastMode ? this.fastMovementSpeed : this.movementSpeed;

        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            var tmpPosition = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (-transform.right * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            var tmpPosition = transform.position + (transform.right * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (transform.right * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            var tmpPosition = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (transform.forward * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            var tmpPosition = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (-transform.forward * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            var tmpPosition = transform.position + (transform.up * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (transform.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            var tmpPosition = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (-transform.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.R) || Input.GetKey(KeyCode.PageUp))
        {
            var tmpPosition = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (Vector3.up * movementSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.F) || Input.GetKey(KeyCode.PageDown))
        {
            var tmpPosition = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + (-Vector3.up * movementSpeed * Time.deltaTime);
        }

        if (looking)
        {
            float newRotationX = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * freeLookSensitivity;
            float newRotationY = transform.localEulerAngles.x - Input.GetAxis("Mouse Y") * freeLookSensitivity;
            transform.localEulerAngles = new Vector3(newRotationY, newRotationX, 0f);
        }

        float axis = Input.GetAxis("Mouse ScrollWheel");
        if (axis != 0)
        {
            var zoomSensitivity = fastMode ? this.fastZoomSensitivity : this.zoomSensitivity;
            var tmpPosition = transform.position + transform.forward * axis * zoomSensitivity;
            if (!canMove(tmpPosition))
            {
                return;
            }
            
            transform.position = transform.position + transform.forward * axis * zoomSensitivity;
        }

        if (Input.GetKeyDown(KeyCode.Mouse2))
        {
            StartLooking();
        }
        else if (Input.GetKeyUp(KeyCode.Mouse2))
        {
            StopLooking();
        }
    }

    private bool canMove(Vector3 position)
    {
        var posX = position.x;
        var posY = position.y;
        var posZ = position.z;

        if ((posX >= minX && posX <= maxX) && (posY >= minY && posY <= maxY) && (posZ >= minZ && posZ <= maxZ))
        {
            return true;
        }

        return false;
    }
    
    void OnDisable()
    {
        StopLooking();
    }

    /// <summary>
    /// Enable free looking.
    /// </summary>
    public void StartLooking()
    {
        looking = true;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    /// <summary>
    /// Disable free looking.
    /// </summary>
    public void StopLooking()
    {
        looking = false;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }
}
