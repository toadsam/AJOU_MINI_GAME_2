# 아주분투 Unity 구현 메모

## 씬 구성

- `Boot`: 초기 진입 씬. 실행 즉시 `Menu`로 이동한다.
- `Menu`: 제목, 부제, 게임 시작, 조작 방법 패널을 런타임 생성한다.
- `Game`: 더블점프 기반 무한 러너 플레이 씬이다. 플레이어, 시작 플랫폼, 앞쪽 플랫폼, coffee 아이템은 씬에 실제 오브젝트로 배치되어 Inspector에서 직접 이동/수정할 수 있다.
- `GameOver`: 성공/실패 결과, 점수, 최고 점수, A+ 개수, 다시 시작/메인 메뉴 버튼을 표시한다.

`Assets/AjouBuntu/Scripts/Editor/AjouBuntuSceneBuilder.cs`가 `GameConfig`와 네 씬, Build Settings를 생성한다. Unity가 열려 있으면 컴파일 후 누락된 씬을 자동 생성하며, 수동으로는 `AjouBuntu > Build Scenes And Config` 메뉴를 실행하면 된다.

## 프리팹/눈에 보이는 편집 구조

프리팹은 `Assets/AjouBuntu/Prefabs`에 있다.

- `Player.prefab`: 플레이어 본체, `Rigidbody2D`, 몸통용 `BoxCollider2D`, 애니메이션/컨트롤러
- `Platform_StoneBridge.prefab`: 보이는 플랫폼과 `OneWayTopCollider` 자식 충돌체
- `Item_Coffee.prefab`: 현재 기본 스폰/배치 아이템
- `Item_APlus.prefab`: A+ 카운트 증가용 아이템

`Game` 씬의 `PlatformSpawner_VisibleEditable` 아래에 시작 플랫폼과 미리 배치된 플랫폼/아이템이 있다. `StartPlatform_Editable`의 상단 충돌 높이는 플레이어 발밑에 맞춰져 있어 시작 즉시 낙하하지 않는다.

플랫폼을 직접 조정할 때는 `PlatformController`의 `Width`, `Top Screen Y`를 기준으로 본다. `Top Screen Y`는 960x540 기준 화면 좌표 감성이라 값이 클수록 화면 아래쪽이다.

## 핵심 오브젝트

`RuntimeBootstrap`이 씬별로 필요한 오브젝트를 생성한다.

- `BackgroundManager`: 낮/노을/밤 배경 레이어와 거리 기반 페이드 전환
- `InputManager`: 마우스 클릭, 터치, 스페이스바 입력 통합
- `GameManager`: 속도 증가, 거리, 승리/패배 판정, 난이도 단계 계산
- `ScoreManager`: 초당 점수, 아이템 점수, PlayerPrefs 최고 점수 저장
- `PlayerController`: 코요테 타임과 더블점프, 낙사 판정용 위치 유지
- `PlayerAnimationController`: `Running`, `Jump`, `Fall`, `Landing`, `Hang` 상태
- `PlatformSpawner`: 시작 플랫폼과 이후 `stoneBridge` 플랫폼 생성
- `ItemController`: coffee 위주 아이템 수집, 점수 텍스트, 수집 이펙트
- `UIManager`: HUD, 거리 게이지, 난이도 상승 팝업
- `WireSystemStub`: `wireEnabled=false` 기본
- `ObstacleSystemStub`: `obstacleEnabled=false` 기본

## 기본 수치

`Assets/AjouBuntu/Resources/GameConfig.asset`에서 조정한다.

- 논리 해상도: `960 x 540`
- 시작 위치 감성: `x=170`, `y=402`
- 중력: `2050`
- 초기 속도: `350`
- 최대 속도: `780`
- 속도 증가: `initialSpeed + (elapsedMs / 900) * 10.5`
- 점프 속도: `-820` 입력값을 Unity y-up 기준으로 변환해 사용
- 목표 거리: `24000`
- 초당 점수: `12`
- 코요테 타임: `0.11`
- 공중 추가 점프: `1`
- 착지 상태: `0.11초`

## 에셋 연결

아래 파일명이 프로젝트 안에 있으면 생성기가 자동으로 `GameConfig`에 연결한다.

- 배경: `bg_campus_day.png`, `bg_campus_sunset.png`, `bg_campus_night.png`
- 플레이어: `chito-sprite-sheet.png`
- 플랫폼: `platform_stone_bridge.png`, `platform_stairs.png`, `platform_rooftop.png`, `platform_library_shelf.png`, `platform_festival_booth.png`, `platform_bus_stop.png`
- 아이템: `item_aplus.png`, `item_idcard.png`, `item_a.png`, `item_attendance.png`, `item_mealticket.png`, `item_coupon.png`, `item_sticker.png`

에셋이 없으면 런타임 폴백 스프라이트로 플레이 가능하다.

## 확장 지점

- 플랫폼 타입 추가: `PlatformKind`와 `GameConfig.platforms`에 정의를 추가한 뒤 `PlatformSpawner`의 선택 로직을 확장한다.
- 아이템 타입 추가: `ItemKind`, `GameConfig.items`, `PlatformSpawner.SpawnItem` 선택 로직을 확장한다.
- 장애물: `ObstacleKind`와 `ObstacleSystemStub`를 실제 스폰 시스템으로 교체한다.
- 와이어: `WireSystemStub`를 실제 발사/부착/스윙 시스템으로 확장하고 `GameConfig.wireEnabled`를 켠다.
