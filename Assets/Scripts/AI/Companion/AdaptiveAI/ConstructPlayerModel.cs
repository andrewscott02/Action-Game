using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConstructPlayerModel : MonoBehaviour
{
    #region Setup

    [Header("Agent Model Info")]
    public GameObject modelCharacter;
    public TextMeshProUGUI stateText;

    Descriptor trueState;
    public Descriptor playerState;

    public Dictionary<Descriptor, float> descriptorValues = new Dictionary<Descriptor, float>();
    public List<DescriptorValue> showValues = new List<DescriptorValue>();

    public bool test = false;

    private void Start()
    {
        descriptorValues.Add(Descriptor.Aggressive, 0);
        descriptorValues.Add(Descriptor.Counter, 0);
        descriptorValues.Add(Descriptor.Defensive, 0);
        descriptorValues.Add(Descriptor.Cautious, 0);
        descriptorValues.Add(Descriptor.Panic, 0);

        if (test) { return; }

        if (modelCharacter != null)
        {
            modelCharacter.GetComponent<Health>().modelConstructor = this;
            modelCharacter.GetComponent<CharacterCombat>().modelConstructor = this;
        }

        InvokeRepeating("CurrentTarget", 0, currentTargetCastInterval);
    }

    private void Update()
    {
        DecayModels();
        AdjustDisplay();
        explore += Time.deltaTime;
        currentSwitchCooldown += Time.deltaTime;
        
    }

    private void AdjustDisplay()
    {
        showValues.Clear();

        Descriptor highestState = Descriptor.Null;
        float highestValue = 0;

        DescriptorValue baseValue = new DescriptorValue();
        baseValue.descriptor = Descriptor.Null;
        baseValue.value = 0;

        showValues.Add(baseValue);

        foreach (var item in descriptorValues)
        {
            DescriptorValue newValue = new DescriptorValue();
            newValue.descriptor = item.Key;
            newValue.value = item.Value;

            showValues.Add(newValue);

            if (newValue.value > highestValue)
            {
                highestValue = newValue.value;
                highestState = newValue.descriptor;
            }
        }

        if (highestState != Descriptor.Null)
        {
            playerState = GetExploreState(highestState);
            trueState = highestState;
        }

        if (test) { return; }

        if (stateText != null)
        {
            stateText.text = "PS: " + playerState.ToString() + "TS: " + trueState.ToString();
        }
    }

    float explore = 0;
    public float switchCooldown = 5;
    float currentSwitchCooldown = 0;

    private Descriptor GetExploreState(Descriptor newState)
    {
        if (trueState != newState)
        {
            explore = 0;
            return newState;
        }

        if (currentSwitchCooldown < switchCooldown)
        {
            return playerState;
        }

        List<Descriptor> possibleStates = new List<Descriptor>();

        float highestValue = descriptorValues[newState];

        foreach (var item in descriptorValues)
        {
            if (item.Key != newState)
            {
                float difference = highestValue - item.Value;

                if (difference < explore)
                {
                    possibleStates.Add(item.Key);
                }
            }
        }

        if (possibleStates.Count > 0)
        {
            Descriptor exploreState = possibleStates[Random.Range(0, possibleStates.Count)];
            Debug.Log("Switched from " + trueState.ToString() + " to " + exploreState.ToString());
            currentSwitchCooldown = 0;

            return exploreState;
        }

        return newState;
    }

    #endregion

    #region Decay Models

    [Header("Decay Values")]
    public float aggressiveDecay = 0.3f;
    public float counterDecay = 0.5f;
    public float defensiveDecay = 0.5f;
    public float cautiousDecay = 0.5f;
    public float panicDecay = 0.5f;

    private void DecayModels()
    {
        descriptorValues[Descriptor.Aggressive] = Mathf.Clamp(descriptorValues[Descriptor.Aggressive] - (aggressiveDecay * Time.deltaTime), 0, Mathf.Infinity);
        descriptorValues[Descriptor.Counter] = Mathf.Clamp(descriptorValues[Descriptor.Counter] - (counterDecay * Time.deltaTime), 0, Mathf.Infinity);
        descriptorValues[Descriptor.Defensive] = Mathf.Clamp(descriptorValues[Descriptor.Defensive] - (defensiveDecay * Time.deltaTime), 0, Mathf.Infinity);
        descriptorValues[Descriptor.Cautious] = Mathf.Clamp(descriptorValues[Descriptor.Cautious] - (cautiousDecay * Time.deltaTime), 0, Mathf.Infinity);
        descriptorValues[Descriptor.Panic] = Mathf.Clamp(descriptorValues[Descriptor.Panic] - (panicDecay * Time.deltaTime), 0, Mathf.Infinity);
    }

    #endregion

    #region Player Actions

    [Header("Player Targeting")]
    public List<CharacterController> currentTargets;
    public float currentTargetCastInterval = 0.6f;

    void CurrentTarget()
    {
        if (modelCharacter != null)
            currentTargets = modelCharacter.GetComponent<CharacterCombat>().currentTargets;
        else
            currentTargets.Clear();
    }

    public void PlayerAttack(bool hit)
    {
        descriptorValues[Descriptor.Aggressive] += 1.5f;

        if (CheckCounter()) { descriptorValues[Descriptor.Counter] += 7.5f; }

        if (!hit) { descriptorValues[Descriptor.Panic] += 3f; }

        AdjustDisplay();
    }

    public void PlayerParry(bool beingAttacked)
    {
        descriptorValues[Descriptor.Defensive] += 5f;

        if (beingAttacked) { SetupCounter(counterWindowParry); }
        else { descriptorValues[Descriptor.Panic] += 7f; }

        AdjustDisplay();
    }

    public void PlayerDodge(bool beingAttacked)
    {

        if (beingAttacked)
        {
            descriptorValues[Descriptor.Cautious] += 3.5f;
            descriptorValues[Descriptor.Defensive] += 3f;
            SetupCounter(counterWindowDodge);
        }
        else 
        {
            descriptorValues[Descriptor.Cautious] += 4f;
            descriptorValues[Descriptor.Panic] += 5f;
        }

        AdjustDisplay();
    }

    public void PlayerHit()
    {
        descriptorValues[Descriptor.Panic] += 3f;
        AdjustDisplay();
    }

    #endregion

    #region Counter Attack Considerations

    [Header("Counter Info")]
    public bool counterAvailable;
    public float counterWindowParry = 1.5f;
    public float counterWindowDodge = 3f;

    bool CheckCounter()
    {
        return counterAvailable;
    }

    void SetupCounter(float counterWindow)
    {
        counterAvailable = true;
        CancelInvoke();
        Invoke("EndCounter", counterWindow);
    }

    void EndCounter()
    {
        counterAvailable = false;
    }

    #endregion
}

[System.Serializable]
public struct DescriptorValue
{
    public Descriptor descriptor;
    public float value;
}

public enum Descriptor
{
    Null, Aggressive, Counter, Defensive, Cautious, Panic
}