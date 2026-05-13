using UnityEngine;
using System;

public class SkyTimeSync : MonoBehaviour
{
    [Header("연결할 오브젝트")]
    public Material skyMaterial;
    public Light sunLight;

    [Header("조명 밝기 설정")]
    public float maxIntensity = 1.2f;
    public float minIntensity = 0.1f;
    public float lightYAngle = -30f; // 역광 방지용 Y축 각도 (인스펙터에서 조절 가능)

    [Header("테스트 모드 (디버깅용)")]
    public bool useTestTime = false; // 체크하면 아래의 가짜 시간을 사용합니다.
    [Range(0, 23)] public int testHour = 12;   // 테스트 시 (0~23)
    [Range(0, 59)] public int testMinute = 0;  // 테스트 분 (0~59)

    void Update()
    {
        float dayPercent = 0f;

        // 1. 시간 가져오기 (테스트 모드 vs 실제 시간)
        if (useTestTime)
        {
            // 인스펙터에 입력한 가짜 시간으로 계산
            float testSeconds = (testHour * 3600f) + (testMinute * 60f);
            dayPercent = testSeconds / 86400f;
        }
        else
        {
            // 실제 컴퓨터 시간으로 계산
            TimeSpan now = DateTime.Now.TimeOfDay;
            dayPercent = (float)now.TotalSeconds / 86400f;
        }

        // 2. 하늘 오프셋 적용
        float offset = dayPercent + 0.4f;
        if (offset >= 1.0f) offset -= 1.0f;
        skyMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));

        // 3. 조명 각도 및 밝기 조절
        if (sunLight != null)
        {
            float sunRotation = (dayPercent * 360f) - 90f;
            // 역광을 방지하기 위해 사용자가 설정한 lightYAngle(기본 30)을 사용
            sunLight.transform.rotation = Quaternion.Euler(sunRotation, lightYAngle, 0f);

            if (sunRotation > 0 && sunRotation < 180)
            {
                float intensityMultiplier = Mathf.Clamp01(Mathf.Sin(sunRotation * Mathf.Deg2Rad));
                sunLight.intensity = Mathf.Lerp(minIntensity, maxIntensity, intensityMultiplier);
            }
            else
            {
                sunLight.intensity = minIntensity;
            }
        }
    }
}