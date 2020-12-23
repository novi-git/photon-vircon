using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour
{
    [SerializeField]
    private VideoPlayer _player;
    [SerializeField]
    private List<string> _queue = new List<string>();
    [SerializeField]
    private bool _autoPlay = false;    
    private int _queueID;

    // Start is called before the first frame update
    void Start()
    {
        _queue.Clear();        
        _queue.Add("http://vircondemo.taktylstudios.com/dev/Videos/Gamification1.mp4");
         _queue.Add("http://vircondemo.taktylstudios.com/dev/Videos/Gamification2.mp4");
         _queue.Add("http://vircondemo.taktylstudios.com/dev/Videos/Gamification3.mp4");
        _queueID = 0;
        //_player = GetComponentInChildren<VideoPlayer>();
        _player.url = _queue[_queueID];
        _player.loopPointReached += Next;        
        
        if (_autoPlay) _player.Play();
    }

    public void Next(VideoPlayer player) {
        _player.Stop();
        _queueID = _queue.IndexOf(_player.url);
        if (_queue[_queueID] == _queue.Last()) {
            _queueID = 0;
        } else {
            _queueID = _queueID + 1;
        }
        _player.url = _queue[_queueID];
        _player.Play();
    }

    public void Next() {
        _player.Stop();
        _queueID = _queue.IndexOf(_player.url);
        if (_queue[_queueID] == _queue.Last()) {
            _queueID = 0;
        } else {
            _queueID = _queueID + 1;
        }
        _player.url = _queue[_queueID];
        _player.Play();
    }

}
