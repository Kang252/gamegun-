using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    [Header("Mục tiêu theo dõi (Kéo Player vào đây)")]
    public Transform target;
    
    [Header("Tốc độ bám theo")]
    public float smoothSpeed = 8f;
    
    [Header("Khoảng cách Camera")]
    public Vector3 offset = new Vector3(0, 0, -10f);

    void LateUpdate()
    {
        if (target != null)
        {
            Vector3 desiredPosition = target.position + offset;
            // Di chuyển mượt mà tới vị trí mục tiêu
            transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        }
        else
        {
            // Tự động tìm nhân vật nếu chưa kéo vào
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null) 
            {
                target = playerObj.transform;
            }
        }
    }
}
