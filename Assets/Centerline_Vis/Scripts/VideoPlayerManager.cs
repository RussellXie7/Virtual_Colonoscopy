using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Video;

public class VideoPlayerManager : MonoBehaviour {

    VideoPlayer videoPlayer;
    private long total_frames;

    private void Awake()
    {
        videoPlayer = GetComponent<VideoPlayer>();
    }

    // Use this for initialization
    void Start () {

        total_frames = (long)videoPlayer.clip.frameCount;
        print("Video Player Count " + total_frames);
        videoPlayer.Pause();
    }
	
	// Update is called once per frame
	void Update ()
    {
        //if (Input.GetKey(KeyCode.LeftArrow))
        //{
        //    current_frame = current_frame - 1 < 0 ? 0 : current_frame - 1;
        //    videoPlayer.frame = current_frame;
        //}

        //if (Input.GetKey(KeyCode.RightArrow))
        //{
        //    current_frame = current_frame + 1 >= total_frames ? total_frames - 1 : current_frame + 1;
        //    videoPlayer.frame = current_frame;
        //}
    }

    public void SetVideoFrame(long current_frame)
    {
        videoPlayer.frame = current_frame;
    }
}
