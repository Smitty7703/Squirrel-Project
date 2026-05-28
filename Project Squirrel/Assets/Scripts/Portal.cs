using UnityEngine;

public class Portal : Activatable
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
            TeleportPlayer(other.gameObject.GetComponent<PlayerController>()); //Teleport player position to other portal
        }
    }

    private void TeleportPlayer(PlayerController player)
    {
        Rigidbody rb = player.GetComponent<Rigidbody>();
        Vector3 localOffset = transform.InverseTransformPoint(player.transform.position);
        player.transform.position = connectedPort.transform.TransformPoint(localOffset);

        Quaternion portalRotationDelta = connectedPort.transform.rotation * Quaternion.Inverse(transform.rotation);

        rb.linearVelocity = portalRotationDelta * rb.linearVelocity;
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

    #region Activation Logic

    public override void SetActiveStatus(Button button, bool isActive)
    {
        this.isActive = isActive;
        Debug.Log(name + " isActive = " + isActive);
    }

    //Some cosmetic logic will be needed here to switch between active states visually

    #endregion
}
