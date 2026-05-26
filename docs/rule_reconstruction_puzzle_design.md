# 규칙 복원 퍼즐 기획 정리

## 1. 핵심 정의

이 게임은 완성된 보드를 맞히는 퍼즐이 아니다.

플레이어는 이미 공개된 최종 보드를 보고, 그 보드를 만들어낸 색상자들의 규칙을 복원한다.

핵심 문장:

> 플레이어는 보드를 고치지 않는다. 플레이어는 보드를 만든 규칙을 복원한다.

## 2. 게임 목표

플레이어에게 주어지는 정보:

- 초기 보드의 색상자 위치
- 목표 최종 보드
- 색상자별 선택 가능한 규칙 후보

플레이어가 제출하는 답:

- 각 색상자가 어떤 방향, 거리, 효과로 주변 칸에 영향을 주는지에 대한 규칙 세트

클리어 조건:

- 플레이어가 선택한 규칙을 시뮬레이션했을 때 결과 보드가 목표 보드와 일치

## 3. 기본 루프

```text
1. 스테이지 입장
2. 초기 상자 배치 확인
3. 목표 최종 상태 확인
4. 색상자별 규칙 선택
5. 시뮬레이션 실행
6. 내 결과와 목표 결과 비교
7. 일치하면 클리어
8. 더 단순한 규칙이나 높은 평가를 위해 재도전
```

## 4. MVP 범위

초기 데모는 작은 규칙 조합으로 재미 검증에 집중한다.

| 항목 | 범위 |
| --- | --- |
| 플랫폼 | Unity PC 또는 WebGL |
| 보드 | 5x5 |
| 스테이지 | 10개 |
| 색상 | 1~2개 |
| 규칙 항목 | 방향, 거리, 효과 |
| 효과 | 숫자 +1 중심 |
| 충돌 처리 | 숫자 누적 |
| 입력 방식 | 보드 직접 수정 없음, 규칙 선택만 가능 |

MVP에서 제외할 요소:

- 좌표 직접 지정 규칙
- 목표 패턴 복사
- 복잡한 조건부 규칙
- 적용 순서 기반 고급 퍼즐
- 유저용 스테이지 에디터

## 5. 규칙 시스템

규칙은 색상자별로 적용된다.

```text
색상자 = 방향 + 거리 + 효과
```

### 방향

| 값 | 의미 |
| --- | --- |
| Cross | 상하좌우 |
| Diagonal | 대각선 |
| Horizontal | 가로 |
| Vertical | 세로 |
| AllAround | 주변 8방향 |

### 거리

| 값 | 의미 |
| --- | --- |
| One | 1칸 |
| Two | 2칸 |
| UntilWall | 벽까지 |
| UntilBeforeBox | 다른 상자 전까지 |

MVP에서는 `One`, `Two`만 사용한다.

### 효과

| 값 | 의미 |
| --- | --- |
| AddNumber | 숫자 +1 |
| Paint | 색칠 |
| Erase | 지우기 |

MVP에서는 `AddNumber`만 사용한다.

## 6. 화면 구성

초기 화면은 세 보드를 동시에 보여준다.

```text
[초기 보드]   [내 규칙 적용 결과]   [목표 보드]
```

하단에는 규칙 선택 UI를 둔다.

- 색상별 방향 선택
- 색상별 거리 선택
- 색상별 효과 선택
- Simulate 버튼
- Submit 버튼
- 틀린 칸 개수 표시
- 클리어 여부 표시

## 7. 보드 표시 정책

초기 보드:

- 색상자 위치 표시
- 빈 칸 표시
- 장애물 또는 벽이 있다면 함께 표시

내 결과 보드:

- 플레이어가 선택한 규칙을 적용한 결과
- 목표와 다른 칸 강조
- 규칙 변경 시 자동 갱신하거나 Simulate 버튼으로 갱신

목표 보드:

- 스테이지의 정답 결과
- 플레이어가 직접 수정할 수 없음

## 8. Unity 구현 방향

게임 로직은 Unity 오브젝트에 의존하지 않는 순수 C# 시뮬레이터로 만든다.

권장 구조:

```text
StageData
SourceBoxData
Rule
RuleSet
BoardState
RuleSimulator
Validator
GridView
RuleControlPanel
```

역할 분리:

| 구성 | 역할 |
| --- | --- |
| StageData | 보드 크기, 상자 배치, 목표 보드, 사용 가능한 규칙 보관 |
| SourceBoxData | 색상자 좌표와 색상 보관 |
| Rule | 방향, 거리, 효과 보관 |
| BoardState | 시뮬레이션 결과 상태 |
| RuleSimulator | 규칙 적용 결과 생성 |
| Validator | 목표 보드와 결과 보드 비교 |
| GridView | 보드 상태를 UI로 표시 |
| RuleControlPanel | 플레이어 규칙 선택 입력 처리 |

## 9. 핵심 데이터 모델

### StageData

```csharp
[CreateAssetMenu(menuName = "Puzzle/Stage")]
public class StageData : ScriptableObject
{
    public int width;
    public int height;
    public List<SourceBoxData> sources;
    public List<CellData> targetCells;
    public List<ColorRuleOptions> availableRules;
}
```

### SourceBoxData

```csharp
[System.Serializable]
public class SourceBoxData
{
    public int x;
    public int y;
    public BoxColor color;
}
```

### Rule

