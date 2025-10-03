using UnityEngine;
using UnityEngine.UI;

public class UIControlButton : MonoBehaviour
{
    public string operation = "plus";
    public AudioClip overrideClickClip;
    Button btn;
    Image img;

    void Awake()
    {
        btn = GetComponent<Button>();
        img = GetComponent<Image>();
        if (btn != null)
            btn.onClick.AddListener(OnClicked);
    }

    void OnDestroy()
    {
        if (btn != null)
            btn.onClick.RemoveListener(OnClicked);
    }

    void OnClicked()
    {
        DoAction();
    }

    public void DoAction()
    {
        GameUIManager.Instance?.HighlightOperation(operation);

        if (SoundManager.InstanceExists)
        {
            if (overrideClickClip != null)
                SoundManager.Instance.PlaySFX(overrideClickClip);
            else
                SoundManager.Instance.PlayClick();
        }

        var player = PlayerController.Instance;
        if (player != null)
        {
            if (operation == "plus") player.OnOperationPlusPressed();
            else if (operation == "mult") player.OnOperationMultPressed();
        }
    }

    public void DoActionWithoutSound()
    {
        GameUIManager.Instance?.HighlightOperation(operation);
        var player = PlayerController.Instance;
        if (player != null)
        {
            if (operation == "plus") player.OnOperationPlusPressed();
            else if (operation == "mult") player.OnOperationMultPressed();
        }
    }
}
