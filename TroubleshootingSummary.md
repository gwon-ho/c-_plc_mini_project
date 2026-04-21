<img width="1022" height="605" alt="19-30-44+(1)" src="https://github.com/user-attachments/assets/7e0d782b-fb43-41d3-8227-30835cb8d52e" />﻿# C# PLC Mini Project Troubleshooting Summary

이 문서는 Unity와 realvirtual 기반의 `DemoRealvirtualOld_CleanControl` 씬을 PLC식 C# 제어 구조로 정리하면서 발생한 주요 문제, 원인 분석, 시도한 해결 방법, 최종 결과를 기록한 문서입니다.

프로젝트 방향은 다음과 같습니다.

- realvirtual의 3D 모델, Drive, Gripper, Sensor, PLC Signal 컴포넌트는 활용한다.
- 공정 제어 로직은 직접 작성한 C# 스크립트로 구성한다.
- PLC 순차제어처럼 Output 명령과 Input 피드백을 분리한다.
- 면접에서 C# 적응력, PLC 제어 사고방식, 디버깅 과정을 설명할 수 있게 한다.

  
## 1. Y축으로 이동하지 않는 문제

### 문제

캔을 감지하고 집은 뒤 박스 쪽으로 이동해야 하는데, Gantry Y축이 움직이지 않았습니다.
<img width="2986" height="1806" alt="image" src="https://github.com/user-attachments/assets/8f648f6a-19dd-418b-ad42-8250963d779b" />

관찰된 상태:

- `Target Y Position Status`는 목표값으로 바뀜
- `Gantry Y Destination Signal Status`도 목표값으로 바뀜
- `Gantry Y Position Status`는 변하지 않음
- `Gantry Y Drive Is Running Status`가 켜지지 않음
- Inspector에서 `Target Start Move`를 수동 체크해도 바로 꺼짐
 
### 발생 원인

`Target Start Move`는 수동으로 유지하는 값이 아니라 `GantryYStart` PLC Output에 의해 매 프레임 제어되는 값이었습니다. 실제 이동이 안 된 더 큰 원인은 Gantry Drive의 `Smooth Acceleration` 옵션이었습니다.

realvirtual 기본/Free 환경에서는 Smooth Acceleration이 정상 동작하지 않거나 Professional 기능 제한의 영향을 받을 수 있었고, 이 때문에 목표 위치는 들어가지만 Drive가 실제로 출발하지 않았습니다.

### 시도한 내용

- `GantryYStart`, `GantryYDestination`, `GantryYDriving`, `GantryYAtDestination` 디버그 필드를 추가했습니다.
- `Drive.TargetPosition`, `Drive.TargetStartMove`, `Drive.IsRunning`, `Drive.IsAtTarget`, `Drive.SmoothAcceleration` 상태를 Inspector에서 보이게 했습니다.
- `Target Start Move`를 수동으로 켜는 방식이 아니라 PLC Output을 통해 제어해야 함을 확인했습니다.

### 해결 방법

`HandlingController.ResolveMissingSignals()`에서 `GantryY`, `GantryZ` Drive를 찾아 다음 설정을 적용했습니다.

```csharp
drive.SmoothAcceleration = false;
```

### 결과

Gantry Y축이 정상적으로 움직였고, 캔을 들어 올린 뒤 박스 위치로 이동하는 흐름이 가능해졌습니다.



## 2. 캔 적재 후 불필요한 동작이 반복되는 문제

### 문제

캔을 박스에 내려놓은 뒤 다음과 같은 이상 동작이 발생했습니다.
<img width="1432" height="754" alt="18-13-30 (1)" src="https://github.com/user-attachments/assets/a6904bc6-0ef3-483b-b4bf-3d19d0c681d2" />


- 캔을 놓고 올라감
- 다시 같은 위치로 내려감
- 캔을 잡으러 가려다가 다시 박스 쪽으로 감
- 이후 다시 정상 흐름으로 복귀하는 듯한 불안정한 움직임 발생

### 발생 원인 분석

처음에는 `LoaderAtDestination()`이 너무 빨리 true가 되는 문제로 의심했습니다.

