using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;

public class CenterlineCreator : MonoBehaviour {
    LineRenderer lineRenderer;
    public string thename;
    // Use this for initialization
    List<Vector3> beforesmooth;
    List<Vector3> aftersmooth;
    
    public GameObject sphere;
    public GameObject sphereblue;

    private GameObject parent;
    bool mode = false;

    public Color thecolor;
    //[Range(0,0.05f)]
    public float speed = 2.0f;
    public float width = 0.01f;
    public bool mirror = false;
    public bool MoveCamera = false;
    public bool m_Recalculate_Bezier = true;

    void Start () {

        lineRenderer = gameObject.AddComponent<LineRenderer>();
        lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
        lineRenderer.widthMultiplier = width;
        lineRenderer.startColor = thecolor;
        lineRenderer.endColor = thecolor;
        thename = "Assets/Centerline_Vis/TextData/" + thename;

        parent = new GameObject();


        ParseFile(thename);

        lineRenderer.useWorldSpace = false;
         
        if (MoveCamera)
        {
            StartCoroutine(moveCamera(0.15f));
        }
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKeyDown(KeyCode.S))
        {
            mode = !mode;

            if (mode)
            {
                RenderSmooth();
            }
            else
            {
                RenderRaw();
            }
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            speed += 0.05f;

            if(speed > 10.0f)
            {
                speed = 10.0f;
            }
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            speed -= 0.05f;
            if (speed < 0.1f)
            {
                speed = 0.1f;
            }
        }

        if (Input.GetKeyDown(KeyCode.H))
        {
            lineRenderer.enabled = !lineRenderer.enabled;
        }

