# 아주대학교 축제 미니게임 프로젝트 가이드

## 실행 시작점

Build Settings 첫 씬은 `Assets/Scenes/00_MainMenu.unity`입니다.

씬 구성:

- `00_MainMenu.unity`: 메인 메뉴
- `01_GameSelect.unity`: 게임 선택
- `02_AjouBoontu.unity`: 아주분투
- `03_BalanceWalk.unity`: 치토 균형걷기
- `04_OneVsOneSoccer.unity`: 아주 1대1 축구
- `05_Result.unity`: 결과 화면

## 자동 생성 메뉴

Unity 상단 메뉴에서 아래 항목을 실행할 수 있습니다.

- `Tools/Ajou Festival/Create MiniGame Project Structure`
- `Tools/Ajou Festival/Create Placeholder Prefabs`
- `Tools/Ajou Festival/Create Basic Scenes`

`Create Basic Scenes`를 실행하면 폴더, placeholder sprite, prefab, 6개 씬, Build Settings를 다시 생성합니다.

## 주요 폴더

- `Assets/Scripts/Core`: 씬 이동, 세션, 최고 점수, 공통 입력
- `Assets/Scripts/UI`: 메인 메뉴, 게임 선택, 결과 UI
- `Assets/Scripts/Games/AjouBoontu`: 러너 게임
- `Assets/Scripts/Games/BalanceWalk`: 균형걷기 게임
- `Assets/Scripts/Games/Soccer`: 2인 축구 게임
- `Assets/Prefabs`: Unity Editor에서 직접 수정 가능한 프리팹
- `Assets/Sprites`: 나중에 교체할 이미지 자리

## 이미지 교체 방법

각 prefab의 루트 또는 자식 `SpriteRenderer`에서 Sprite를 교체하면 됩니다.

추천 교체 위치:

- `Prefabs/Games/AjouBoontu/ChitoRunner.prefab`
- `Prefabs/Games/AjouBoontu/Platform_Default.prefab`
- `Prefabs/Games/AjouBoontu/Item_APlus.prefab`
- `Prefabs/Games/AjouBoontu/Obstacle_Default.prefab`
- `Prefabs/Games/BalanceWalk/BalancePlayer.prefab`
- `Prefabs/Games/Soccer/SoccerPlayer1.prefab`
- `Prefabs/Games/Soccer/SoccerPlayer2.prefab`
- `Prefabs/Games/Soccer/SoccerBall.prefab`

## Inspector 조정 포인트

아주분투:

- `ChitoRunnerController`: `runSpeed`, `jumpForce`, `fallDeathY`
- `WireActionController`: `maxWireDuration`, `wireMinAirTime`, `wirePullForce`, `wireMaxFallSpeed`
- `RunnerPlatformSpawner`: `platformPrefabs`, `minGap`, `maxGap`, `yRange`
- `RunnerItemSpawner`: `itemPrefabs`, `spawnInterval`, `yRange`
- `RunnerObstacleSpawner`: `obstaclePrefabs`, `baseSpawnChance`, `spawnInterval`

치토 균형걷기:

- `BalanceWalkGameManager`: `countdownDuration`
- `BalanceWalkGameManager`: `scorePerMeter`
- `BalancePlayerController`: `balanceTorque`, `randomTiltForce`, `maxSafeAngle`, `difficultyIncreaseRate`, `moveSpeed`, `speedIncreaseRate`, `useAutoMove`
- `BalanceCameraController`: `offset`, `followSpeed`, `followXOnly`
- `BalanceGroundLoop`: `tileWidth`, `tileCount`, `recycleBehind`
- `BalanceDistanceCueLoop`: `spacing`, `cueCount`, `recycleBehind`
- `BalanceParallaxLoop`: `parallaxFactor`, `tileWidth`, `tileCount`
- `BalanceUI`: `timeText`, `bestText`, `hintText`, `countdownText`

축구:

- `SoccerPlayerController`: `playerIndex`, `moveSpeed`, `kickForce`, `kickRange`
- `SoccerBallController`: `maxSpeed`, `linearDamping`
- `SoccerGoal`: `scoringPlayer`
- `SoccerGameManager`: `matchDuration`

## 조작법

공통:

- `ESC`: 게임 선택으로
- `R`: 현재 게임 다시 시작

아주분투:

- `Space` 또는 마우스 클릭: 점프
- 공중에서 `Space` 또는 마우스 길게 누르기: 와이어

치토 균형걷기:

- `A/D` 또는 `←/→`: 균형 조절
- 시작 시 `3, 2, 1, Start!` 카운트다운 후 이동과 넘어짐 판정이 시작됩니다.
- 치토는 자동으로 오른쪽으로 계속 전진하고, 카메라는 치토를 따라갑니다.
- 점수와 최고 기록은 생존 시간이 아니라 시작 지점부터 이동한 거리 기준입니다.
- 바닥 대시, 거리 표지판, 캠퍼스 실루엣이 지나가면서 전진감을 만듭니다.

축구:

- Player 1: `WASD` 이동, `Space` 슛
- Player 2: 방향키 이동, `Enter` 또는 `RightControl` 슛

## 테스트 체크리스트

- MainMenu에서 GameSelect로 이동되는가?
- GameSelect에서 아주분투 씬으로 이동되는가?
- GameSelect에서 균형걷기 씬으로 이동되는가?
- GameSelect에서 축구 씬으로 이동되는가?
- 균형걷기가 3초 카운트다운 후 시작되는가?
- 균형걷기에서 치토가 앞으로 이동하고 카메라가 따라가는가?
- 각 게임에서 게임오버/시간 종료 후 Result 씬으로 이동되는가?
- Result 씬에서 다시하기가 되는가?
- Result 씬에서 게임 선택으로 돌아갈 수 있는가?
- 최고 점수가 PlayerPrefs에 저장되는가?
- ESC로 게임 선택 화면으로 돌아갈 수 있는가?
- R로 현재 게임을 다시 시작할 수 있는가?
