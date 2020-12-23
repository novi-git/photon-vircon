using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CustomLatency : MonoBehaviour {
    public struct Latency {
        public float HighLatency;
        public float LowLatency;
    }

    public float HighLatency = 0.5f;
    public float LowLatency = 1f;
    private string sceneId;

    public static Dictionary<string, Latency> LatencyPerScene = new Dictionary<string, Latency>();

    private void OnEnable() {
        sceneId = SceneManager.GetActiveScene().ToString();
        LatencyPerScene.Add(SceneManager.GetActiveScene().ToString(),
            new Latency { HighLatency = this.HighLatency, LowLatency = this.LowLatency });
    }

    private void OnDisable() {
        if (LatencyPerScene.ContainsKey(sceneId)) {
            LatencyPerScene.Remove(sceneId);

        }
    }

    public static Latency GetLatency() { 

        if (LatencyPerScene.ContainsKey(SceneManager.GetActiveScene().ToString())) {
            return LatencyPerScene[SceneManager.GetActiveScene().ToString()];
        }

        return new Latency { HighLatency = 0.5f, LowLatency = 1f };

    }
}
