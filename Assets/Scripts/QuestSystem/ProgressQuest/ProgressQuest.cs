using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Enfabler.Quests;

public class ProgressQuest : MonoBehaviour
{
    public Quest[] quests;
    public bool progressAll = false;
    protected int stage = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        foreach(var item in quests)
        {
            //Debug.Log("Added to quest delegate " + item.name);
            item.updateQuestDelegate += CheckUpdate;
            if (item.state == E_QuestStates.InProgress)
                QuestInProgress(true);
        }
    }

    private void OnDestroy()
    {
        foreach (var item in quests)
        {
            //Debug.Log("Remove from quest delegate " + item.name);
            item.updateQuestDelegate -= CheckUpdate;
        }
    }

    #region Quest Markers

    public GameObject overrideParentQuestMarker;
    public bool attachAsParent = true;
    public Vector3 markerOffset;

    public void CheckUpdate(E_QuestStates questState, int progress)
    {
        //Debug.Log("Quest updated - Received message");

        bool showMarker = false;

        foreach (var item in quests)
        {
            if (item.state == E_QuestStates.InProgress)
                showMarker = true;
        }

        QuestInProgress(showMarker);
    }

    GameObject marker;

    public void QuestInProgress(bool inProgress)
    {
        if (inProgress)
        {
            if (marker == null)
                marker = QuestMarkerManager.instance.AddQuestMarker(overrideParentQuestMarker == null ? this.gameObject : overrideParentQuestMarker, markerOffset, attachAsParent);
        }
        else
        {
            QuestMarkerManager.instance.RemoveQuestMarker(marker);
            marker = null;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(overrideParentQuestMarker == null ? transform.position + markerOffset : overrideParentQuestMarker.transform.position + markerOffset, 0.5f);
    }

    #endregion

    protected virtual void ProgressQuestStage()
    {
        if (progressAll)
        {
            foreach (var item in quests)
            {
                item.QuestProgress(true);
            }

            return;
        }

        if (stage >= quests.Length)
            return;

        if (quests[stage].QuestProgress(true))
            stage++;
    }
}

[System.Serializable]
public enum E_TutorialSteps
{
    OnTriggerEnter, OnTriggerExit, OnHit,
}