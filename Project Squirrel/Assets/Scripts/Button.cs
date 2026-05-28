using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class Activatable : MonoBehaviour
{
    public virtual void SetActiveStatus(Button Button, bool IsActive) {
    }
}

public class Button : MonoBehaviour
{
    [SerializeField] private List<Activatable> connectedObjs = new List<Activatable>();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            ActivateObjects();
            Debug.Log("Button Pressed");
        }
    }

    public void ActivateObjects()
    {
        foreach (Activatable obj in connectedObjs)
        {
            obj.SetActiveStatus(this, true);
        }
    }

    public void DeactivateObjects()
    {
        foreach (Activatable obj in connectedObjs)
        {
            obj.SetActiveStatus(this, false);
        }
    }
}
