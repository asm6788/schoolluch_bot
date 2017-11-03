# [@schoolluch_bot](http://telegram.me/schoollunch_bot)

급식충을 위한 봇을 만들어보자 해서... 만들게 됐습니다

프로그램 컴파일 전 해야 할 것
-------------
몽고 DB, 텔레그램 봇API Token 설정 필요합니다.
Program 클래스 있는...  
API:
```csharp
private static readonly TelegramBotClient Bot = new TelegramBotClient("사용할 API Token");
```
  
몽고DB:
```csharp
static MongoClient cli = new MongoClient("사용할 mongodb");
```
를 수정해주세요

현재 사용 가능 명령어
-------------
## /start

메인 페이지

## /오늘의급식
오늘의 급식을 불러옵니다. (학교정보 DB연동 가능)

## /내일의급식
내일의 급식을 불러옵니다. (학교정보 DB연동 가능)

## /특정한날의급식
특정한 날의 급식을 불러옵니다. (학교정보 DB연동 가능)

## /구독신청
새벽 12시. 원하는 시간에 급식을 자동으로 전송해드립니다

## /구독취소
구독을 취소합니다.

## /학교코드검색
급식을 불러올 때 필요한 학교코드(NEIS코드)를 검색합니다

## /학교등록
학교정보 DB로 사용자 학교를 저장하여 한번 입력하면 다시 입력할 필요가 없습니다.