가능성:

- 이전 이동의 `AtDestination` 신호가 남아 있음
- 새 이동 명령 직후 Drive 피드백이 아직 갱신되지 않았는데 다음 Step으로 넘어감
- `Drive.IsAtTarget`과 PLC Input 피드백 사이에 타이밍 차이가 있음
- `AxisNeedsMove()` 최적화가 실제 필요한 명령을 생략함

### 시도한 내용

- `AxisNeedsMove()`를 추가해 필요한 축만 이동시키려 했습니다.
- 하지만 이 최적화로 인해 `Last Command`는 `Z=0`인데 실제 Drive Target은 이전 값 `310`에 남는 문제가 관찰되었습니다.
- 이후 `AxisNeedsMove()` 최적화를 제거하고, 이동 명령 시 Y/Z 목표값과 Start 신호를 항상 다시 쓰도록 변경했습니다.
- `LoaderAtDestination()`에 짧은 안정화 시간 `commandSettleSeconds`를 추가했습니다.
- `AxisAtTarget()`은 Drive 내부값보다 PLC 위치 피드백을 우선해서 판단하도록 변경했습니다.

### 해결 방법

최종적으로 이동 명령은 명확하게 다시 쓰는 구조로 정리했습니다.

- `MoveLoaderTo()` 호출 시 Y/Z Destination을 항상 기록
- Y/Z Start Pulse를 항상 발생
- `DriveTo()`도 항상 호출
- `LoaderAtDestination()`은 명령 직후 바로 완료되지 않도록 최소 안정화 시간 적용
- 실제 위치 피드백인 `GantryYCurrentPosition`, `GantryZCurrentPosition`을 우선 사용

### 결과

이전 명령 잔상과 Drive Target 불일치 문제를 줄였고, 상태 전이 디버깅이 가능해졌습니다. 다만 이후 별도의 신호 충돌 문제가 발견되어 추가 분석을 진행했습니다.

## 3. 그리퍼 완료 확인 없이 다음 동작으로 넘어가는 문제

### 문제

처음 구조에서는 `WaitingForCan`에서 캔이 감지되면 그리퍼 열기 명령을 내린 직후 바로 픽업 위치로 이동했습니다.
<img width="1022" height="605" alt="19-30-44+(1)" src="https://github.com/user-attachments/assets/e422c362-7c65-4354-9188-67a0b401b660" />

예시:

```csharp
Set(openGripper, true);
Set(closeGripper, false);
MoveLoaderTo(pickCanPosY, pickCanPosZ);
currentStep = HandlingStep.MoveToPick;
```

### 발생 원인

PLC식 순차제어 관점에서는 명령 출력과 완료 입력이 분리되어야 합니다.

- `openGripper`: 열기 명령 Output
- `gripperOpened`: 실제 열림 완료 Input

하지만 초기 구조에서는 `gripperOpened`가 실제로 켜졌는지 확인하지 않고 다음 Step으로 넘어갔습니다.

### 시도한 내용

처음에는 별도 Step을 추가했습니다.

```text
WaitingForCan
-> OpenGripperBeforePick
-> MoveToPick
-> CloseGripper
```

하지만 한 번에 상태가 많이 바뀌면서 전체 동작이 더 꼬였습니다. 이후 변경 범위를 줄여 `RunStepFeedbackAware()` 구조로 다시 정리했습니다.

### 해결 방법

그리퍼 명령과 완료 확인을 helper로 분리했습니다.

- `SetGripperCommand(bool open)`
- `GripperOpenedComplete()`
- `GripperClosedComplete()`

또한 명령 직후 센서가 바로 따라오는 경우를 피하기 위해 `gripperCommandSettleSeconds`를 추가했습니다.

### 결과

제어 구조가 PLC식 설명에 가까워졌습니다. 다만 실제 `gripperOpened`가 물리적 완료 피드백인지, 명령을 거의 즉시 따라오는 신호인지 별도 검증이 필요하다는 점도 확인했습니다.

## 4. OpenGripper 상태에서 Z축이 올라가는 문제

### 문제

가장 큰 문제였습니다.

