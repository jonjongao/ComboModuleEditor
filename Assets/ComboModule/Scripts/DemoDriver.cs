using UnityEngine;
using System.Collections;

public class DemoDriver : MonoBehaviour
{
    private ComboModule cm;
    private Rigidbody2D body;
    public float timeScale = 1f;
    public Vector2 axis;

    void Start()
    {
        cm = GetComponent<ComboModule>();
        body = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        //cm.SetActivator(Input.GetMouseButton(0));
        Time.timeScale = timeScale;

        if (Input.GetKeyDown(KeyCode.Space))
        {
            cm.SetStagger(3f);
        }

        if (Input.GetMouseButton(0))
        {
            cm.SetActivator(true,0);
        }
        else if (Input.GetMouseButton(1))
        {
            cm.SetActivator(true,1);
        }

        axis = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

        body.velocity = new Vector2(axis.x, 0f);
        if (axis.x == 1f) transform.eulerAngles = new Vector3(0, 180f, 0);
        else if (axis.x == -1f) transform.eulerAngles = new Vector3(0, 0, 0);
    }


}