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
        if (Input.GetKey(KeyCode.W))
        {
            _rigidbody.AddForce(Vector2.up * _movementForce);
        }
        else if (Input.GetKey(KeyCode.A)) {
            _rigidbody.AddForce(Vector2.left * _movementForce);
        }
        else if (Input.GetKey(KeyCode.S)) {
            _rigidbody.AddForce(Vector2.down * _movementForce);
        }
        else if (Input.GetKey(KeyCode.D)) {
            _rigidbody.AddForce(Vector2.right * _movementForce);
        }
    }
}
