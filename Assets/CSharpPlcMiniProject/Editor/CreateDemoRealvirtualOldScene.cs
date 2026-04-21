// 박스 컨베이어 컨트롤러를 생성하고 연결하기 위한 네임스페이스입니다.
using CSharpPlcMiniProject.BoxLine;
// 캔 컨베이어 컨트롤러를 생성하고 연결하기 위한 네임스페이스입니다.
using CSharpPlcMiniProject.CanLine;
// 핸들링 컨트롤러를 생성하고 연결하기 위한 네임스페이스입니다.
using CSharpPlcMiniProject.Handling;
// 로봇 컨트롤러를 생성하고 연결하기 위한 네임스페이스입니다.
using CSharpPlcMiniProject.Robot;
// realvirtual 원본 PLC 스크립트와 PLC 신호 타입을 참조하기 위한 네임스페이스입니다.
using realvirtual;
// Unity 에디터 메뉴와 AssetDatabase 기능을 사용하기 위한 네임스페이스입니다.
using UnityEditor;
// Unity 에디터에서 씬을 열고 저장하기 위한 네임스페이스입니다.
using UnityEditor.SceneManagement;
// Unity의 GameObject, Component, Resources 같은 기능을 사용하기 위한 네임스페이스입니다.
using UnityEngine;
// Unity 씬 타입과 씬 관련 기능을 사용하기 위한 네임스페이스입니다.
using UnityEngine.SceneManagement;

// 에디터 자동화 코드를 프로젝트 전용 네임스페이스로 묶습니다.
namespace CSharpPlcMiniProject.Editor
{
    // 원본 realvirtual 데모 씬을 복사해 우리 제어 코드가 붙은 씬을 생성하는 에디터 유틸리티 클래스입니다.
    public static class CreateDemoRealvirtualOldScene
    {
        // 복사할 realvirtual 원본 데모 씬 경로입니다.
        private const string SourceScenePath = "Assets/realvirtual/Scenes/DemoRealvirtualOld.unity";
        // 생성할 우리 프로젝트용 클린 제어 씬 경로입니다.
        private const string TargetScenePath = "Assets/Scenes/DemoRealvirtualOld_CleanControl.unity";

