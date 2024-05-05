using System.Collections;
using UnityEditor;
using UnityEngine;

public class Laser : MonoBehaviour
{
    BoxCollider2D boxCollider;
    LineRenderer lineRenderer;
    public float delta;
    [Range(0,1)] public float maxWidth;
    public Vector3 start;
    public Vector3 end;
    private bool goingUp;
    public bool horizontal;

    public GameObject startVFX;
    public GameObject endVFX;
    public GameObject startParticle;
    public GameObject endParticle;

    // Start is called before the first frame update
    void Start()
    {
        boxCollider = GetComponent<BoxCollider2D>();
        lineRenderer = GetComponent<LineRenderer>();
        goingUp = true;
        lineRenderer.positionCount = 2;
        this.transform.position = start;
        Vector3[] list = new Vector3[2];
        list[0] = start;
        list[1] = end;
        startVFX.transform.position = start;
        endVFX.transform.position = end;
        lineRenderer.SetPositions(list);
        lineRenderer.startWidth = maxWidth;
        lineRenderer.endWidth = maxWidth;
        lineRenderer.enabled = true;

        if (horizontal)
        {
            boxCollider.size = new Vector2(Mathf.Abs(end.x-start.x), maxWidth);
            boxCollider.offset = new Vector2(boxCollider.size.x / 2, 0);
            startParticle.transform.eulerAngles = new Vector3(0, 90, 0);
            endParticle.transform.eulerAngles = new Vector3(0, -90, 0);
        }
        else
        {
            boxCollider.size = new Vector2(maxWidth, Mathf.Abs(end.y-start.y));
            boxCollider.offset = new Vector2(0, -boxCollider.size.y / 2);
            startParticle.transform.eulerAngles = new Vector3(90, 0, 0);
            endParticle.transform.eulerAngles = new Vector3(-90, 0, 0);
        }

        startParticle.SetActive(false);
        endParticle.SetActive(false);
        StartCoroutine("LaserPlay");
    }

    // Update is called once per frame
    IEnumerator LaserPlay()
    {
        while (true)
        {
            if(lineRenderer.startWidth < maxWidth && goingUp)
            {
                lineRenderer.startWidth += delta;
                lineRenderer.endWidth += delta;
                yield return null;
            }
            if (lineRenderer.startWidth >= maxWidth)
            {
                goingUp = false;
                boxCollider.enabled = true;
                startParticle.SetActive(true);
                endParticle.SetActive(true);
                yield return new WaitForSeconds(2f);
                startParticle.SetActive(false);
                endParticle.SetActive(false);
                boxCollider.enabled = false;
                lineRenderer.startWidth -= delta;
                lineRenderer.endWidth -= delta;
            }
            if(lineRenderer.startWidth < maxWidth && !goingUp)
            {
                lineRenderer.startWidth -= delta;
                lineRenderer.endWidth -= delta;
                yield return null;
            }
            if(lineRenderer.startWidth <= 0f)
            {
                goingUp = true;
                lineRenderer.enabled = false;
                yield return new WaitForSeconds(2f);
                lineRenderer.enabled = true;
            }
        }
    }

    private void OnDrawGizmos()
    {
        //DrawGizmoPoint(start, 0.2f, Color.green);

        //DrawGizmoPoint(end, 0.2f, Color.green);
    }
}

[CustomEditor(typeof(Laser), true)]
[InitializeOnLoad]
public class DraggableLaserPoint : Editor
{
    private void OnSceneGUI()
    {
        Laser laser = target as Laser;
        Handles.color = Color.green;
        EditorGUI.BeginChangeCheck();
        Vector3 oldStartPos = laser.start;
        Vector3 oldEndPos = laser.end;
        Vector3 newStartPos = Handles.PositionHandle(oldStartPos, Quaternion.identity);
        Handles.Label(newStartPos - new Vector3(0.1f, 0.25f, 0), "start");
        Vector3 newEndPos = Handles.PositionHandle(oldEndPos, Quaternion.identity);
        Handles.Label(newEndPos - new Vector3(0.1f, 0.25f, 0), "end");
        if (EditorGUI.EndChangeCheck())
        {
            Undo.RecordObject(target, "Free Move Handle");
            laser.start = newStartPos;
            laser.end = newEndPos;
        }
    }
}