`OpenGripper` 상태에서는 그리퍼가 열릴 때까지 기다려야 하는데, 실제 화면에서는 그리퍼가 열리는 도중 Z축 또는 그리퍼가 위로 올라가는 것처럼 보였습니다.
<img width="1790" height="942" alt="18-25-32" src="https://github.com/user-attachments/assets/3a436d72-67fb-4197-9716-55fb19ccea5e" />


관찰된 상태:

- `Current Step = OpenGripper`
- `Last Command = Move Y=430.0, Z=310.0`
- `Target Z Position Status = 310`
- `Gantry Z Position Status = 308.9997` 근처에서 변하지 않음
- 그런데 화면상으로는 위로 올라감
- 이후 `Gantry Z Drive Target Position Status`가 `0`으로 튀는 현상도 관찰됨

### 발생 원인 분석

처음에는 `positionTolerance = 2.0f` 때문에 `308.9997`을 도착으로 판단해서 Step이 너무 빨리 넘어간 것으로 의심했습니다.

하지만 사용자의 관찰로 핵심이 바뀌었습니다.

- `OpenGripper` 상태로 넘어가는 것 자체는 문제 아님
- 문제는 `OpenGripper` 상태에서 대기해야 하는데 실제로 올라가는 것
- `Gantry Z Position Status`가 변하지 않는데 눈으로는 올라감

이 단서로 인해 실제로 움직이는 것은 우리가 읽는 `GantryZCurrentPosition` 축이 아닐 수 있음을 확인했습니다.

이후 더 중요한 원인을 찾았습니다.

기존 realvirtual 데모 스크립트 `PLC_Handling`이 씬에 남아 있었고, 이 스크립트도 같은 PLC 신호를 제어하고 있었습니다.

즉 두 컨트롤러가 같은 신호를 동시에 쓰고 있었습니다.

```text
HandlingController
-> GantryZDestination, GantryZStart 제어

PLC_Handling
-> GantryZDestination, GantryZStart 제어
```

기존 `PLC_Handling`은 자체 상태머신에서 `openinggripper` 이후 `DriveLoaderTo(placepos, TransportCanPosZ)`를 호출하여 `Z=0` 상승 명령을 보낼 수 있었습니다.

### 시도한 내용

- `OpenGripper -> LiftAfterPlace` 사이에 `CommandLiftAfterPlace` 단계를 추가했습니다.
- 명령이 실제 Output과 Drive Target에 반영됐는지 확인하는 `gantryYCommandAcceptedStatus`, `gantryZCommandAcceptedStatus`를 추가했습니다.
- 하지만 이 방식만으로는 문제를 해결하지 못했습니다.
- 이후 `OpenGripper` 중 `Gantry Z Position Status`가 변하지 않는데 `Drive Target`이 튀는 것을 보고 신호 충돌로 판단했습니다.

### 해결 방법

`HandlingController` 시작 시 기존 `PLC_Handling`을 자동으로 비활성화했습니다.

```csharp
private void DisableLegacyHandlingControllers()
{
    PLC_Handling[] legacyControllers = Resources.FindObjectsOfTypeAll<PLC_Handling>();
    foreach (PLC_Handling legacy in legacyControllers)
    {
        if (legacy == null || !legacy.gameObject.scene.IsValid())
        {
            continue;
        }

        legacy.On = false;
        legacy.enabled = false;
    }
}
```

이 메서드를 `Awake()`와 `Start()`에서 호출했습니다.

### 결과

`OpenGripper` 상태에서 `Gantry Z Drive Target Position Status`가 `0`으로 튀는 문제가 해결되었습니다.

최종 원인은 다음과 같이 정리됩니다.

```text
기존 realvirtual PLC_Handling과 HandlingController가
같은 Gantry PLC 신호를 동시에 제어하면서 발생한 신호 충돌
```

## 5. 박스 컨베이어가 다음 줄로 이동하지 않는 문제

### 문제

Z축 신호 충돌 문제를 해결한 뒤, 기존에는 캔이 한 줄에 다 차면 박스 컨베이어가 움직여 다음 줄에 적재할 수 있었는데, 이제 박스 컨베이어가 움직이지 않았습니다.

