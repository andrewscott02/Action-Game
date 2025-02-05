using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

namespace Enfabler.Quests
{
    public class QuestInfo : MonoBehaviour
    {
        public static QuestInfo instance;
        public GameObject questInfoGO;

        // Start is called before the first frame update
        void Start()
        {
            /*
            if (instance != null)
            {
                Destroy(this.gameObject);
            }
            */
            instance = this;
            //DontDestroyOnLoad(this.gameObject);

            if (baseQuest != null)
            {
                //baseQuest.ForceRestartQuest();

                if (trackingQuest == null)
                    SetTrackingQuest(baseQuest);
            }

            UpdateQuestInfo();
        }

        public Quest baseQuest;
        Quest trackingQuest;

        public TextMeshProUGUI title, number, description, progressCount;

        public void SetTrackingQuest(Quest quest)
        {
            if (quest != null)
            {
                trackingQuest = quest.GetParent();
                UpdateQuestInfo();
                questInfoGO.SetActive(true);
            }
            else
            {
                questInfoGO.SetActive(false);
            }
        }

        public void UpdateQuestInfo()
        {
            if (trackingQuest != null)
            {
                Quest sub = trackingQuest.GetCurrentQuestProgress();
                if (sub != null)
                {
                    title.text = sub.questName;
                    number.text = sub.questNumber.ToString();
                    description.text = TextReplace.instance.ReplaceText(sub.questDescription);
                    progressCount.text = sub.currentProgress.ToString() + "/" + sub.maxProgress.ToString();
                }
                else
                {
                    title.text = "No Quest";
                    number.text = "0";
                    description.text = "You are not tracking a quest, open the inventory with i and then open the journal to see your quests";
                    questInfoGO.SetActive(false);
                }
            }
            else
                questInfoGO.SetActive(false);
        }

    }
}