using UnityEngine;

public class Portal : MonoBehaviour
{
    [SerializeField] private bool isActive;
    [SerializeField] private Portal connectedPort;
    private bool isPortaling;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && isActive && !connectedPort.IsPortaling())
        {
            if (connectedPort == null) { Debug.Log("No Connected Portal"); return; }
            isPortaling = true;
            other.gameObject.transform.position = connectedPort.gameObject.transform.position; //Teleport player position to other portal
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player") && connectedPort.IsPortaling())
        {
            connectedPort.SetPortaling(false);
        }
    }

    public bool IsPortaling()
    {
        return isPortaling;
    }

    public void SetPortaling(bool isPort)
    {
        isPortaling = isPort;
    }
}