        if (Input.GetKeyDown(KeyCode.J))
        {
            parent.SetActive(!parent.activeSelf);
        }
    }

    void ParseFile(string name)
    {
        List<Vector3> thepoints = new List<Vector3>();

        try
        {
            // Create an instance of StreamReader to read from a file.
            // The using statement also closes the StreamReader.
            using (StreamReader sr = new StreamReader(name))
            {
                string line;
                // Read and display lines from the file until the end of 
                // the file is reached.
                while ((line = sr.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');
                    Vector3 point = new Vector3(float.Parse(words[0]), float.Parse(words[1]), float.Parse(words[2]));

                    if (mirror)
                    {
                        point = new Vector3(float.Parse(words[0]), float.Parse(words[1]), -float.Parse(words[2]));
                    }

                    thepoints.Add(point);
                    //Debug.Log(point);
                }
            }
        }
        catch (Exception e)
        {
            // Let the user know what went wrong.
            Debug.Log("The file could not be read:");
            Debug.Log(e.Message);
        }

        if (m_Recalculate_Bezier) { 
            //beforesmooth = thepoints;
            beforesmooth = ReducePoints(thepoints.ToArray());


            //thepoints.Reverse();
            Vector3[] lineCurvePoints = beforesmooth.ToArray();
            lineRenderer.positionCount = lineCurvePoints.Length;
            lineRenderer.SetPositions(lineCurvePoints);


            List<Vector3> smoothline = new List<Vector3>();

            float total_length = (lineCurvePoints.Length - lineCurvePoints.Length % 2) / 2;

            for (int i = 0; i < total_length && (2 * i + 2) < lineCurvePoints.Length; i++)
            {
                Vector3 p0 = lineCurvePoints[2*i];
                Vector3 p1 = lineCurvePoints[2*i + 1];
                Vector3 p2 = lineCurvePoints[2*i + 2];
                //Vector3 p3 = lineCurvePoints[3 * i + 3];

                if (2 * i - 1 >= 0)
                {
                    p0 = (lineCurvePoints[2 * i - 1] + p1) / 2;
                }

                if (2 * i + 3 < lineCurvePoints.Length) {
                    p2 = (p1 + lineCurvePoints[2 * i + 3]) / 2;
                }

                if (sphere != null)
                {
                    Instantiate(sphere, p0, Quaternion.identity).transform.SetParent(parent.transform);
                    Instantiate(sphereblue, p1, Quaternion.identity).transform.SetParent(parent.transform);
                    Instantiate(sphere, p2, Quaternion.identity).transform.SetParent(parent.transform);
                }

                smoothline.AddRange(BezierPointsCreator(p0,p1,p2));
            }

            aftersmooth = smoothline;
            Debug.Log(smoothline.ToArray().Length);
            //lineCurvePoints = smoothline.ToArray();
            //lineRenderer.positionCount = lineCurvePoints.Length;
            //lineRenderer.SetPositions(lineCurvePoints);
            //WriteToFile("centerline_points_raw.txt", beforesmooth);
        }
        else
        {
            beforesmooth = thepoints;

            //thepoints.Reverse();
            Vector3[] lineCurvePoints = beforesmooth.ToArray();
            lineRenderer.positionCount = lineCurvePoints.Length;
            lineRenderer.SetPositions(lineCurvePoints);

            aftersmooth = thepoints;
        }


    }


    List<Vector3> BezierPointsCreator(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        int pointCount = 100;

        List<Vector3> pointList = new List<Vector3>();

        for (int i = 1; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;

            float c0 = (1 - t) * (1 - t);
            float c1 = 2 * (1 - t) * t;
            float c2 = t * t;

            Vector3 point = c0 * p0 + c1 * p1 + c2 * p2;

            pointList.Add(point);
        }

        return pointList;
    }

    List<Vector3> BezierPointsCreatorC2(Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        int pointCount = 20;

        List<Vector3> pointList = new List<Vector3>();

        for (int i = 1; i <= pointCount; i++)
        {
            float t = i / (float)pointCount;

            //float c0 = (1 - t) * (1 - t) * (1-t);
            //float c1 = 3 * (1 - t) * (1-t) * t;
            //float c2 = 3 * (1-t) * t * t;
            //float c3 = t * t * t;

            float c0 = -t * t * t + 3 * t * t - 3 * t + 1;
            float c1 = 3 * t * t * t - 6 * t * t + 3 * t;
            float c2 = -3 * t * t * t + 3 * t * t;
            float c3 = t * t * t;

            Vector3 point = c0 * p0 + c1 * p1 + c2 * p2 + c3 * p3;

            pointList.Add(point);
        }

        return pointList;
    }

    void RenderSmooth()
    {
        Vector3[] lineCurvePoints = aftersmooth.ToArray();
        lineRenderer.positionCount = lineCurvePoints.Length;
        lineRenderer.SetPositions(lineCurvePoints);
    }

    void RenderRaw()
    {
        Vector3[] lineCurvePoints = beforesmooth.ToArray();
        lineRenderer.positionCount = lineCurvePoints.Length;
        lineRenderer.SetPositions(lineCurvePoints);
    }

    IEnumerator moveCamera(float progress)
    {
        int start_index = 0;
        int max_index = aftersmooth.ToArray().Length - 1;
        //print(aftersmooth.ToArray().Length);
        start_index = (int) Mathf.Ceil(max_index * progress);
        //print(start_index);

        Camera.main.transform.position = aftersmooth[start_index];
        Camera.main.transform.forward = aftersmooth[start_index + 1] - aftersmooth[start_index];


        Vector3 curr_pos = aftersmooth[start_index];

        int last_index = start_index;
        int next_index = start_index + 1;

        while (next_index < max_index && next_index > 0)
        {
            float delta_distance = Time.deltaTime * Math.Abs(speed);
            float segment_length = Vector3.Distance(aftersmooth[last_index], aftersmooth[next_index]);
            float cur_to_next = Vector3.Distance(curr_pos, aftersmooth[next_index]);

            while (delta_distance > cur_to_next && next_index < max_index && next_index > 0)
            {
                delta_distance -= cur_to_next;
                last_index = next_index;

                if (speed > 0)
                    next_index++;
                else
                    next_index--;

                curr_pos = aftersmooth[last_index];
                cur_to_next = Vector3.Distance(curr_pos, aftersmooth[next_index]);
                segment_length = Vector3.Distance(aftersmooth[last_index], aftersmooth[next_index]);
            }

            curr_pos = curr_pos + (aftersmooth[next_index] - aftersmooth[last_index]) / segment_length * delta_distance;
            Camera.main.transform.position = curr_pos;
            Camera.main.transform.forward = aftersmooth[next_index] - aftersmooth[last_index];

            yield return null;
        }

        yield return null;
    }

    public void WriteToFile(string filename, List<Vector3> pos_array)
    {
        // Write to disk
        StreamWriter writer = new StreamWriter(filename, true);

        foreach(Vector3 pos in pos_array.ToArray())
        {
            string serializedData = JsonUtility.ToJson(pos);
            writer.Write(serializedData+"\n");
        }
    }

    public List<Vector3> ReducePoints(Vector3[] points_array, int multiplier = 5)
    {
        List<Vector3> new_array = new List<Vector3>();
        int index = 0;

        foreach (Vector3 p in points_array)
        {
            if(index % multiplier == 0)
            {
                new_array.Add(p);
            }

            index++;
        }

        return new_array;
    }
}


