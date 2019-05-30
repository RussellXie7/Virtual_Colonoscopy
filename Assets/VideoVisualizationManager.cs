using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VideoType
{
    Raw,
    Masked,
    Wall
}

public class VideoVisualizationManager : MonoBehaviour {

    public GameObject rawVideo;
    public GameObject wallVideo;
    public GameObject maskedVideo;
    public GameObject rawVideo_Interporlated;
    public GameObject wallVideo_Interporlated;
    public GameObject maskedVideo_Interporlated;

    public GameObject VisulizeHelper;
    public GameObject canvas;

    private bool isInterporlated = false;

    private GameObject currentVideo;
    private GameObject previousVideo;

    private Vector3 presentation_pos;
    private Vector3 bystander_pos;

    private VideoType curr_videoType;

    private List<VideoPlayerManager> videoPlayers;

    // Use this for initialization
    void Start() {

        presentation_pos = rawVideo.transform.position;
        bystander_pos = maskedVideo.transform.position;

        currentVideo = rawVideo;
        previousVideo = null;
        curr_videoType = VideoType.Raw;
        canvas.SetActive(false);

        videoPlayers = new List<VideoPlayerManager>();
        videoPlayers.Add(rawVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayers.Add(wallVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayers.Add(maskedVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayers.Add(rawVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());
        videoPlayers.Add(wallVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());
        videoPlayers.Add(maskedVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());
    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchVideo(VideoType.Raw);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchVideo(VideoType.Wall);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchVideo(VideoType.Masked);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            VisulizeHelper.SetActive(!VisulizeHelper.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            isInterporlated = !isInterporlated;
            canvas.SetActive(isInterporlated);
            SwitchVideo(curr_videoType);
        }
    }

    private void SwitchVideo(VideoType type)
    {
        previousVideo = currentVideo;

        switch (type)
        {
            case VideoType.Raw:
                currentVideo = isInterporlated ? rawVideo_Interporlated : rawVideo;
                break;
            case VideoType.Masked:
                currentVideo = isInterporlated ? maskedVideo_Interporlated : maskedVideo;
                break;
            case VideoType.Wall:
                currentVideo = isInterporlated ? wallVideo_Interporlated : wallVideo;
                break;
            default:
                Debug.LogError("The Video Type doesn not make sense: " + type.ToString());
                break;
        }

        // The sequence matters so that it handles the case when two videos are the same.
        previousVideo.transform.position = bystander_pos;
        currentVideo.transform.position = presentation_pos;

        curr_videoType = type;
    }

    public void SetVideosFrame(long current_frame)
    {
        foreach (VideoPlayerManager v in videoPlayers)
        {
            v.SetVideoFrame(current_frame);
        }
    }
}


/*
 * using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum VideoType
{
    Raw,
    Masked,
    Wall
}

public class VideoVisualizationManager : MonoBehaviour {

    public GameObject rawVideo;
    public GameObject wallVideo;
    public GameObject maskedVideo;
    public GameObject rawVideo_Interporlated;
    public GameObject wallVideo_Interporlated;
    public GameObject maskedVideo_Interporlated;

    public GameObject VisulizeHelper;
    public GameObject canvas;

    private bool isInterporlated = false;

    private GameObject currentVideo;
    private GameObject previousVideo;

    private Vector3 presentation_pos;
    private Vector3 bystander_pos;

    private VideoType curr_videoType;
    private long m_current_frame;

    Dictionary<GameObject,VideoPlayerManager> videoPlayer_dict;

    // Use this for initialization
    void Start() {
        presentation_pos = rawVideo.transform.position;
        bystander_pos = maskedVideo.transform.position;

        currentVideo = rawVideo;
        previousVideo = null;
        curr_videoType = VideoType.Raw;
        canvas.SetActive(false);

        videoPlayer_dict = new Dictionary<GameObject, VideoPlayerManager>();
        videoPlayer_dict.Add(rawVideo,rawVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayer_dict.Add(wallVideo, wallVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayer_dict.Add(maskedVideo, maskedVideo.GetComponentInChildren<VideoPlayerManager>());
        videoPlayer_dict.Add(rawVideo_Interporlated, rawVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());
        videoPlayer_dict.Add(wallVideo_Interporlated, wallVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());
        videoPlayer_dict.Add(maskedVideo_Interporlated, maskedVideo_Interporlated.GetComponentInChildren<VideoPlayerManager>());

    }

    // Update is called once per frame
    void Update() {

        if (Input.GetKeyDown(KeyCode.Z))
        {
            SwitchVideo(VideoType.Raw);
        }

        if (Input.GetKeyDown(KeyCode.X))
        {
            SwitchVideo(VideoType.Wall);
        }

        if (Input.GetKeyDown(KeyCode.C))
        {
            SwitchVideo(VideoType.Masked);
        }

        if (Input.GetKeyDown(KeyCode.V))
        {
            VisulizeHelper.SetActive(!VisulizeHelper.activeSelf);
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            isInterporlated = !isInterporlated;
            canvas.SetActive(isInterporlated);
            SwitchVideo(curr_videoType);
        }
    }

    private void SwitchVideo(VideoType type)
    {
        previousVideo = currentVideo;

        switch (type)
        {
            case VideoType.Raw:
                currentVideo = isInterporlated ? rawVideo_Interporlated : rawVideo;
                break;
            case VideoType.Masked:
                currentVideo = isInterporlated ? maskedVideo_Interporlated : maskedVideo;
                break;
            case VideoType.Wall:
                currentVideo = isInterporlated ? wallVideo_Interporlated : wallVideo;
                break;
            default:
                Debug.LogError("The Video Type doesn not make sense: " + type.ToString());
                break;
        }

        SetVideosFrame(m_current_frame);

        // The sequence matters so that it handles the case when two videos are the same.
        previousVideo.transform.position = bystander_pos;
        currentVideo.transform.position = presentation_pos;

        curr_videoType = type;
    }

    public void SetVideosFrame(long current_frame)
    {
        m_current_frame = current_frame;
        videoPlayer_dict[currentVideo].SetVideoFrame(current_frame);
    }
}

 */
