using UnityEngine;
using System;

public class SkyTimeSync : MonoBehaviour
{
    public Material skyMaterial; // SimpleSky 머티리얼 연결

    void Update()
    {
        // 1. 현재 시간의 진행도 계산 (0.0 ~ 1.0)
        TimeSpan now = DateTime.Now.TimeOfDay;
        float dayPercent = (float)now.TotalSeconds / 86400f;

        // 2. 오프셋 계산 공식: (시간 비율 * 전체 이동량 1.0) + 시작점 0.5
        float offset = dayPercent + 0.5f;

        // 3. 순환 처리 (1.0을 넘어가면 0.0부터 다시 시작)
        // 예: 오후 6시(0.75) -> 0.75 + 0.5 = 1.25 -> 여기서 1.0을 빼면 0.25 (오후 색상)
        if (offset >= 1.0f)
        {
            offset -= 1.0f;
        }

        // 4. 머티리얼에 적용
        skyMaterial.SetTextureOffset("_MainTex", new Vector2(offset, 0));
    }
}