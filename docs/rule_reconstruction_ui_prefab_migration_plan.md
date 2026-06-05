# Rule Reconstruction UI 프리팹 전환 계획

## 목적

현재 Rule Reconstruction UI는 Unity 씬과 프리팹보다 C# 코드 생성에 크게 의존한다.
이 구조는 빠른 프로토타입에는 유효했지만, 실제 게임 UI로 다듬기에는 유지보수성이 낮다.

목표는 정적 레이아웃과 반복 UI 단위를 Unity Editor에서 직접 확인하고 수정할 수 있는 구조로 바꾸는 것이다.

## 현재 문제

- 정적 레이아웃이 코드에 숨어 있어 씬에서 화면 구조를 파악하기 어렵다.
- 패널, 버튼, 슬롯, 장식 요소가 코드로 생성되어 RectTransform 조정 비용이 크다.
- 반복 UI가 프리팹화되어 있지 않아 룰 카드, 슬롯, 버튼 스타일이 코드 곳곳에 퍼진다.
- 씬에 있는 UI와 코드가 생성하는 UI 사이의 기준이 불명확하다.
- 시각 폴리싱을 할 때 코드 수정, 실행, 캡처 확인 루프가 과도하게 길어진다.

## 전환 원칙

- 화면의 고정 구조는 씬에 둔다.
- 반복되는 UI 단위는 프리팹으로 만든다.
- 런타임 데이터에 따라 개수가 달라지는 요소만 Instantiate한다.
- Instantiate 대상도 빈 GameObject 조립이 아니라 프리팹 복제로 처리한다.
- C#은 UI 생성보다 바인딩, 상태 갱신, 입력 처리에 집중한다.
- 순수 규칙 로직은 Domain/Application에 유지하고 Presentation은 화면 표시만 담당한다.

## 목표 씬 구조

```text
RuleReconstructionScreen
  Header
    StageTitleText
    PrevButton
    NextButton
  Content
    Sidebar
      RuleListRoot
      BlockTrayRoot
      ActionButtonRoot
      StatusRoot
    BoardArea
      BoardTitle
      BoardRoot
      ResultOverlayRoot
```

씬은 큰 레이아웃, 여백, 배경, 고정 버튼 위치를 가진다.
코드는 위 오브젝트들을 직렬화 필드로 참조한다.

## 목표 프리팹

```text
Assets/_Project/Features/RuleReconstruction/Presentation/Prefabs/
  RuleCard.prefab
  RuleSlot.prefab
  BlockButton.prefab
  BoardCell.prefab
  IconButton.prefab
  ResultMarker.prefab
```

### RuleCard.prefab

색상, 범위, 거리, 효과 슬롯을 가진 룰 한 줄이다.
선택 상태는 배경 교체보다 outline, 좌측 인디케이터, 슬롯 강조로 표시한다.

### RuleSlot.prefab

룰 카드 안에 들어가는 단일 슬롯이다.
색, 범위, 거리, 효과를 모두 같은 슬롯 구조로 표현한다.

### BlockButton.prefab

하단 블록 트레이에서 선택 가능한 블록이다.
카테고리별 outline 색상과 선택 상태를 가진다.

### BoardCell.prefab

보드 한 칸이다.
숫자, 색상 source, 오답 하이라이트, 증감 결과 표시를 담당한다.

### IconButton.prefab

Prev, Next, Test, Target 같은 아이콘 버튼이다.
비활성 상태와 hover/pressed 상태를 프리팹에서 통일한다.

### ResultMarker.prefab

클리어, 오답, 비교 결과 표시용 마커다.
큰 텍스트 대신 보드 위에 겹치는 짧은 시각 신호로 사용한다.

## 코드 역할 재정의

### 제거 대상

- 정적 패널 생성 코드
- 고정 버튼 생성 코드
- 고정 레이아웃 RectTransform 수치 조립
- 텍스트/이미지 오브젝트를 매번 새로 만드는 UI Factory 책임

### 유지 대상

- 스테이지 데이터 로딩
- 룰 선택 상태 갱신
- 블록 선택 상태 갱신
- 보드 셀 데이터 바인딩
- 시뮬레이션 단계 표시
- 클리어/오답 상태 표시

## 예상 클래스 역할

