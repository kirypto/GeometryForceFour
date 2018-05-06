using UnityEngine;

public class TotemScript : MonoBehaviour
{
    public bool ShouldBlink;

    [SerializeField] private float _xPosMax = 10f;
    [SerializeField] private float _xPosMin = -10f;
    [SerializeField] private float _yPosMax = 10f;
    [SerializeField] private float _yPosMin = -10f;

    [SerializeField] private float _reboundForce = 1f;

    private void Update()
    {
        if (transform.position.x > _xPosMax ||
            transform.position.y > _yPosMax ||
            transform.position.x < _xPosMin ||
            transform.position.y < _yPosMin)
        {
            Vector2 vector2 = Vector2.zero - (Vector2) transform.position;
            GetComponent<Rigidbody2D>().AddForce(vector2 * _reboundForce);
        }
    }
}
