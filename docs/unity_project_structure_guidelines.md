# Unity 프로젝트 폴더 구조 가이드

## 목적

Unity 프로젝트를 `_Project` 기반 feature-based 구조로 정리할 때의 판단 기준을 정의한다.

이 문서는 특정 프로젝트 전용 규칙이 아니라, Unity 프로젝트 전반에 재사용할 수 있는 폴더 리팩토링 기준을 목표로 한다.

## 핵심 원칙

1. 프로젝트 소유물과 외부 자산을 분리한다.
2. 최상위 폴더 수를 적게 유지한다.
3. 기능 단위로 먼저 묶고, 필요한 경우 feature 안에서 계층을 나눈다.
4. Unity 의존 코드는 `Presentation`에 둔다.
5. 순수 C# 규칙 로직이 생긴 뒤에만 `Domain`을 만든다.
6. 저장, 로딩, 외부 SDK 연결이 생긴 뒤에만 `Infrastructure`를 만든다.
7. 빈 계층 폴더는 만들지 않는다.
8. 파일 이동 시 `.meta`를 함께 이동해 GUID 참조를 보존한다.

## `_Project`를 쓰는 이유

`_Project`는 아키텍처 계층이 아니라 Unity 에디터 정리 관습이다.

Unity 프로젝트에는 에셋스토어 패키지, SDK, 샘플, URP 설정, TextMesh Pro 리소스 등이 계속 추가된다. `_Project` 아래에 팀이 직접 관리하는 파일을 모으면 외부 자산과 프로젝트 소유물을 구분하기 쉽다.

```text
Assets/
  _Project/
  Plugins/
  Settings/
  TextMesh Pro/
  ThirdParty/
```

언더스코어는 Project 창 정렬에서 상단에 오게 하기 위한 관습이다. 팀 선호에 따라 `_Game`, `_App`, `Game`, 프로젝트명 폴더를 써도 된다.

## 기본 구조

작거나 중간 규모의 Unity 게임은 아래 구조를 기본값으로 둔다.

```text
Assets/
  _Project/
    Scenes/
    Features/
      FeatureName/
        Presentation/
        Application/
        Domain/
        Infrastructure/
    Common/
    Art/
    Audio/
    Input/
```

모든 feature가 모든 계층을 가져야 하는 것은 아니다. 실제 파일이 생길 때만 폴더를 만든다.

## 계층 판단 기준

### Domain

게임 규칙, 상태, 판정 로직을 둔다.

조건:

- `UnityEngine` 없이 컴파일 가능하다.
- `MonoBehaviour`, `GameObject`, `Transform`, `Scene`, `Prefab`을 모른다.
- 테스트나 시뮬레이션에서 Unity 에디터 없이 실행할 가치가 있다.

예시:

- 보드 상태
- 규칙 판정기
- 오염 상태 계산기
- 스테이지 목표
- 점수 계산

아직 순수 C# 객체가 없으면 `Domain` 폴더를 만들지 않는다.

### Application

플레이 흐름과 유스케이스를 둔다.

조건:

- 여러 도메인 객체를 조합해 한 행동을 처리한다.
- 저장소, 상태, 규칙 평가를 연결한다.
- Unity 입력이나 화면 표현 자체는 직접 다루지 않는다.

예시:

- 스테이지 시작
- 퍼즐 평가
- 리셋 처리
- 런 상태 진행

### Presentation

Unity에 직접 붙는 표현, 입력, 연출 코드를 둔다.

조건:

- `MonoBehaviour`다.
- `GameObject`, `Transform`, `Collider`, `Renderer`, `Light`, `Camera`, UI를 다룬다.
- 도메인 상태를 화면에 표시하거나 플레이어 입력을 Unity 객체로 받는다.

예시:

- 플레이어 컨트롤러
- 오브젝트 뷰
- HUD
- 문, 트리거, 상호작용 컴포넌트
- 조명, 공간 반응 연출

### Infrastructure

저장 방식, 데이터 로딩 방식, 외부 시스템 연결을 둔다.

조건:

- `ScriptableObject` 데이터, JSON, `PlayerPrefs`, 파일 저장, 서버, SDK를 다룬다.
- 도메인 객체와 외부 표현 사이를 변환한다.

예시:

- ScriptableObject 기반 스테이지 저장소
- SaveRepository
- Addressables 로딩
- 외부 분석 SDK 어댑터