```text
RuleReconstructionGame
  세션 상태와 View 바인딩을 조율한다.

RuleReconstructionScreenView
  씬에 배치된 루트 UI 참조를 가진다.

RuleCardView
  RuleCard.prefab의 표시와 선택 상태를 갱신한다.

BlockButtonView
  BlockButton.prefab의 표시와 선택 상태를 갱신한다.

BoardCellView
  BoardCell.prefab의 표시 상태를 갱신한다.

RuleReconstructionBoardRenderer
  BoardCellView 풀 또는 인스턴스를 관리하고 보드 상태를 표시한다.
```

## 마이그레이션 단계

### 1단계: 씬 루트 정리

- `RuleReconstructionScreen` 루트 오브젝트를 씬에 만든다.
- Header, Sidebar, BoardArea, ActionButtonRoot를 씬에 배치한다.
- 현재 코드가 생성하는 고정 루트 패널을 씬 참조로 교체한다.

완료 기준:

- 플레이 모드가 아니어도 기본 화면 골격이 씬에서 보인다.
- Test/Target 버튼이 씬 배치 기준으로 유지된다.

### 2단계: 버튼 프리팹화

- IconButton 프리팹을 만든다.
- Prev, Next, Test, Target 버튼에 같은 프리팹 구조를 적용한다.
- 코드에서는 클릭 이벤트와 활성 상태만 바인딩한다.

완료 기준:

- 버튼 생성 코드가 사라진다.
- 버튼 비활성 상태가 프리팹 스타일로 통일된다.

### 3단계: 보드 셀 프리팹화

- BoardCell 프리팹을 만든다.
- 보드 루트는 씬에 두고, 셀만 프리팹 Instantiate로 생성한다.
- 셀 크기, 색, 폰트, outline은 프리팹 기준으로 관리한다.

완료 기준:

- 보드 셀 GameObject 조립 코드가 사라진다.
- 보드 크기 변경은 프리팹과 Grid/Layout 설정으로 처리된다.

### 4단계: 룰 카드 프리팹화

- RuleCard와 RuleSlot 프리팹을 만든다.
- 룰 개수만큼 RuleCard를 Instantiate한다.
- 각 슬롯은 색, 범위, 거리, 효과 상태를 바인딩한다.

완료 기준:

- 룰 카드 내부 레이아웃 코드가 사라진다.
- 3색/4색 스테이지 전환 시 카드 개수만 변하고 스타일은 유지된다.

### 5단계: 블록 트레이 프리팹화

- BlockButton 프리팹을 만든다.
- 범위, 거리, 효과 그룹 컨테이너는 씬에 둔다.
- 각 그룹 안의 블록만 프리팹으로 생성한다.

완료 기준:

- 블록 버튼 구조가 코드가 아닌 프리팹에서 보인다.
- 선택된 룰 카드와 블록 그룹의 outline 색상이 일관된다.

### 6단계: 임시 UI Factory 축소

- `RuleReconstructionUiFactory`의 책임을 제거하거나 테스트 보조 수준으로 축소한다.
- Presentation 코드는 프리팹 인스턴스와 View 컴포넌트 중심으로 정리한다.

완료 기준:

- 정적 UI 생성 함수가 남아 있지 않다.
- 새 UI 요소를 추가할 때 먼저 씬/프리팹을 수정하게 된다.

## 검증 기준

- 플레이 모드가 아니어도 씬에서 기본 UI 구조가 보인다.
- 모바일 가로 비율에서 좌측 패널과 보드가 겹치지 않는다.
- 3색/4색 스테이지 전환 시 룰 카드와 보드가 중복 생성되지 않는다.
- Test/Target, Prev/Next 버튼이 프리팹 스타일과 활성 상태를 유지한다.
- 실제 Game 화면 캡처에서 텍스트 겹침, 잘림, 과도한 빈 영역이 없다.

## 하지 않을 것

- 규칙 계산 로직을 UI 전환과 함께 바꾸지 않는다.
- 스테이지 기획 데이터를 동시에 재설계하지 않는다.
- 저장/진행도 시스템을 이 전환 작업에 포함하지 않는다.
- 모든 UI를 한 번에 갈아엎지 않고 프리팹 단위로 작게 전환한다.
