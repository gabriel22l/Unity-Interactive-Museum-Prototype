using UnityEngine;

public class VRFollowCamera : MonoBehaviour
{
    private Camera cm;
    private Vector3 offset = new Vector3(0, 0f, 0.85f);
    private float scale = 0.0008f;

    [SerializeField] private float positionSmoothSpeed = 5f;
    [SerializeField] private float rotationSmoothSpeed = 5f;

    private void Awake()
    {
        cm = Camera.main;
        transform.localScale = scale * Vector3.one;
    }

    private void LateUpdate()
    {
        if (!cm) return;

        Vector3 targetPosition = cm.transform.position + cm.transform.rotation * offset;
        Quaternion targetRotation = Quaternion.LookRotation(cm.transform.forward);
        targetRotation.z = 0;
        targetRotation.x = 0;

        float posT = 1f - Mathf.Exp(-positionSmoothSpeed * Time.deltaTime);
        float rotT = 1f - Mathf.Exp(-rotationSmoothSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(transform.position, targetPosition, posT);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, rotT);
    }
}
