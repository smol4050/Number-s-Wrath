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
        if (btn != null) btn.onClick.AddListener(OnClicked);

        if (img != null)
            img.color = Color.black;
    }

    void OnDestroy()
    {
        if (btn != null) btn.onClick.RemoveListener(OnClicked);
    }

    void OnClicked()
    {
        DoAction();
    }

    public void DoAction()
    {
        var player = PlayerController.Instance;
        if (player == null) return;
        if (!player.HasPendingReward()) return;

        if (SoundManager.InstanceExists)
        {
            if (overrideClickClip != null) SoundManager.Instance.PlaySFX(overrideClickClip);
            else SoundManager.Instance.PlayClick();
        }

        if (operation == "plus")
            player.ApplyPendingAsSum();
        else if (operation == "mult" || operation == "multiply")
            player.ApplyPendingAsMultiply();

        GameUIManager.Instance?.UpdatePendingUI(false);
        GameUIManager.Instance?.HighlightOperation(operation);
    }

    public void DoActionWithoutSound()
    {
        var player = PlayerController.Instance;
        if (player == null) return;
        if (!player.HasPendingReward()) return;

        if (operation == "plus")
            player.ApplyPendingAsSum();
        else if (operation == "mult" || operation == "multiply")
            player.ApplyPendingAsMultiply();

        GameUIManager.Instance?.UpdatePendingUI(false);
        GameUIManager.Instance?.HighlightOperation(operation);
    }

    public void StartBlinking()
    {
        if (isBlinking) return;
        if (operation != "plus" && operation != "mult" && operation != "multiply") return;

        isBlinking = true;
        blinkRoutine = StartCoroutine(Blink());
    }

    public void StopBlinking()
    {
        if (!isBlinking) return;
        isBlinking = false;

        if (blinkRoutine != null)
            StopCoroutine(blinkRoutine);

        blinkRoutine = null;

        if (img != null)
            img.color = Color.black;
    }

    IEnumerator Blink()
    {
        float duration = 1.2f;
        float t = 0f;
        Color colorA = Color.black;
        Color colorB = new Color(0f, 1f, 0.098f); 

        while (isBlinking)
        {
            if (img != null)
            {
                float lerp = Mathf.PingPong(t / duration, 1f);
                img.color = Color.Lerp(colorA, colorB, lerp);
            }

            t += Time.deltaTime;
            yield return null;
        }

        if (img != null)
            img.color = Color.black;
    }
}
