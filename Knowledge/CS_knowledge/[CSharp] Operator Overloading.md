# [CSharp] Operator Overloading

- **Date**: 2026-06-30
- **Tags**: #CSharp #OperatorOverloading #Syntax #CSharpAdvanced
- **Related**: [[[CSharp] Struct Dictionary Boxing]]

> **TL;DR**: 연산자 오버로딩은 사용자 정의 타입(struct/class)에 `+`, `-`, `==` 등의 기본 연산자 동작을 재정의하여 직관적인 문법을 제공하는 기능이다. `operator` 키워드와 `public static` 반환타입 조합으로 선언하며, 일부 연산자는 쌍으로(==/!=, </> 등) 함께 구현해야 한다.

---

# 1. 기본 문법

---

### 1) 선언 형식

```csharp
public static 반환타입 operator 연산자(매개변수 목록)
```

- 반드시 `public static`으로 선언
- 매개변수 중 최소 하나는 선언한 타입이어야 함 (피연산자 타입 제약)
- `ref`, `in`, `out` 매개변수 불가

### 2) 단항 vs 이항 연산자

| 구분 | 예시 | 매개변수 개수 |
|------|------|:----------:|
| 단항 (Unary) | `+`, `-`, `!`, `~`, `++`, `--`, `true`, `false` | 1개 |
| 이항 (Binary) | `+`, `-`, `*`, `/`, `%`, `==`, `!=`, `<`, `>`, `<=`, `>=`, `&`, `\|`, `^`, `<<`, `>>` | 2개 |

```text
단항:   public static Vector3 operator -(Vector3 v)
이항:   public static Vector3 operator +(Vector3 a, Vector3 b)
```

---

# 2. 오버로딩 가능 / 불가능 연산자

---

### 1) 오버로딩 가능한 연산자

| 범주 | 연산자 |
|------|--------|
| 산술 | `+`, `-`, `*`, `/`, `%` |
| 비트 | `&`, `\|`, `^`, `~`, `<<`, `>>` |
| 비교 | `==`, `!=`, `<`, `>`, `<=`, `>=` |
| 논리 | `true`, `false` (커스텀 논리 평가용) |
| 증감 | `++`, `--` (전위/후위 둘 다 자동 적용) |
| 기타 | `!` |

### 2) 오버로딩 불가능한 연산자

| 연산자 | 이유 |
|--------|------|
| `=` (대입) | 문법 구조상 불가; `+=` 등 복합 대입은 대응되는 이항 연산자가 오버로딩되면 자동 지원 |
| `?:` (조건 삼항) | 언어 차원 제한 |
| `&&`, `\|\|` (조건부 논리) | 직접 오버로딩 불가; `&`, `\|`, `true`, `false`를 오버로딩하면 간접 구현 가능 |
| `[]` (인덱서) | 연산자 오버로딩이 아닌 indexer 별도 문법 사용 |
| `()` (호출) | delegate로 대체 |
| `->`, `.`, `new`, `is`, `as`, `typeof`, `sizeof`, `nameof` | 언어 키워드 |

### 3) 복합 대입 연산자 자동 지원

`+`를 오버로딩하면 `+=`도 자동으로 사용 가능. 단, `a += b`는 `a = a + b`로 컴파일러가 변환.

---

# 3. 연산자 쌍(Pair) 규칙

---

일부 연산자는 반드시 짝을 이루어 함께 구현해야 컴파일 오류가 발생하지 않습니다.

| 필수 쌍 | 비고 |
|---------|------|
| `==` 와 `!=` | `Equals` + `GetHashCode`도 함께 재정의 권장 |
| `<` 와 `>` | |
| `<=` 와 `>=` | |
| `true` 와 `false` | 드물게 사용; `Nullable<T>` 같은 타입에서 활용 |

```csharp
public static bool operator ==(MyType a, MyType b) => a.Equals(b);
public static bool operator !=(MyType a, MyType b) => !(a == b);

public override bool Equals(object obj) => ...;
public override int GetHashCode() => ...;
```

---

# 4. 주요 패턴별 예제

---

### 1) 산술 연산자 (struct Vector2 예시)