        // Unity 상단 메뉴에 씬 생성 명령을 추가합니다.
        [MenuItem("Tools/C# PLC Mini Project/Create DemoRealvirtualOld PLC Scene")]
        public static void CreateScene()
        {
            // Assets/Scenes 폴더가 없으면 생성합니다.
            EnsureScenesFolder();
            // 원본 씬을 대상 경로로 복사합니다.
            CopySourceScene();

            // 복사한 대상 씬을 단일 씬 모드로 엽니다.
            Scene scene = EditorSceneManager.OpenScene(TargetScenePath, OpenSceneMode.Single);

            // 원본 캔 컨베이어 PLC 스크립트를 찾습니다.
            PLC_CanConveyor oldCanConveyor = FindRequired<PLC_CanConveyor>();
            // 원본 박스 컨베이어 PLC 스크립트를 찾습니다.
            PLC_BoxConveyor oldBoxConveyor = FindRequired<PLC_BoxConveyor>();
            // 원본 핸들링 PLC 스크립트를 찾습니다.
            PLC_Handling oldHandling = FindRequired<PLC_Handling>();
            // 원본 로봇 PLC 스크립트를 찾습니다.
            PLC_Robot oldRobot = FindRequired<PLC_Robot>();

            // 우리가 만든 제어 스크립트들을 담을 루트 오브젝트를 생성합니다.
            GameObject root = new GameObject("PLC Controllers");

            // 캔 컨베이어 제어 오브젝트를 만들고 컨트롤러를 붙입니다.
            CanConveyorController canConveyor = CreateChild<CanConveyorController>(root, "PLC - Can Conveyor");
            // 박스 컨베이어 제어 오브젝트를 만들고 컨트롤러를 붙입니다.
            BoxConveyorController boxConveyor = CreateChild<BoxConveyorController>(root, "PLC - Box Conveyor");
            // 핸들링 제어 오브젝트를 만들고 컨트롤러를 붙입니다.
            HandlingController handling = CreateChild<HandlingController>(root, "PLC - Handling");
            // 로봇 제어 오브젝트를 만들고 컨트롤러를 붙입니다.
            RobotController robot = CreateChild<RobotController>(root, "PLC - Robot");

            // 원본 캔 컨베이어 PLC 신호를 새 컨트롤러에 연결합니다.
            ConnectCanConveyor(canConveyor, oldCanConveyor);
            // 원본 박스 컨베이어 PLC 신호를 새 컨트롤러에 연결합니다.
            ConnectBoxConveyor(boxConveyor, oldBoxConveyor, robot);
            // 원본 핸들링 PLC 신호를 새 컨트롤러에 연결합니다.
            ConnectHandling(handling, oldHandling, boxConveyor);
            // 원본 로봇 PLC 신호를 새 컨트롤러에 연결합니다.
            ConnectRobot(robot, oldRobot);

            // 원본 데모 PLC 스크립트는 중복 제어를 막기 위해 비활성화합니다.
            DisableOldDemoControllers(oldCanConveyor, oldBoxConveyor, oldHandling, oldRobot);

            // 씬이 변경되었음을 Unity 에디터에 알립니다.
            EditorSceneManager.MarkSceneDirty(scene);
            // 변경된 씬을 대상 경로에 저장합니다.
            EditorSceneManager.SaveScene(scene, TargetScenePath);
            // 생성된 루트 오브젝트를 선택해 사용자가 바로 확인할 수 있게 합니다.
            Selection.activeGameObject = root;

            // 씬 생성이 끝났음을 사용자에게 알리는 대화상자를 표시합니다.
            EditorUtility.DisplayDialog(
                "PLC Demo Created",
                "Created Assets/Scenes/DemoRealvirtualOld_CleanControl.unity\n\nThe original DemoRealvirtualOld scene was not modified.",
                "OK");
        }

        // Assets/Scenes 폴더가 존재하는지 확인하고 없으면 생성하는 메서드입니다.
        private static void EnsureScenesFolder()
        {
            // Assets/Scenes 폴더가 없으면 새로 만듭니다.
            if (!AssetDatabase.IsValidFolder("Assets/Scenes"))
            {
                // Assets 폴더 아래에 Scenes 폴더를 생성합니다.
                AssetDatabase.CreateFolder("Assets", "Scenes");
            }
        }

        // 원본 씬을 대상 씬 경로로 복사하는 메서드입니다.
        private static void CopySourceScene()
        {
            // 원본 씬 에셋이 존재하지 않으면 예외를 발생시킵니다.
            if (!AssetDatabase.LoadAssetAtPath<SceneAsset>(SourceScenePath))
            {
                // 어떤 원본 씬을 찾지 못했는지 경로를 포함해 알려줍니다.
                throw new System.IO.FileNotFoundException("Source scene not found.", SourceScenePath);
            }

            // 대상 씬이 이미 존재하면 새로 만들기 위해 삭제합니다.
            if (AssetDatabase.LoadAssetAtPath<SceneAsset>(TargetScenePath))
            {
                // 이전에 생성한 클린 씬 에셋을 삭제합니다.
                AssetDatabase.DeleteAsset(TargetScenePath);
            }

            // 원본 씬을 대상 경로로 복사하고 실패하면 false가 반환됩니다.
            if (!AssetDatabase.CopyAsset(SourceScenePath, TargetScenePath))
            {
                // 복사 실패 시 명확한 예외를 발생시킵니다.
                throw new System.InvalidOperationException("Failed to copy scene to " + TargetScenePath);
            }

            // AssetDatabase를 갱신해 새 씬이 에디터에 반영되도록 합니다.
            AssetDatabase.Refresh();
        }

