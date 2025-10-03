using UnityEngine;
using System.Collections;

public class SwordSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    public float baseAngle = 6.071f;
    public float swingAngle = -28.021f;
    public float swingSpeed = 300f;
    public float returnDelay = 0.06f;

    bool swinging = false;
    Quaternion startRot;
    Quaternion targetRot;
    Vector3 initialLocalScale;

    void Awake()
    {
        startRot = Quaternion.Euler(0, 0, baseAngle);
        
        transform.localRotation = startRot;
        initialLocalScale = transform.localScale;
        gameObject.SetActive(false);
    }

    public void DoSwing(bool facingRight)
    {
        if (swinging) return;

        Vector3 s = initialLocalScale;
        s.x = Mathf.Abs(s.x);
        transform.localScale = s;

        float startAngle = facingRight ? baseAngle : -baseAngle;
        float endAngle = facingRight ? swingAngle : -swingAngle;

        startRot = Quaternion.Euler(0, 0, startAngle);
        targetRot = Quaternion.Euler(0, 0, endAngle);

        transform.localRotation = startRot;

        gameObject.SetActive(true);
        StartCoroutine(SwingRoutine());
    }

    IEnumerator SwingRoutine()
    {
        swinging = true;

        while (Quaternion.Angle(transform.localRotation, targetRot) > 0.6f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, targetRot, swingSpeed * Time.deltaTime);
            yield return null;
        }

        yield return new WaitForSeconds(returnDelay);

        while (Quaternion.Angle(transform.localRotation, startRot) > 0.6f)
        {
            transform.localRotation = Quaternion.RotateTowards(transform.localRotation, startRot, swingSpeed * Time.deltaTime);
            yield return null;
        }

        transform.localRotation = startRot;
        gameObject.SetActive(false);
        swinging = false;
    }
}