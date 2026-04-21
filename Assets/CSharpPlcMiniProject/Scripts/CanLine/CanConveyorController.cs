// realvirtual의 PLC 신호 타입과 ControlLogic 기반 클래스를 사용하기 위한 네임스페이스입니다.
using realvirtual;
// Unity의 MonoBehaviour, SelectionBase, Header 같은 기능을 사용하기 위한 네임스페이스입니다.
using UnityEngine;

// 캔 투입 라인 제어 코드를 프로젝트 전용 네임스페이스로 묶습니다.
namespace CSharpPlcMiniProject.CanLine
{
    // 씬에서 이 컴포넌트를 선택하면 루트 오브젝트가 선택되도록 돕는 Unity 속성입니다.
    [SelectionBase]
    // 캔 컨베이어를 PLC 로직처럼 제어하는 클래스입니다.
    public sealed class CanConveyorController : ControlLogic
    {
        // 인스펙터에서 동작 모드 관련 필드를 묶어 보여줍니다.
        [Header("Mode")]
        // 이 컨트롤러를 동작시킬지 끌지 결정하는 수동 활성화 플래그입니다.
        public bool isEnabled = true;

        // 인스펙터에서 PLC 출력 신호들을 묶어 보여줍니다.
        [Header("PLC Outputs")]
        // 캔 컨베이어 모터를 켜고 끄는 PLC 출력 신호입니다.
        public PLCOutputBool startConveyor;
        // 캔이 픽업 위치에 도착했음을 표시하는 램프 출력 신호입니다.
        public PLCOutputBool lampCanAtPosition;

        // 인스펙터에서 PLC 입력 신호들을 묶어 보여줍니다.
        [Header("PLC Inputs")]
        // 캔이 픽업 위치 센서에 감지되었는지 읽는 PLC 입력 신호입니다.
        public PLCInputBool sensorOccupied;
        // 작업자가 캔 컨베이어 운전을 허용했는지 읽는 PLC 입력 신호입니다.
        public PLCInputBool buttonConveyorOn;

        // Unity 물리 주기에 맞춰 반복 실행되는 제어 루프입니다.
        private void FixedUpdate()
        {
            // 비상정지 상태이거나 컨트롤러가 꺼져 있으면 컨베이어를 정지합니다.
            if (ForceStop || !isEnabled)
            {
                // 컨베이어 시작 출력을 false로 내려 안전하게 정지시킵니다.
                Set(startConveyor, false);
                // 아래 제어 로직은 실행하지 않고 빠져나갑니다.
                return;
            }

            // 버튼 신호가 없으면 항상 허용으로 보고, 신호가 있으면 버튼 값을 따릅니다.
            bool operatorAllowsConveyor = buttonConveyorOn == null || buttonConveyorOn.Value;
            // 센서 신호가 있고 센서가 ON이면 캔이 픽업 위치에서 대기 중이라고 판단합니다.
            bool canIsWaitingAtPickPosition = sensorOccupied != null && sensorOccupied.Value;

            // 작업자가 허용했고 픽업 위치에 캔이 없을 때만 컨베이어를 구동합니다.
            Set(startConveyor, operatorAllowsConveyor && !canIsWaitingAtPickPosition);
            // 픽업 위치에 캔이 있으면 상태 표시 램프를 켭니다.
            Set(lampCanAtPosition, canIsWaitingAtPickPosition);
        }

        // PLCOutputBool 신호가 null인지 확인한 뒤 안전하게 값을 쓰는 헬퍼 메서드입니다.
        private static void Set(PLCOutputBool signal, bool value)
        {
            // 신호 연결이 되어 있을 때만 값을 씁니다.
            if (signal != null)
            {
                // 실제 PLC 출력 신호 값을 변경합니다.
                signal.Value = value;
            }
        }
    }
}
