using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] private float _movementForce = 1f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 vectorToMouse = ((Vector2)(mousePosition - transform.position)).normalized;
        transform.right = vectorToMouse;

        if (Input.GetKey(KeyCode.W))
        {
            _rigidbody.AddForce(Vector2.up * _movementForce);
        }

        if (Input.GetKey(KeyCode.A))
        {
            _rigidbody.AddForce(Vector2.left * _movementForce);
        }

        if (Input.GetKey(KeyCode.S))
        {
            _rigidbody.AddForce(Vector2.down * _movementForce);
        }

        if (Input.GetKey(KeyCode.D))
        {
            _rigidbody.AddForce(Vector2.right * _movementForce);
        }
    }
}
