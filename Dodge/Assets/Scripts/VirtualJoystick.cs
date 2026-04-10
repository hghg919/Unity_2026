using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 터치 이벤트를 위해 필요

public class VirtualJoystick : MonoBehaviour, IDragHandler, IPointerUpHandler, IPointerDownHandler
{
    private RectTransform bgRect;
    private RectTransform handleRect;
    private Vector3 inputVector;

    // 조이스틱의 입력을 받아갈 플레이어 컨트롤러
    public PlayerController player;

    void Start()
    {
        bgRect = GetComponent<RectTransform>();
        handleRect = transform.GetChild(0).GetComponent<RectTransform>();
    }

    // 드래그 중일 때 호출
    public void OnDrag(PointerEventData eventData)
    {
        Vector2 pos;
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(bgRect, eventData.position, eventData.pressEventCamera, out pos))
        {
            // 배경 안에서 터치 위치 계산
            pos.x = (pos.x / bgRect.sizeDelta.x);
            pos.y = (pos.y / bgRect.sizeDelta.y);

            // 위치 정규화 (-1 ~ 1 사이 값)
            inputVector = new Vector3(pos.x * 2, 0, pos.y * 2);
            inputVector = (inputVector.magnitude > 1.0f) ? inputVector.normalized : inputVector;

            // 핸들 움직이기
            handleRect.anchoredPosition = new Vector2(inputVector.x * (bgRect.sizeDelta.x / 3), inputVector.z * (bgRect.sizeDelta.y / 3));

            // 플레이어에게 방향 전달
            player.SetJoystickInput(inputVector.x, inputVector.z);
        }
    }

    // 터치했을 때
    public void OnPointerDown(PointerEventData eventData)
    {
        OnDrag(eventData);
    }

    // 손을 뗐을 때 (원래 위치로 복귀)
    public void OnPointerUp(PointerEventData eventData)
    {
        inputVector = Vector3.zero;
        handleRect.anchoredPosition = Vector2.zero;
        player.SetJoystickInput(0, 0); // 멈춤
    }
}