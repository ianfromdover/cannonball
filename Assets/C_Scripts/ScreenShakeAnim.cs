using UnityEngine;

public class ScreenShakeAnim : MonoBehaviour
{
    private static float shakeAmount = 3;
    private static float currShakeAmount = 0;
    Vector3 cameraStartPos;

    void Start()
    {
        cameraStartPos = transform.position;
    }

    void Update()
    {
        currShakeAmount = Mathf.Lerp(currShakeAmount, 0, 0.02f);
        transform.position = cameraStartPos + Random.onUnitSphere * currShakeAmount;
    }

    public static void Shake()
    {
        currShakeAmount = shakeAmount;
    }
}