        // 부모 오브젝트 아래에 자식 오브젝트를 만들고 지정한 컴포넌트를 붙이는 제네릭 메서드입니다.
        private static T CreateChild<T>(GameObject parent, string name) where T : Component
        {
            // 지정한 이름의 새 자식 오브젝트를 생성합니다.
            GameObject child = new GameObject(name);
            // 생성한 오브젝트를 부모 오브젝트 아래로 배치합니다.
            child.transform.SetParent(parent.transform);
            // 요청한 타입의 컴포넌트를 붙이고 반환합니다.
            return child.AddComponent<T>();
        }

        // 씬에서 반드시 필요한 컴포넌트를 찾아 반환하는 메서드입니다.
        private static T FindRequired<T>() where T : Object
        {
            // 현재 열린 씬에서 지정한 타입의 첫 번째 오브젝트를 찾습니다.
            T instance = Object.FindFirstObjectByType<T>();
            // 찾지 못했다면 씬 구성이 잘못된 것이므로 예외를 발생시킵니다.
            if (instance == null)
            {
                // 어떤 타입을 찾지 못했는지 메시지에 포함합니다.
                throw new System.InvalidOperationException("Required component not found: " + typeof(T).Name);
            }

            // 찾은 컴포넌트를 반환합니다.
            return instance;
        }

        // 원본 캔 컨베이어 PLC 신호를 새 캔 컨트롤러에 연결하는 메서드입니다.
        private static void ConnectCanConveyor(CanConveyorController controller, PLC_CanConveyor old)
        {
            // 원본 On 설정을 새 컨트롤러 활성화 값으로 복사합니다.
            controller.isEnabled = old.On;
            // 원본 컨베이어 시작 출력 신호를 연결합니다.
            controller.startConveyor = old.StartConveyor;
            // 원본 캔 위치 센서 입력 신호를 연결합니다.
            controller.sensorOccupied = old.SensorOccupied;
            // 원본 컨베이어 운전 버튼 입력 신호를 연결합니다.
            controller.buttonConveyorOn = old.ButtonConveyorOn;
            // 원본 캔 위치 램프 출력 신호를 연결합니다.
            controller.lampCanAtPosition = old.LampCanAtPosition;
        }

        // 원본 박스 컨베이어 PLC 신호를 새 박스 컨트롤러에 연결하는 메서드입니다.
        private static void ConnectBoxConveyor(
            BoxConveyorController controller,
            PLC_BoxConveyor old,
            RobotController robot)
        {
            // 원본 On 설정을 새 컨트롤러 활성화 값으로 복사합니다.
            controller.isEnabled = old.On;
            // 원본 다음 행 이동 요청 값을 복사합니다.
            controller.driveNextRow = old.DriveNextRow;
            // 원본 다음 행 이동 거리 값을 복사합니다.
            controller.nextRowDistance = old.NextRowDistance;
            // 원본 박스 교체 사이클 요청 값을 복사합니다.
            controller.boxChangeCycle = old.BoxChangeCycle;
            // 새 로봇 컨트롤러 참조를 연결합니다.
            controller.robot = robot;
            // 원본 정방향 컨베이어 출력 신호를 연결합니다.
            controller.startConveyor = old.StartConveyor;
            // 원본 gantry 위치 센서 입력 신호를 연결합니다.
            controller.sensorGantryOccupied = old.SensorGantryOccupied;
            // 원본 로봇 위치 센서 입력 신호를 연결합니다.
            controller.sensorRobotOccupied = old.SensorRobotOccupied;
            // 원본 컨베이어 현재 위치 입력 신호를 연결합니다.
            controller.conveyorPosition = old.ConveyorPosition;
            // 원본 역방향 컨베이어 출력 신호를 연결합니다.
            controller.startConveyorBackwards = old.StartConveyorBackwards;
            // 원본 박스 위치 램프 출력 신호를 연결합니다.
            controller.lampBoxAtPosition = old.LampBoxAtPosition;
            // 원본 박스 교체 램프 출력 신호를 연결합니다.
            controller.lampBoxChangeCycle = old.LampBoxChangeCycle;
        }

