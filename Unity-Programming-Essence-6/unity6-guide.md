# 유니티 6 가이드
유니티 게임 프로그래밍 에센스 6 초판은 유니티 2023.2 버전을 기반으로 제작되었습니다.
<br>하지만 예제 프로젝트들이 유니티 6를 기준으로 업그레이드 됨에 따라 워크플로우가 달라졌습니다.

다음 변경사항들을 유의하고 책을 따라합니다.

## 새 3D 프로젝트 만들기
유니티 허브에서 기존 3D 템플릿의 이름이 3D (Built-In Render Pipeline)으로 변경되었습니다.
<br>그리고 새 프로젝트 생성 창을 열면 기본값으로 Universal 3D 템플릿이 자동 선택됩니다.

따라서 새 3D 프로젝트를 생성할 때...

* 기존 : 새로운 3D 프로젝트를 만들때 3D 템플릿을 선택
* 유니티 6 : **Universal 3D** 대신 **3D (Built-in Render Pipeline)** 템플릿을 찾아 선택<br>(선택 후 **Download Template** 버튼을 눌러 템플릿을 다운로드해야 할 수 있습니다)

![upgrade-image](doc-images/upgrade-image001.png)

## 프로젝트 창의 뷰 모드
유니티 2023.2까지는 프로젝트 창의 뷰 모드 기본값이 **Two Column Layout** 이었으나 유니티 6부터는 **One Column Layout**으로 변경되었습니다. 따라서 유니티 6에서 프로젝트를 처음 만들어 열면 프로젝트 창에 사이드바가 없고, 다음과 같이 계층 구조로 보이게 될것입니다.

<img src="doc-images/upgrade-image002.png" width="300"/>

만약 해당 뷰를 원하지 않고 책과 동일한 인터페이스로 진행하고 싶다면 다음과 같은 과정으로 Two Colum Layout으로 변경할 수 있습니다.

* 과정 : 프로젝트 창 우측 상단의 **컨텍스트 버튼** 클릭 > **Two Column Layout** 클릭

![alt text](doc-images/upgrade-image003.png)


## C# 스크립트 생성 버튼 위치

기본 MonoBehaviour C# 스크립트 생성 버튼 위치가 변경되었습니다.

* 기존 : + > **C# Script**
* 유니티 6 : + > **MonoBehaviour Script**

![alt text](doc-images/upgrade-image004.png)

## 빌드 설정

기존의 빌드 설정(**Build Settigns...**) 메뉴가 빌드 프로파일(**Bulid Profiles...**)로 변경되었습니다.

> 빌드 프로파일은 여러 빌드 설정을 일종의 빌드 프리셋인 프로파일로 여러개를 만들어 쓸 수 있는 기능입니다.

빌드 프로파일 기능을 통해 여러개의 빌드 프리셋을 만들 수 있는 것 이외에는 변경사항이 없으므로 책에서 빌드를 하는 부분을 다음과 같이 바꿔 따라하면 정상적으로 빌드를 진행할 수 있습니다.

### 빌드 설정 창 진입

* 기존 : File > **Build Settings...**
* 유니티 6 : File > **Build Profiles...**

![alt text](doc-images/upgrade-image005.png)

### 플레이어 설정(Player Settings) 버튼 위치

빌드 설정 창의 Player Settings 버튼 위치가 다음과 같이 변경되었습니다.

![alt text](doc-images/upgrade-image006.png)

### 씬 목록 창 편집 위치 변화

유니티 2023까지는 빌드 설정 창에서 빌드에 포함할 씬 목록을 바로 편집할 수 있었습니다.
유니티 6 부터는 **Platforms** 목록에서 **Scene List**을 클릭하여 씬 목록 편집 페이지로 진입합니다.

![alt text](doc-images/upgrade-image007.png)