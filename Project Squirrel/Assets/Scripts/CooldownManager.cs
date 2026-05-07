using NUnit.Framework;
using System.Collections.Generic;
using UnityEngine;

public class Cooldown
{
    public float duration;
    public float readyTime;
    public bool isOnCooldown;
}

public class CooldownManager : MonoBehaviour
{
    public static CooldownManager Instance { get; private set; }
    private List<Cooldown> cooldowns = new List<Cooldown>();

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    public int AddCooldown(float cooldown)
    {
        Cooldown cd = new Cooldown{
            duration = cooldown,
            readyTime = 0,
            isOnCooldown = false};
        cooldowns.Add(cd);
        Debug.Log("Cooldown Added, Info: " + cooldowns[cooldowns.Count - 1]);
        return cooldowns.Count - 1;
    }

    public void RemoveCooldown(int index)
    {
        Debug.Log("Cooldown Removed, Info: " + cooldowns[index]);
        cooldowns.RemoveAt(index);
    }

    public bool InquireCooldownStatus(int index)
    {
        if (cooldowns[index] == null)
        {
            Debug.LogError("Cooldown Index Does Not Exist");
            return false;
        }
        bool cd = cooldowns[index].isOnCooldown;
        Debug.Log("Cooldown Inquiry, Results: " + cd);
        return cd;
    }

    public void StartCooldown(int index)
    {
        float time = Time.time + cooldowns[index].duration;
        cooldowns[index].readyTime = time;
        cooldowns[index].isOnCooldown = true;
        Debug.Log("Cooldown Starting");
    }

    private void Update()
    {
        foreach (Cooldown cooldown in cooldowns)
        {
            if (cooldown.isOnCooldown && Time.time >= cooldown.readyTime)
            {
                Debug.Log("Cooldown Ending, Info: " + cooldown);
                cooldown.isOnCooldown = false;
            }
        }
    }
}