        // 원본 핸들링 PLC 신호와 설정값을 새 핸들링 컨트롤러에 연결하는 메서드입니다.
        private static void ConnectHandling(
            HandlingController controller,
            PLC_Handling old,
            BoxConveyorController boxConveyor)
        {
            // 원본 On 설정을 새 컨트롤러 활성화 값으로 복사합니다.
            controller.isEnabled = old.On;
            // 원본 Y축 목표 위치 출력 신호를 연결합니다.
            controller.gantryYDestination = old.GantryYDestination;
            // 원본 Y축 시작 출력 신호를 연결합니다.
            controller.gantryYStart = old.GantryYStart;
            // 원본 Y축 도착 입력 신호를 연결합니다.
            controller.gantryYAtDestination = old.GantryYAtDestination;
            // 원본 Y축 이동 중 입력 신호를 연결합니다.
            controller.gantryYDriving = old.GantryYDriving;
            // 원본 스크립트에 직접 필드가 없는 Y축 현재 위치 신호는 이름으로 찾아 연결합니다.
            controller.gantryYCurrentPosition = FindSignal<PLCInputFloat>("GantryYCurrentPosition");
            // 원본 Z축 목표 위치 출력 신호를 연결합니다.
            controller.gantryZDestination = old.GantryZDestination;
            // 원본 Z축 시작 출력 신호를 연결합니다.
            controller.gantryZStart = old.GantryZStart;
            // 원본 Z축 도착 입력 신호를 연결합니다.
            controller.gantryZAtDestination = old.GantryZAtDestination;
            // 원본 Z축 이동 중 입력 신호를 연결합니다.
            controller.gantryZDriving = old.GantryZDriving;
            // 원본 스크립트에 직접 필드가 없는 Z축 현재 위치 신호는 이름으로 찾아 연결합니다.
            controller.gantryZCurrentPosition = FindSignal<PLCInputFloat>("GantryZCurrentPosition");
            // 원본 그리퍼 열기 출력 신호를 연결합니다.
            controller.openGripper = old.OpenGripper;
            // 원본 그리퍼 닫기 출력 신호를 연결합니다.
            controller.closeGripper = old.CloseGripper;
            // 원본 그리퍼 열림 입력 신호를 연결합니다.
            controller.gripperOpened = old.GripperOpened;
            // 원본 그리퍼 닫힘 입력 신호를 연결합니다.
            controller.gripperClosed = old.GripperClosed;
            // 원본 캔 감지 센서 입력 신호를 연결합니다.
            controller.sensorCan = old.SensorCan;
            // 원본 박스 교체 버튼 입력 신호를 연결합니다.
            controller.buttonStartChangeCycle = old.ButtonStartChangeCycle;
            // 원본 핸들링 운전 허용 버튼 입력 신호를 연결합니다.
            controller.buttonHandlingOn = old.ButtonHandlingOn;
            // 새 박스 컨베이어 컨트롤러 참조를 연결합니다.
            controller.boxConveyor = boxConveyor;
            // 원본 픽업 Y 위치 설정값을 복사합니다.
            controller.pickCanPosY = old.PickCanPosY;
            // 원본 픽업 Z 위치 설정값을 복사합니다.
            controller.pickCanPosZ = old.PickCanPosZ;
            // 원본 내려놓기 Z 위치 설정값을 복사합니다.
            controller.placeCanPosZ = old.PlaceCanPosZ;
            // 원본 내려놓기 Y 위치 설정값을 복사합니다.
            controller.placeCanPosY = old.PlaceCanPosY;
            // 원본 이동 안전 Z 위치 설정값을 복사합니다.
            controller.transportCanPosZ = old.TransportCanPosZ;
            // 원본 박스 행 수 설정값을 복사합니다.
            controller.numberOfRows = old.NumberOfRows;
            // 원본 박스 열 수 설정값을 복사합니다.
            controller.numberOfColumns = old.NumberOfColums;
            // 원본 열 간격 설정값을 복사합니다.
            controller.distanceColumn = old.DistanceCol;
            // 원본 행 간격 설정값을 복사합니다.
            controller.distanceRow = old.DistancerRow;
        }

