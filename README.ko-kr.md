# WslManager

[English](README.md)

2026년 9월 5일 기준 WslManager 0.2 개발 버전은 .NET 10과 WPF를 사용해 Windows에서 WSL 배포판을 관리하는 화면을 제공합니다. 기존 .NET 5 및 Windows Forms 구현을 교체했습니다.

이번 버전에서 배포판 관리, 설정 저장, 아카이브 가져오기, 드라이브 연결과 디스크 마운트 기능을 다룹니다. Windows x64와 ARM64용 설치 프로그램 및 포터블 패키지도 생성합니다.

지원 환경과 기능을 먼저 살펴보겠습니다. 빌드 및 배포 절차를 설명한 뒤 설정 저장 방식과 검증 범위를 안내합니다. [릴리스 노트](docs/release-notes.md)에서 변경 사항과 남은 검증 항목을 확인할 수 있습니다.

개발 버전의 확인 범위를 다음과 같이 고지합니다.

> 2026년 9월 5일 기준으로 작성했습니다. Windows 화면 자동 테스트는 가짜 WSL 응답을 사용합니다. 실제 Windows Server Core와 RDP 조합, 물리 디스크 작업 및 Microsoft Store 게시 결과는 확인되지 않았습니다.

## Windows와 WSL 실행 조건

