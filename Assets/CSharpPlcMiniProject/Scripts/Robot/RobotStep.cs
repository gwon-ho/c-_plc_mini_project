// 로봇 제어 상태 enum을 프로젝트 전용 네임스페이스로 묶습니다.
namespace CSharpPlcMiniProject.Robot
{
    // 박스 배출 로봇이 어떤 순서 단계에 있는지 표현하는 상태 목록입니다.
    public enum RobotStep
    {
        // 로봇이 시작 신호를 기다리는 대기 상태입니다.
        Waiting,
        // 로봇이 박스를 잡으러 이동하는 상태입니다.
        MoveToPick,
        // 로봇 그리퍼를 닫아 박스를 잡는 상태입니다.
        CloseGripper,
        // 박스를 기울여 내부 캔을 비우는 상태입니다.
        DumpCans,
        // 비운 박스를 다시 컨베이어 위에 내려놓는 상태입니다.
        PlaceBox,
        // 로봇 그리퍼를 열어 박스를 놓는 상태입니다.
        OpenGripper,
        // 로봇이 초기 대기 위치로 돌아가는 상태입니다.
        MoveToWaitPosition
    }
}
