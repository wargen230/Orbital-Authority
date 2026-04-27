using UnityEngine;

public class FirstPersonController : MonoBehaviour
{
    [Header("Movement")]
    public float walkSpeed = 5f;
    public float gravity = -9.81f;
    
    [Header("Camera")]
    public Transform playerCamera; // Ваша камера как дочерний объект
    public float mouseSensitivity = 2f;
    
    private CharacterController controller;
    private Vector3 velocity;
    private float xRotation = 0f;
    
    void Start()
    {
        controller = GetComponent<CharacterController>();
        
        // Блокируем курсор в центре экрана
        Cursor.lockState = CursorLockMode.Locked;
    }
    
    void Update()
    {
        // Вращение камеры (только по вертикали)
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;
        
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);
        
        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        
        // Поворот игрока (по горизонтали)
        transform.Rotate(Vector3.up * mouseX);
        
        // Движение
        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");
        
        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * walkSpeed * Time.deltaTime);
        
        // Гравитация
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
        
        // Сброс скорости при касании земли
        if (controller.isGrounded && velocity.y < 0)
        {
            velocity.y = -2f;
        }
    }
}