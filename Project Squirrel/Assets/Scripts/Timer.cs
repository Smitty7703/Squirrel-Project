using System.Collections;
using System.Timers;
using UnityEngine;

public class Timer : Activatable
{
    [SerializeField] private float duration;
    [SerializeField] private Button motherButton;

    public override void SetActiveStatus(Button button, bool isActive)
    {
        motherButton = button;
        if (isActive)
        {
            StartCoroutine(Countdown());
            //logic for visual cue
        }
        else
        {
            //logic for disabling visual cue
        }
    }

    private IEnumerator Countdown()
    {
        yield return new WaitForSeconds(duration);
        motherButton.DeactivateObjects();
    }
}
