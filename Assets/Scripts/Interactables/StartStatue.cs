using UnityEngine;

public class StartStatue : Interactable
{
    [SerializeField] EnvironmentGenerator environmentGenerator;
    [SerializeField] GameObject gameplayUI;
    [SerializeField] GameObject twitchChat;

    public override void Interact()
    {
        base.Interact();
        gameplayUI.SetActive(true);
        environmentGenerator.OpenRoom(new Vector2Int(54, 0));
        twitchChat.SetActive(true);
        FindObjectOfType<AudioManager>().Play("Click");
    }
}
