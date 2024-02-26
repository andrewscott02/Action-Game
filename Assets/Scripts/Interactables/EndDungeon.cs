using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndDungeon : Interactable, IInteractable
{
    public E_Scenes sceneToLoad;
    public GrammarsDungeonData dungeonData;

    public string message = "You are victorious!";

    public override void Interacted(BaseCharacterController interactCharacter)
    {
        base.Interacted(interactCharacter);

        TextPopupManager.instance.ShowMessageText(message);
        StartCoroutine(ILoadScene(2f));
    }

    IEnumerator ILoadScene(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (dungeonData != null)
        {
            DungeonManager.grammarsDungeonData = dungeonData;
        }

        LoadingScreen.instance.StartLoadingScreen();

        SceneManager.LoadScene(sceneToLoad.ToString());
    }
}