실행 환경의 조건을 정리하겠습니다. Windows x64 또는 ARM64 환경에 WSL을 설치하면 앱을 실행할 수 있습니다. 설치 프로그램은 Windows 빌드 19041 이상을 요구하며 실제 지원 범위에는 [.NET 10의 운영체제 지원 정책](https://github.com/dotnet/core/blob/main/release-notes/10.0/supported-os.md)도 적용합니다.

기본 사용자 변경 기능은 현재 WSL의 `--manage --set-default-user` 명령 지원 여부를 검사합니다. 디스크 마운트 기능은 최신 WSL과 해당 작업에 대한 관리자 승인을 요구합니다. 일반 배포판 관리는 현재 Windows 사용자 권한으로 실행합니다. 자세한 조건은 [WSL 기본 명령](https://learn.microsoft.com/windows/wsl/basic-commands)과 [디스크 마운트 문서](https://learn.microsoft.com/windows/wsl/wsl2-mount-disk)에서 설명합니다.

## 배포판 관리와 아카이브 가져오기

배포판을 선택하면 다음 기능을 사용할 수 있습니다. 명령의 동작과 데이터 삭제 범위는 [Microsoft WSL 문서](https://learn.microsoft.com/windows/wsl/basic-commands)를 기준으로 구현했습니다.

- 터미널 실행과 배포판 파일 열기
- 기존 Linux 사용자로 실행 및 셸 스크립트 실행
- 현재 WSL 배포판 목록을 이용한 설치
- `.tar`, `.tar.gz`, `.tgz` 아카이브 가져오기와 내보내기
- 기본 배포판 및 기본 사용자 지정
- 바로 가기 생성과 Linux 사용자 조회
- 네트워크 드라이브 연결 및 해제
- ext4 물리 디스크와 가상 디스크 마운트 및 해제
- 배포판 종료와 등록 해제

지원하는 아카이브 하나를 창에 끌어다 놓으면 가져오기 대화상자가 열립니다. 다른 로컬 드라이브의 빈 폴더나 새 폴더도 설치 위치로 지정할 수 있습니다. 앱은 가져오기 전에 출처 신뢰 여부를 묻고 설정에서 경고를 다시 표시하거나 생략할 수 있도록 제공합니다. 등록 해제 시에는 선택한 배포판 이름을 입력한 뒤 데이터를 삭제합니다.

## .NET 10 빌드와 회귀 테스트

빌드 절차를 안내하겠습니다. 안정 버전 .NET 10 SDK를 설치한 환경에서 저장소 루트를 기준으로 다음 명령을 실행합니다.

```sh
dotnet build src/WslManager.slnx -c Release
dotnet test tests/WslManager.Core.Tests -c Release
```

핵심 테스트는 Windows, Linux와 macOS에서 실행할 수 있습니다. WPF 앱은 Windows에서 실행합니다. [CI 워크플로](.github/workflows/build.yml)는 Windows 화면 조작과 패키지 생성도 검증합니다. [검증 문서](docs/validation.md)에 테스트별 범위를 기록했습니다.

## 설치 프로그램과 포터블 패키지

배포 경로에서는 패키지 생성과 외부 게시의 차이를 짚어보겠습니다. **Build and test** 워크플로는 CI 산출물을 제공하고 **Package release** 워크플로는 GitHub 릴리스 초안에 x64 및 ARM64 패키지를 첨부합니다. 공개 버전은 [Releases 페이지](https://github.com/wslhub/WslManager/releases)에서 확인할 수 있습니다.

생성하는 파일에는 사용자별 EXE 설치 프로그램, 포터블 ZIP, 서명하지 않은 MSIX와 SHA-256 해시 목록을 포함합니다. 자체 포함 패키지는 .NET 런타임을 함께 제공합니다. MSIX를 직접 설치하려면 해당 환경에서 신뢰하는 서명이 필요합니다. [배포 설정 문서](docs/distribution.md)에 설치 프로그램 생성과 Microsoft Store 자동 제출 구성을 설명했습니다.

다만 Microsoft Store 앱 등록, 인증 정보 연결, 인증 심사와 게시 결과는 확인되지 않았습니다. 자동 제출 워크플로의 존재만으로 Store 게시 완료를 의미하지 않습니다.

## 로컬 설정 저장과 목록 갱신

설정 저장 방식을 확인하겠습니다. 일반 설치에서는 `%LOCALAPPDATA%\WslManager\settings.json`에 설정을 저장합니다. 실행 파일 옆에 `portable.flag` 파일이 있으면 같은 디렉터리에 설정을 저장합니다. 앱은 임시 파일을 작성한 뒤 기존 설정 파일을 교체하며 잘못된 JSON 파일은 별도 백업으로 보존합니다.

저장 항목에는 창 위치와 크기, 최대화 상태, 열 순서와 너비, 정렬, 기본 터미널과 편집기, 가져오기 경고를 포함합니다. 사용자 지정 터미널의 인자 뒤에는 WSL 실행 파일과 명령 인자를 전달합니다. 편집기 인자 뒤에는 편집할 파일 경로를 전달합니다. 세미콜론이 포함된 스크립트는 Windows Terminal의 구분자 해석을 피하도록 시스템 콘솔에서 실행합니다.

이어서 목록 갱신 동작을 설명합니다. 앱은 작업 완료와 창 활성화 시 목록을 갱신하고 레지스트리 변경 알림을 묶어서 처리합니다. 실행 상태만 바뀌는 경우를 보완하기 위해 활성 창에서는 30초 간격으로 상태를 조회합니다. 조회에 실패하면 마지막으로 성공한 목록을 유지합니다. [핵심 구현](src/WslManager.Core)과 [검증 문서](docs/validation.md)에서 관련 동작을 다룹니다.

## 검증 범위와 남은 환경 확인

여기까지 정리하면 WslManager 0.2는 WPF 화면에서 WSL 명령과 로컬 설정을 관리하도록 구성했습니다. 핵심 로직과 Windows 화면 자동 검증 결과는 [검증 문서](docs/validation.md)에 기록합니다. 실제 배포판과 디스크를 변경하는 작업은 문서의 실기 점검 절차로 확인할 수 있습니다.

Windows Server Core와 RDP 환경 검증은 [이슈 #16](https://github.com/wslhub/WslManager/issues/16)에서 추적합니다. Store 자동 제출의 실제 동작은 [이슈 #5](https://github.com/wslhub/WslManager/issues/5)에서 추적합니다. 일반 Windows 환경에서 기능을 검토하려는 경우에는 포터블 패키지와 테스트용 배포판을 사용할 수 있습니다. Server Core 지원이나 Store 게시가 필요한 경우에는 해당 환경의 검증 결과를 기준으로 판단할 수 있습니다.

프로젝트는 [MIT 라이선스](License.txt)를 따릅니다. 한국어 README의 최초 기여는 [PR #30](https://github.com/wslhub/WslManager/pull/30)에서 제공했으며 이번 갱신에서도 작성자 커밋 이력을 보존했습니다. 기존 아이콘 출처는 [Icons8](https://www.icons8.com)와 [Penguin window by mimooh](https://commons.wikimedia.org/wiki/File:Penguin_window_by_mimooh.svg)입니다.
