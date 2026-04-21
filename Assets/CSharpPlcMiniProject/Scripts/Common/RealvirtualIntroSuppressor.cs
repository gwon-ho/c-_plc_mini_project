// 코루틴을 사용하기 위한 네임스페이스입니다.
using System.Collections;
// Unity의 GameObject, Transform, RuntimeInitializeOnLoadMethod 같은 기능을 사용하기 위한 네임스페이스입니다.
using UnityEngine;
// Unity UI의 Text 컴포넌트를 찾기 위한 네임스페이스입니다.
using UnityEngine.UI;

// 공통 유틸리티 코드를 프로젝트 전용 네임스페이스로 묶습니다.
namespace CSharpPlcMiniProject.Common
{
    // realvirtual 데모 시작 안내 UI를 자동으로 숨기는 클래스입니다.
    public sealed class RealvirtualIntroSuppressor : MonoBehaviour
    {
        // 숨길 realvirtual 안내창을 식별하기 위해 사용하는 고유 문장입니다.
        private const string IntroTextMarker = "Let's redefine the game of simulation and virtual commissioning.";

        // 씬 로드 직후 자동으로 실행되는 초기화 메서드입니다.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Install()
        {
            // 안내 UI를 숨기는 작업을 수행할 임시 오브젝트를 생성합니다.
            GameObject suppressor = new GameObject("Realvirtual Intro Suppressor");
            // 씬이 바뀌어도 작업 도중 파괴되지 않도록 유지합니다.
            DontDestroyOnLoad(suppressor);
            // 하이어라키에 불필요하게 보이지 않도록 숨김 플래그를 설정합니다.
            suppressor.hideFlags = HideFlags.HideAndDontSave;
            // 실제 숨김 로직을 실행할 컴포넌트를 붙입니다.
            suppressor.AddComponent<RealvirtualIntroSuppressor>();
        }

        // 컴포넌트 시작 시 여러 프레임 동안 안내창을 찾아 숨기는 코루틴입니다.
        private IEnumerator Start()
        {
            // realvirtual 안내 UI가 늦게 생성될 수 있으므로 30프레임 동안 반복 확인합니다.
            for (int i = 0; i < 30; i++)
            {
                // 현재 프레임에 생성된 안내 패널을 찾아 숨깁니다.
                HideIntroPanel();
                // 다음 프레임까지 대기합니다.
                yield return null;
            }

            // 작업이 끝난 임시 오브젝트를 제거합니다.
            Destroy(gameObject);
        }

        // 씬 안의 UI Text를 검사해 realvirtual 안내 패널을 숨기는 메서드입니다.
        private static void HideIntroPanel()
        {
            // 비활성 오브젝트까지 포함해 모든 Text 컴포넌트를 찾습니다.
            Text[] texts = Resources.FindObjectsOfTypeAll<Text>();
            // 찾은 Text 컴포넌트를 하나씩 검사합니다.
            foreach (Text text in texts)
            {
                // Text가 없거나 문자열이 비어 있으면 건너뜁니다.
                if (text == null || string.IsNullOrEmpty(text.text))
                {
                    // 다음 Text를 검사합니다.
                    continue;
                }

                // Text 내용에 realvirtual 안내창 고유 문장이 없으면 건너뜁니다.
                if (!text.text.Contains(IntroTextMarker))
                {
                    // 다음 Text를 검사합니다.
                    continue;
                }

                // 안내 문장을 가진 Text의 부모를 따라 올라가 패널 루트를 찾습니다.
                GameObject panel = FindUiPanelRoot(text.transform);
                // 패널 루트를 찾았으면 비활성화합니다.
                if (panel != null)
                {
                    // 안내 UI가 화면에 나타나지 않도록 끕니다.
                    panel.SetActive(false);
                }
            }
        }

        // Text 오브젝트에서 시작해 안내 UI 패널의 루트 오브젝트를 찾는 메서드입니다.
        private static GameObject FindUiPanelRoot(Transform start)
        {
            // 부모를 따라 올라가기 위해 현재 Transform을 시작 위치로 설정합니다.
            Transform current = start;
            // 더 이상 부모가 없을 때까지 반복합니다.
            while (current != null)
            {
                // realvirtual 안내창의 패널 이름이 Box이고 UI RectTransform을 가진 경우 패널 루트로 판단합니다.
                if (current.name == "Box" && current.GetComponent<RectTransform>() != null)
                {
                    // 찾은 패널 루트 오브젝트를 반환합니다.
                    return current.gameObject;
                }

                // 한 단계 위 부모 Transform으로 이동합니다.
                current = current.parent;
            }

            // 패널 루트를 찾지 못하면 시작 Text 오브젝트 자체를 반환합니다.
            return start.gameObject;
        }
    }
}
