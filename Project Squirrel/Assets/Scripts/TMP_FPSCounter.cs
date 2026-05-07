using UnityEngine;

public class TMP_FPSCounter : MonoBehaviour
{
    [SerializeField] private bool fpsCounterEnabled = false;
    float timer;
    int frames;

    void Update()
    {
        if (!fpsCounterEnabled) return;
        timer += Time.unscaledDeltaTime;
        frames++;

        if (timer >= 1f)
        {
            Debug.Log("FPS: " + frames);
            frames = 0;
            timer = 0f;
        }
    }
}