        // 씬 안에서 특정 이름을 가진 PLC 신호 컴포넌트를 찾는 메서드입니다.
        private static T FindSignal<T>(string objectName) where T : Component
        {
            // 비활성 오브젝트까지 포함해 해당 타입의 모든 컴포넌트를 찾습니다.
            T[] signals = Resources.FindObjectsOfTypeAll<T>();
            // 찾은 컴포넌트 목록을 하나씩 검사합니다.
            foreach (T signal in signals)
            {
                // 신호가 없거나 게임 오브젝트 이름이 원하는 이름과 다르면 건너뜁니다.
                if (signal == null || signal.gameObject.name != objectName)
                {
                    // 다음 후보를 검사합니다.
                    continue;
                }

                // 프로젝트 에셋이 아니라 실제 씬에 존재하는 오브젝트인지 확인합니다.
                if (!signal.gameObject.scene.IsValid())
                {
                    // 씬 오브젝트가 아니면 건너뜁니다.
                    continue;
                }

                // 조건을 만족하는 신호를 반환합니다.
                return signal;
            }

            // 원하는 신호를 찾지 못했음을 Unity 콘솔에 경고로 표시합니다.
            Debug.LogWarning("Signal component not found: " + objectName + " / " + typeof(T).Name);
            // 신호를 찾지 못했으므로 null을 반환합니다.
            return null;
        }

        // 원본 로봇 PLC 신호와 구성 요소를 새 로봇 컨트롤러에 연결하는 메서드입니다.
        private static void ConnectRobot(RobotController controller, PLC_Robot old)
        {
            // 원본 On 설정을 새 컨트롤러 활성화 값으로 복사합니다.
            controller.isEnabled = old.On;
            // 원본 시작 사이클 상태 값을 복사합니다.
            controller.startCycle = old.StartCycle;
            // 원본 사이클 진행 상태 값을 복사합니다.
            controller.cycleActive = old.CycleActive;
            // 원본 로봇 Animator 참조를 연결합니다.
            controller.robotAnimator = old.RobotAnimator;
            // 원본 realvirtual Grip 참조를 연결합니다.
            controller.gripper = old.Gripper;
            // 원본 로봇 6축 오브젝트 참조를 연결합니다.
            controller.robotAxis6 = old.RobotAxis6;
            // 원본 그리퍼 열기 출력 신호를 연결합니다.
            controller.gripperOpen = old.GripperOpen;
            // 원본 그리퍼 열림 입력 신호를 연결합니다.
            controller.gripperOpened = old.GripperOpened;
            // 원본 그리퍼 닫기 출력 신호를 연결합니다.
            controller.gripperClose = old.GripperClose;
            // 원본 그리퍼 닫힘 입력 신호를 연결합니다.
            controller.gripperClosed = old.GripperClosed;
            // 원본 PLC 시작 사이클 출력 신호를 연결합니다.
            controller.plcStartCycle = old.PLCStartCycle;
            // 원본 PLC 사이클 진행 입력 신호를 연결합니다.
            controller.plcCycleActive = old.PLCCycleActive;
        }

        // 원본 데모 PLC 스크립트를 비활성화해 새 컨트롤러와 중복 제어되지 않도록 하는 메서드입니다.
        private static void DisableOldDemoControllers(
            PLC_CanConveyor canConveyor,
            PLC_BoxConveyor boxConveyor,
            PLC_Handling handling,
            PLC_Robot robot)
        {
            // 원본 캔 컨베이어 PLC 스크립트를 비활성화합니다.
            canConveyor.enabled = false;
            // 원본 박스 컨베이어 PLC 스크립트를 비활성화합니다.
            boxConveyor.enabled = false;
            // 원본 핸들링 PLC 스크립트를 비활성화합니다.
            handling.enabled = false;
            // 원본 로봇 PLC 스크립트를 비활성화합니다.
            robot.enabled = false;
        }
    }
}
