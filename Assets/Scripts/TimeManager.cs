using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeManager : MonoBehaviour
{
    public static TimeManager instance;

    float defaultTimeScale = 1;
    float currentTime = 1;

    // Start is called before the first frame update
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    public void SetTimeScale(float timeScale, float durationRealTime)
    {
        if (timeScale < Time.timeScale)
        {
            StopAllCoroutines();

            StartCoroutine(IResetTimeScale(timeScale, durationRealTime));
        }
    }

    IEnumerator IResetTimeScale(float timeScale, float delay)
    {
        Time.timeScale = timeScale;
        currentTime = Time.timeScale;
        yield return new WaitForSecondsRealtime(delay);

        if (!PauseMenu.instance.paused)
        {
            Time.timeScale = defaultTimeScale;

            currentTime = Time.timeScale;
        }
    }
}
