using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(PlayerHealth))]
public class PlayerCrateEffects : MonoBehaviour
{
    [SerializeField]
    private GameObject toggleableLights;
    public event Action<CrateType> CrateCollected;
    public float SpeedMultiplier => Time.time < speedUntil ? speedMultiplier : 1f;

    private PlayerHealth health;
    private float speedUntil;
    private float speedMultiplier = 1f;
    private float lightUntil;
    private bool lightBoostActive;
    private readonly Dictionary<Light, bool> lightStates = new Dictionary<Light, bool>();
    private readonly Dictionary<GameObject, bool> objectStates = new Dictionary<GameObject, bool>();

    private void Awake()
    {
        health = GetComponent<PlayerHealth>();
        FindToggleableLights();
    }

    private void OnEnable()
    {
        health.Died += ClearEffects;
    }

    private void OnDisable()
    {
        health.Died -= ClearEffects;
        ClearEffects();
    }

    private void Update()
    {
        if (lightBoostActive && Time.time >= lightUntil)
        {
            RestoreLights();
        }
    }

    public void Collect(CrateType type, float multiplier, float duration)
    {
        if (health.IsDead())
        {
            return;
        }

        switch (type)
        {
            case CrateType.HP:
                health.Heal(1);
                break;
            case CrateType.Speed:
                speedMultiplier = Mathf.Max(1f, multiplier);
                speedUntil = Time.time + Mathf.Max(0f, duration);
                break;
            case CrateType.Light:
                if (duration <= 0f || !EnableLights())
                {
                    return;
                }

                lightUntil = Time.time + duration;
                lightBoostActive = true;
                break;
        }

        CrateCollected?.Invoke(type);
    }

    private void FindToggleableLights()
    {
        if (toggleableLights != null)
        {
            return;
        }

        foreach (Transform target in FindObjectsByType<Transform>(FindObjectsInactive.Include))
        {
            string name = target.name.Replace(" ", "").Replace("_", "").Replace("-", "");
            if (string.Equals(name, "ToggleableLights", StringComparison.OrdinalIgnoreCase))
            {
                toggleableLights = target.gameObject;
                return;
            }
        }
    }

    private bool EnableLights()
    {
        if (lightBoostActive)
        {
            return true;
        }

        FindToggleableLights();
        if (toggleableLights == null)
        {
            Debug.LogWarning("Assign the Toggleable Lights group on Player Crate Effects.", this);
            return false;
        }

        Light[] lights = toggleableLights.GetComponentsInChildren<Light>(true);
        if (lights.Length == 0)
        {
            return false;
        }

        foreach (Light light in lights)
        {
            lightStates.Add(light, light.enabled);
            Transform target = light.transform;
            while (target != null)
            {
                if (!objectStates.ContainsKey(target.gameObject))
                {
                    objectStates.Add(target.gameObject, target.gameObject.activeSelf);
                }

                if (target == toggleableLights.transform)
                {
                    break;
                }

                target = target.parent;
            }
        }

        foreach (GameObject target in objectStates.Keys)
        {
            target.SetActive(true);
        }

        foreach (Light light in lightStates.Keys)
        {
            light.enabled = true;
        }

        return true;
    }

    private void RestoreLights()
    {
        foreach (KeyValuePair<Light, bool> state in lightStates)
        {
            if (state.Key != null)
            {
                state.Key.enabled = state.Value;
            }
        }

        foreach (KeyValuePair<GameObject, bool> state in objectStates)
        {
            if (state.Key != null)
            {
                state.Key.SetActive(state.Value);
            }
        }

        lightStates.Clear();
        objectStates.Clear();
        lightBoostActive = false;
    }

    private void ClearEffects()
    {
        speedUntil = 0f;
        RestoreLights();
    }
}
