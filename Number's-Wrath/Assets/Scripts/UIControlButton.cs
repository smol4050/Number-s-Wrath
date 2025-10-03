using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class UIControlButton : MonoBehaviour
{
    public string operation = "plus";
    public AudioClip overrideClickClip;

    Button btn;
    Image img;
    Coroutine blinkRoutine;
    bool isBlinking = false;

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
        var player = PlayerController.Instance;
        if (player == null)
        {
            Debug.LogWarning("[UIControlButton] No player instance found.");
            return;
        }

        if (!player.HasPendingReward())
        {
            Debug.Log($"UIControlButton: No pending reward — '{operation}' ignored.");
            return;
        }

        if (SoundManager.InstanceExists)
        {
            if (overrideClickClip != null)
                SoundManager.Instance.PlaySFX(overrideClickClip);
            else
                SoundManager.Instance.PlayClick();
        }

        if (operation == "plus")
        {
            player.ApplyPendingAsSum();
        }
        else if (operation == "mult" || operation == "multiply")
        {
            player.ApplyPendingAsMultiply();
        }
        else
        {
            Debug.LogWarning($"UIControlButton: operación desconocida '{operation}'");
        }

        GameUIManager.Instance?.UpdatePendingUI(false);

        GameUIManager.Instance?.HighlightOperation(operation);
    }

    
    public void DoActionWithoutSound()
    {
        var player = PlayerController.Instance;
        if (player == null) return;
        if (!player.HasPendingReward())
        {
            Debug.Log($"UIControlButton: No pending reward — '{operation}' ignored (without sound).");
            return;
        }

        if (operation == "plus") player.ApplyPendingAsSum();
        else if (operation == "mult" || operation == "multiply") player.ApplyPendingAsMultiply();

        GameUIManager.Instance?.UpdatePendingUI(false);
        GameUIManager.Instance?.HighlightOperation(operation);
    }

    public void StartBlinking()
    {
        if (isBlinking) return;
        isBlinking = true;
        
        if (img != null) img.enabled = true;
        blinkRoutine = StartCoroutine(Blink());
    }

    public void StopBlinking()
    {
        if (!isBlinking) return;
        isBlinking = false;
        if (blinkRoutine != null) StopCoroutine(blinkRoutine);
        blinkRoutine = null;
        if (img != null) img.enabled = true;
    }

    IEnumerator Blink()
    {
        float interval = 0.32f;
        while (isBlinking)
        {
            if (img != null)
                img.enabled = !img.enabled;
            yield return new WaitForSeconds(interval);
        }
        if (img != null) img.enabled = true;
    }
}
