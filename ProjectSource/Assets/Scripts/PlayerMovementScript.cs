using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    [SerializeField] private float _movementForce = 1f;
    [SerializeField] private float _xPosMax = 10f;
    [SerializeField] private float _xPosMin = -10f;
    [SerializeField] private float _yPosMax = 10f;
    [SerializeField] private float _yPosMin = -10f;

    private Rigidbody2D _rigidbody;

    private void Awake()
    {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(new Vector2(Input.mousePosition.x, Input.mousePosition.y));
        Vector2 vectorToMouse = ((Vector2)(mousePosition - transform.position)).normalized;
        transform.right = vectorToMouse;

        if (Input.GetKey(KeyCode.W) && transform.position.y < _yPosMax)
        {
            _rigidbody.AddForce(Vector2.up * _movementForce);
        }

        if (Input.GetKey(KeyCode.A) && transform.position.x > _xPosMin)
        {
            _rigidbody.AddForce(Vector2.left * _movementForce);
        }

        if (Input.GetKey(KeyCode.S) && transform.position.y > _yPosMin)
        {
            _rigidbody.AddForce(Vector2.down * _movementForce);
        }

        if (Input.GetKey(KeyCode.D) && transform.position.x < _xPosMax)
        {
            _rigidbody.AddForce(Vector2.right * _movementForce);
        }
    }
}
