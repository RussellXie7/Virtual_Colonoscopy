using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using System;


public class PlaneMover : MonoBehaviour {

    public VideoVisualizationManager vvm;
    public GameObject outerMesh;

    public string thename;
    public bool mirror = false;
    public GameObject planeObject;
    public GameObject m_3DScene;
    public bool m_RecalculateBezier = true;
    public bool m_ReversePointsArray = true;

    public float rotate_speed = 30f;
    public bool delay_enabled;
    public float move_fps = 10f;

    private Vector3[] curve_points;
    private Vector3[] points_forward;
    private int current_frame;
    private float delay_time;
    private float delay_target;
    // Use this for initialization
    void Start () {
        current_frame = 0;
        thename = "Assets/Centerline_Vis/TextData/" + thename;

        ParseFile (thename);
        CreateSmoothForwards(curve_points);
        VisualizePlane();

        delay_time = 0;
        delay_target = 1 / move_fps;

        print("Centerline Points Count: " + curve_points.Length.ToString());

        outerMesh.SetActive(false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if (Input.GetKey(KeyCode.RightArrow))
        {
            MovePlaneForward();
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            MovePlaneBackward();
        }

        if (Input.GetKey(KeyCode.A))
        {
            m_3DScene.transform.RotateAround(new Vector3(256,256,144), Vector3.up, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.D))
        {
            m_3DScene.transform.RotateAround(new Vector3(256, 256, 144), Vector3.down, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.W))
        {
            m_3DScene.transform.RotateAround(new Vector3(256, 256, 144), Vector3.right, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.S))
        {
            m_3DScene.transform.RotateAround(new Vector3(256, 256, 144), Vector3.left, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Q))
        {
            m_3DScene.transform.RotateAround(new Vector3(256, 256, 144), Vector3.forward, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.E))
        {
            m_3DScene.transform.RotateAround(new Vector3(256, 256, 144), Vector3.back, rotate_speed * Time.deltaTime);
        }

        if (Input.GetKeyDown(KeyCode.O))
        {
            outerMesh.SetActive(!outerMesh.activeSelf);
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

        if (m_ReversePointsArray)
        {
            thepoints.Reverse();
        }

        if (m_RecalculateBezier)
        {
            List<Vector3> points = MakeBezier(thepoints);
            curve_points = points.ToArray();
        }
        else
        {
            curve_points = thepoints.ToArray();
        }
    }

    void MovePlaneForward()
    {
        if (delay_enabled && delay_time < delay_target)
        {
            delay_time += Time.deltaTime;
        }
        else
        {
            delay_time = 0f;
            current_frame = current_frame + 1 >= curve_points.Length ? curve_points.Length - 1 : current_frame + 1;
            VisualizePlane();
            vvm.SetVideosFrame(current_frame);
        }
    }

    void MovePlaneBackward()
    {
        if (delay_enabled && delay_time < delay_target)
        {
            delay_time += Time.deltaTime;
        }
        else
        {
            delay_time = 0f;
            current_frame = current_frame - 1 < 0 ? 0 : current_frame - 1;
            VisualizePlane();
            vvm.SetVideosFrame(current_frame);
        }
    }

    void VisualizePlane()
    {
        planeObject.transform.localPosition = curve_points[current_frame];
        planeObject.transform.forward = m_3DScene.transform.rotation * points_forward[current_frame];
    }

    void CreateForwards(Vector3[] points)
    {
        List<Vector3> forwards = new List<Vector3>();
        int index = 0;

        foreach (Vector3 p in points)
        {

            Vector3 tangent = new Vector3(0, 0, 0);

            if (index - 1 >= 0 && index + 1 < points.Length)
            {
                tangent = points[index + 1] - points[index - 1];
            }
            else if (index -1 >= 0)
            {
                tangent = points[index] - points[index - 1];
            }
            else if (index + 1 < points.Length)
            {
                tangent = points[index + 1] - points[index];
            }
            else
            {
                print("seems that the list has only one or no element.");
                break;
            }

            tangent = tangent.normalized;
            forwards.Add(tangent);

            index++;
        }

        points_forward = forwards.ToArray();
    }

    void CreateSmoothForwards(Vector3[] points)
    {
        List<Vector3> forwards = new List<Vector3>();
        int index = 0;

        foreach (Vector3 p in points)
        {

            Vector3 agg_tangent = new Vector3();

            int ending = 200;

            if (index + ending >= points.Length)
            {
                ending = points.Length - index - 1;
            }

            for (int j = 0; j < ending; j++)
            {
                agg_tangent += points[index + j] - points[index];
            }

            forwards.Add(agg_tangent.normalized);

            index++;
        }

        points_forward = forwards.ToArray();
    }

    #region BezierFunctions
    List<Vector3> MakeBezier(List<Vector3> thepoints)
    {

        //beforesmooth = thepoints;
        List<Vector3> beforesmooth = ReducePoints(thepoints.ToArray());


        //thepoints.Reverse();
        Vector3[] lineCurvePoints = beforesmooth.ToArray();


        List<Vector3> smoothline = new List<Vector3>();

        float total_length = (lineCurvePoints.Length - lineCurvePoints.Length % 2) / 2;

        for (int i = 0; i < total_length && (2 * i + 2) < lineCurvePoints.Length; i++)
        {
            Vector3 p0 = lineCurvePoints[2 * i];
            Vector3 p1 = lineCurvePoints[2 * i + 1];
            Vector3 p2 = lineCurvePoints[2 * i + 2];
            //Vector3 p3 = lineCurvePoints[3 * i + 3];

            if (2 * i - 1 >= 0)
            {
                p0 = (lineCurvePoints[2 * i - 1] + p1) / 2;
            }

            if (2 * i + 3 < lineCurvePoints.Length)
            {
                p2 = (p1 + lineCurvePoints[2 * i + 3]) / 2;
            }

            smoothline.AddRange(BezierPointsCreator(p0, p1, p2));
        }

        List<Vector3> aftersmooth = smoothline;
        Debug.Log(smoothline.ToArray().Length);

        return aftersmooth;
    }


    public List<Vector3> ReducePoints(Vector3[] points_array, int multiplier = 5)
    {
        List<Vector3> new_array = new List<Vector3>();
        int index = 0;

        foreach (Vector3 p in points_array)
        {
            if (index % multiplier == 0)
            {
                new_array.Add(p);
            }

            index++;
        }

        return new_array;
    }

    List<Vector3> BezierPointsCreator(Vector3 p0, Vector3 p1, Vector3 p2)
    {
        int pointCount = 60;

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
    #endregion
}