## Feature 분리 기준

feature는 파일 종류가 아니라 팀이 이해하는 기능 경계로 나눈다.

좋은 feature 기준:

- 같이 바뀌는 파일이 많다.
- 플레이어 경험상 하나의 기능으로 설명된다.
- 담당자나 작업 단위가 자연스럽게 나뉜다.
- 다른 feature 없이 독립적으로 읽을 수 있다.

나쁜 feature 기준:

- `Managers`, `Systems`, `Helpers`처럼 역할이 모호하다.
- 파일이 1개뿐인데 미래를 예상해서 과하게 분리한다.
- `Domain`, `Presentation` 같은 계층을 최상위 기준으로 먼저 나눈다.

## 에셋 타입별 폴더 기준

모든 에셋을 feature 안에 넣을 필요는 없다.

공용성이 높거나 타입별 관리가 더 자연스러운 항목은 `_Project` 바로 아래에 둔다.

```text
Assets/
  _Project/
    Art/
    Audio/
    Input/
    Scenes/
```

feature 전용 프리팹이나 ScriptableObject가 많아지면 해당 feature 아래로 이동한다.

```text
Assets/
  _Project/
    Features/
      Stage/
        ScriptableObjects/
      UI/
        Prefabs/
```

## 폴더를 만들지 않는 기준

아래 경우에는 폴더를 만들지 않는다.

- 아직 파일이 없다.
- 파일 1개를 위해 계층을 4개로 나눠야 한다.
- 이름이 미래 계획만 설명하고 현재 책임을 설명하지 못한다.
- Unity 의존 코드밖에 없는데 `Domain`이라고 부르는 경우.
- 저장이나 로딩 코드가 없는데 `Infrastructure`를 만드는 경우.

## 리팩토링 절차

1. 현재 `Assets` 루트 파일을 파악한다.
2. 프로젝트 소유물과 외부, 설정, 샘플을 구분한다.
3. 프로젝트 소유물을 `_Project` 아래로 모은다.
4. 기능 경계를 3~6개 정도로만 먼저 잡는다.
5. 각 feature 내부에는 현재 필요한 계층만 만든다.
6. 파일과 `.meta`를 함께 이동한다.
7. 문자열 경로를 쓰는 빌더, 설정, 에디터 스크립트를 수정한다.
8. Unity에서 refresh와 compile을 확인한다.
9. 씬 열기, 빌더 실행, 플레이 모드 진입을 확인한다.

## 현재 프로젝트 적용

`expost`의 `RuleReconstruction`은 순수 C# 규칙 로직, 플레이 진행 유스케이스, ScriptableObject/Resources 기반 로딩, Canvas 기반 프로토타입 UI가 분리되어 있으므로 아래 구조를 사용한다.

```text
Assets/
  _Project/
    Scenes/
      SampleScene.unity
    Input/
      InputSystem_Actions.inputactions
    Features/
      RuleReconstruction/
        Application/
          RuleReconstructionSession.cs
        Domain/
          PrototypeStageFactory.cs
          PuzzleModel.cs
          RuleSimulator.cs
          StageRuleAnalyzer.cs
          Validator.cs
        Infrastructure/
          StageAsset.cs
          StageRepository.cs
        Presentation/
          RuleReconstructionPrototype.cs
```

판단:

- `Application`: 선택 규칙, 스테이지 이동, 시뮬레이션 진행 상태, 검증 결과 갱신을 조합한다.
- `Domain`: Unity 없이 실행 가능한 보드, 규칙, 검증, 스테이지 분석 로직이다.
- `Infrastructure`: `ScriptableObject`와 `Resources` 기반 스테이지 로딩을 담당한다.
- `Presentation`: `MonoBehaviour`, Canvas, UI, 입력 버튼, 시뮬레이션 표시를 담당한다.
- `Scenes`: 프로젝트 소유 씬이므로 `_Project/Scenes`에 둔다.
- `Input`: 여러 feature가 참조할 수 있는 공용 입력 자산이므로 `_Project/Input`에 둔다.

`Assets/Settings`, URP 프로필, 렌더러 설정 등 Unity 프로젝트 설정 자산은 외부 패키지와 프로젝트 소유 코드의 경계가 아니므로 기존 위치를 유지한다.