```csharp
public readonly struct Vector2
{
    public float X { get; }
    public float Y { get; }

    public Vector2(float x, float y) => (X, Y) = (x, y);

    public static Vector2 operator +(Vector2 a, Vector2 b)
        => new Vector2(a.X + b.X, a.Y + b.Y);

    public static Vector2 operator -(Vector2 a, Vector2 b)
        => new Vector2(a.X - b.X, a.Y - b.Y);

    public static Vector2 operator *(Vector2 v, float scalar)
        => new Vector2(v.X * scalar, v.Y * scalar);

    public static Vector2 operator *(float scalar, Vector2 v)
        => v * scalar; // 교환 법칙 지원
}
```

### 2) 비교 연산자 + Equals

```csharp
public readonly struct Fraction : IEquatable<Fraction>
{
    public int Numerator { get; }
    public int Denominator { get; }

    public Fraction(int num, int den)
    {
        if (den == 0) throw new ArgumentException("Denominator cannot be zero.");
        (Numerator, Denominator) = (num, den);
    }

    public static bool operator ==(Fraction a, Fraction b)
        => a.Numerator * b.Denominator == b.Numerator * a.Denominator;

    public static bool operator !=(Fraction a, Fraction b) => !(a == b);

    public override bool Equals(object obj) => obj is Fraction f && Equals(f);
    public bool Equals(Fraction other) => this == other;

    public override int GetHashCode() => HashCode.Combine(Numerator, Denominator);
}
```

### 3) true/false 오버로딩 (3-state logic)

```csharp
public struct NullableBool
{
    private readonly sbyte _value; // -1: null, 0: false, 1: true

    private NullableBool(sbyte value) => _value = value;

    public static NullableBool True    => new NullableBool(1);
    public static NullableBool False   => new NullableBool(0);
    public static NullableBool Null    => new NullableBool(-1);

    public static bool operator true(NullableBool val)  => val._value == 1;
    public static bool operator false(NullableBool val) => val._value == 0;
}
```

### 4) 암시적/명시적 변환 연산자

```csharp
public readonly struct Angle
{
    public double Radians { get; }

    public Angle(double radians) => Radians = radians;

    // 명시적 변환: double -> Angle
    public static explicit operator Angle(double radians)
        => new Angle(radians);

    // 암시적 변환: Angle -> double (안전하므로)
    public static implicit operator double(Angle angle)
        => angle.Radians;
}
```

변환 연산자 규칙:

| 키워드 | 의미 | 사용 조건 |
|--------|------|-----------|
| `implicit` | 암시적 변환 (캐스트 없이 대입 가능) | 데이터 손실이 없음이 보장될 때 |
| `explicit` | 명시적 변환 (`(T)value` 캐스트 필요) | 데이터 손실 가능성이 있을 때 |

```text
Angle a = (Angle)3.14;   // explicit: 캐스트 필요
double d = a;            // implicit: 캐스트 불필요
```

---

# 5. 연산자 오버로딩 vs 메서드 호출

---

```text
연산자 방식:  Vector3 result = v1 + v2;
메서드 방식:  Vector3 result = Vector3.Add(v1, v2);
```

연산자 오버로딩은 **가독성**을 높이는 문법적 설탕(syntactic sugar)이다. 내부 동작은 정적 메서드와 동일하지만, 피연산자 사이의 연산을 직관적으로 표현할 수 있다는 장점이 있다.

---

# 6. 주의사항 및 Best Practices

---

- **의미를 벗어난 오버로딩 금지**: `+`로 두 객체를 '빼는' 동작을 하는 등 직관에 반하는 구현은 피할 것
- **불변 타입(struct) 권장**: 연산자는 대개 새 값을 반환하므로, readonly struct가 적합
- **null 체크 필수** (class인 경우): `==` 오버로딩 시 `ReferenceEquals`와 `operator ==`의 무한 재귀에 주의

```csharp
// 무한 재귀 위험 (잘못된 예)
public static bool operator ==(MyClass a, MyClass b)
    => a == b;  // 자기 자신 호출 → StackOverflow

// 올바른 예
public static bool operator ==(MyClass a, MyClass b)
{
    if (ReferenceEquals(a, b)) return true;
    if (a is null || b is null) return false;
    return a.X == b.X;
}
```

- **Equals/GetHashCode 동반 재정의**: `==`를 오버로딩하면 `Dictionary`나 `HashSet`에서의 동등 비교 일관성을 위해 반드시 함께 재정의
- **연산자 쌍 누락 금지**: `==`만 정의하고 `!=`를 빼면 컴파일 에러
