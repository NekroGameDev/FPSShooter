using FishNet.Object;
using UnityEngine;

public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform cameraPivot;
    [SerializeField] private Camera _camera;

    private float rotationX;
    private float rotationY;

    private const string mouseX = "Mouse X";
    private const string mouseY = "Mouse Y";

    public Transform GetCameraPivot => cameraPivot;

    public override void OnStartClient()
    {
        base.OnStartClient();

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        if (!IsOwner)
        {
            Destroy(_camera.gameObject);
        }
    }

    private void Update()
    {
        if (!IsOwner) { return; }

        CameraRotation();
    }

    private void CameraRotation()
    {
        rotationX += Input.GetAxis(mouseX);
        rotationY -= Input.GetAxis(mouseY);

        rotationY = Mathf.Clamp(rotationY, -90f, 90f);

        cameraPivot.localRotation = Quaternion.AngleAxis(rotationY, Vector3.right);
        transform.rotation = Quaternion.AngleAxis(rotationX, Vector3.up);
    }
}