### 발생 원인

Z축 문제와 같은 패턴이었습니다.

기존 realvirtual 데모 스크립트 `PLC_BoxConveyor`와 새로 작성한 `BoxConveyorController`가 같은 PLC Output을 동시에 제어하고 있었습니다.

```text
BoxConveyorController
-> startConveyor, startConveyorBackwards 제어

PLC_BoxConveyor
-> StartConveyor, StartConveyorBackwards 제어
```

따라서 PLC-style 컨트롤러가 한 줄 완료 후 `driveNextRow = true`를 주고 `startConveyor = true`를 써도, 기존 `PLC_BoxConveyor`가 바로 `false`로 덮어쓸 수 있었습니다.

### 해결 방법

`BoxConveyorController` 시작 시 기존 `PLC_BoxConveyor`를 자동으로 비활성화했습니다.

```csharp
private void DisableLegacyBoxConveyorControllers()
{
    PLC_BoxConveyor[] legacyControllers = Resources.FindObjectsOfTypeAll<PLC_BoxConveyor>();
    foreach (PLC_BoxConveyor legacy in legacyControllers)
    {
        if (legacy == null || !legacy.gameObject.scene.IsValid())
        {
            continue;
        }

        legacy.On = false;
        legacy.enabled = false;
    }
}
```

이 메서드를 `Awake()`와 `Start()`에서 호출했습니다.

### 결과

기존 `PLC_BoxConveyor`가 PLC-style 컨트롤러의 `startConveyor` 출력을 덮어쓰지 않게 되었고, 한 줄 적재 완료 후 박스 컨베이어가 다음 줄 위치로 이동하는 동작이 복구되었습니다.

## 최종 원인 구조

최종적으로 가장 중요한 원인은 다음이었습니다.

```text
realvirtual 기존 PLC 스크립트와 새 PLC-style C# 스크립트가
같은 PLC Signal을 동시에 제어하고 있었다.
```

이 때문에 Inspector에서 PLC-style 컨트롤러의 상태는 정상처럼 보여도, 실제 Drive Target이나 Conveyor Output은 다른 스크립트에 의해 덮어써졌습니다.

대표적인 충돌:

| 대상 | 기존 스크립트 | 새 스크립트 | 충돌 신호 |
| --- | --- | --- | --- |
| Gantry Handling | `PLC_Handling` | `HandlingController` | `GantryYDestination`, `GantryZDestination`, `GantryYStart`, `GantryZStart`, `OpenGripper`, `CloseGripper` |
| Box Conveyor | `PLC_BoxConveyor` | `BoxConveyorController` | `StartConveyor`, `StartConveyorBackwards`, Box row movement request |

## 최종 해결 전략

최종적으로 다음 전략을 적용했습니다.

1. realvirtual의 3D 모델, Drive, Gripper, Sensor, Signal은 사용한다.
2. 기존 realvirtual 데모 PLC 스크립트는 비활성화한다.
3. 실제 제어 로직은 PLC-style C# 컨트롤러에서만 수행한다.
4. 한 PLC Signal은 하나의 컨트롤러만 제어하도록 한다.
5. Inspector 디버그 필드를 추가해 상태머신, 목표 위치, 실제 위치, Drive 상태, Output/Input 상태를 동시에 확인한다.



## 배운 점

- Unity에서 Play 화면의 움직임만 보면 원인을 착각하기 쉽다.
- Inspector에 상태, 명령, 피드백, Drive Target을 분리해서 표시하면 원인을 훨씬 빨리 좁힐 수 있다.
- PLC식 제어에서는 Output과 Input을 분리해서 생각해야 한다.
- `명령을 냈다`와 `동작이 완료됐다`는 다른 개념이다.
- 하나의 PLC Signal을 두 스크립트가 동시에 제어하면 상태머신은 정상이어도 실제 설비 동작은 꼬일 수 있다.
- realvirtual 같은 프레임워크를 사용할 때는 기존 데모 제어 스크립트가 살아 있는지 반드시 확인해야 한다.
