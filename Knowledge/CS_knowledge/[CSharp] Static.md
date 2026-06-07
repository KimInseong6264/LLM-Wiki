# [CSharp] Static

- **Date**: 2026-06-07
- **Tags**: #CSharp #Static #MemoryManagement #ProgrammingConcepts
- **Related**: [[ [CS] Memory Structure ]]

---

# 1. static 필드(매개변수)

---

### 1) 개념
- 프로그램 실행 시, static 메모리 공간을 미리 할당하여 메모리에 적재한다.
- 클래스의 필드들이 동일한 값(정적 필드)을 공유하는 공유 필드로 만들 수 있다.
- 동일한 설계도(class) 내에서 같은 값을 공유한다.
    - 예: 게임 속 데스 카운트, 메이플스토리 유니온, 계정 속 공유 화폐
- static은 Heap과 별개인 static 공간(정적 공간)에 생성된다.
- 인스턴스에 종속되지 않으며, 클래스 이름을 통해 직접 접근할 수 있다.

### 2) 특징 및 장점
- 편하게 꺼내 쓸 수 있다 (예: `Random`).
- 객체를 새로 생성(인스턴스화)하지 않고도 값을 도출할 수 있다 (예: `Console.WriteLine`).

# 2. 동작 원리 및 시각화

---

### 1) 메모리 구조 시각화
static 영역은 클래스 로드 시점에 메모리에 적재되며, 인스턴스(Heap)와는 독립적인 생명주기를 가집니다.

```text
+-----------------------------------+        +---------------------------+
|        Static Memory Area         |        |     Heap Memory Area      |
|-----------------------------------|        |---------------------------|
| [static createCount] = 3          | <----  | Instance (Enemy1)         |
|                                   | <----  | Instance (Enemy2)         |
|                                   | <----  | Instance (Enemy3)         |
+-----------------------------------+        +---------------------------+
```

### 2) 데이터 공유 관계
여러 인스턴스가 생성되어도 static 변수는 단 하나만 존재합니다.

```text
[Class Enemy]
     |
     +-- static int createCount (공유 자원)

Enemy A +-----> [createCount : 3]
Enemy B +-----> [createCount : 3]
Enemy C +-----> [createCount : 3]
```

각 인스턴스에서 `createCount`를 변경하면, 해당 필드가 static 공간에 단 하나만 존재하므로 다른 모든 인스턴스에서도 변경된 값을 공유받게 됩니다.