```csharp
public enum DirectionType
{
    Cross,
    Diagonal,
    Horizontal,
    Vertical,
    AllAround
}

public enum RangeType
{
    One,
    Two,
    UntilWall
}

public enum EffectType
{
    AddNumber,
    Paint,
    Erase
}

public class Rule
{
    public DirectionType Direction;
    public RangeType Range;
    public EffectType Effect;
}
```

## 10. 시뮬레이터 책임

RuleSimulator는 다음 순서로 동작한다.

```text
1. 빈 BoardState 생성
2. StageData의 모든 SourceBox 순회
3. SourceBox 색상에 해당하는 Rule 조회
4. 영향 좌표 계산
5. 효과 적용
6. 최종 BoardState 반환
```

구현 단위:

- `GetAffectedPositions`
- `ApplyEffect`
- `ResolveConflict`

MVP 충돌 규칙:

- 같은 칸이 여러 번 영향받으면 숫자를 누적한다.
- 색상자는 결과 보드에서도 위치를 유지한다.
- 보드 밖 좌표는 무시한다.

## 11. 스테이지 예시

초기 배치:

```text
. . . . .
. R . . .
. . . . .
. . . B .
. . . . .
```

목표 상태:

```text
0 1 0 0 0
1 R 1 0 0
0 1 0 1 0
0 0 1 B 1
0 0 0 1 0
```

가능한 정답:

```text
R = 상하좌우 / 1칸 / 숫자 +1
B = 상하좌우 / 1칸 / 숫자 +1
```

## 12. 난이도 조절

난이도는 다음 요소로 조절한다.

| 요소 | 쉬움 | 어려움 |
| --- | --- | --- |
| 색상 수 | 1개 | 3개 이상 |
| 규칙 항목 | 방향만 추론 | 거리, 효과, 조건까지 추론 |
| 보드 크기 | 4x4 | 7x7 이상 |
| 영향 겹침 | 거의 없음 | 여러 색이 같은 칸에 중첩 |
| 검증 보드 | 1개 | 같은 규칙을 여러 보드에 적용 |

추천 해금 순서:

```text
1장: 방향
2장: 거리
3장: 숫자와 색칠
4장: 벽과 장애물
5장: 겹침과 충돌 처리
6장: 적용 순서
7장: 조건부 규칙
8장: 규칙끼리 규칙을 바꾸는 후반 구조
```

## 13. 스테이지 제작 방식

스테이지는 결과를 손으로 만드는 대신 규칙으로 생성한다.

```text
1. 숨겨진 색상별 규칙 설정
2. 상자 배치
3. 시뮬레이션 실행
4. 생성된 결과를 목표 보드로 저장
5. 플레이어에게 초기 배치와 목표 보드만 공개
```

좋은 스테이지 조건:

- 비슷한 가짜 규칙을 탈락시킬 증거가 있음
- 플레이어가 원인을 분해할 수 있음
- 틀렸을 때 차이를 비교하기 쉬움
- 규칙 하나를 바꾸면 결과가 명확히 달라짐

## 14. 평가 구조

유일해를 강제하지 않는다.

기본 클리어:

- 목표 결과와 동일한 보드를 만드는 규칙 세트 제출

추가 평가:

- 더 적은 규칙 항목
- 더 단순한 규칙
- 예외 조건이 적은 규칙
- 숨겨진 테스트 보드까지 통과하는 일반화된 규칙

랭크 예시:

| 랭크 | 조건 |
| --- | --- |
| C | 목표 결과와 일치 |
| B | 규칙 수 제한 만족 |
| A | 예외 조건 없이 클리어 |
| S | 숨겨진 테스트 보드까지 통과 |

## 15. 개발 단계

### 1단계: 핵심 시뮬레이터

- 5x5 보드 생성
- 상자 배치
- 고정 규칙 적용
- 결과 보드 출력
- 목표 보드와 비교

### 2단계: 시각화

- 초기 보드 표시
- 목표 보드 표시
- 시뮬레이션 결과 표시
- 틀린 칸 강조

### 3단계: 규칙 선택 UI

- 색상별 방향 선택
- 색상별 거리 선택
- 색상별 효과 선택
- Simulate 버튼
- Submit 버튼

### 4단계: 스테이지 데이터화

- ScriptableObject 기반 스테이지 관리
- 스테이지 목록
- 다음 스테이지 이동
- 클리어 상태 저장

### 5단계: 제작 보조 툴

- 상자 배치
- 숨겨진 규칙 설정
- 목표 보드 자동 생성
- 스테이지 저장

### 6단계: 자동 검증기

- 가능한 규칙 조합 완전탐색
- 목표와 일치하는 규칙 세트 개수 계산
- 가장 짧은 풀이 탐색
- 난이도 점수 계산

## 16. 구현 우선순위

1. 순수 C# 보드/규칙/시뮬레이터
2. 목표 보드 비교 Validator
3. 임시 스테이지 데이터 1개
4. Canvas 기반 3보드 UI
5. 색상별 규칙 선택 UI
6. 10개 스테이지 ScriptableObject화
7. 스테이지 클리어 흐름

## 17. MVP 검증 질문

- 최종 상태를 보고 규칙을 복원하는 행위가 재미있는가
- 규칙을 바꿀 때 결과 변화가 직관적인가
- 틀렸을 때 왜 틀렸는지 이해 가능한가
- 스테이지를 빠르게 만들 수 있는가
- 유일해가 아니어도 플레이어가 납득하는가

