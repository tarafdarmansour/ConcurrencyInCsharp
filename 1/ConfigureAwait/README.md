# ConfigureAwait در ASP.NET Core - اهمیت استفاده صحیح

این پروژه یک نمونه عملی از اهمیت استفاده از `ConfigureAwait(false)` در برنامه‌های ASP.NET Core است.

## چرا ConfigureAwait مهم است؟

در ASP.NET Core، هر درخواست HTTP روی یک thread از thread pool اجرا می‌شود. وقتی از `await` استفاده می‌کنیم:

### بدون ConfigureAwait(false):
- Continuation (کد بعد از await) سعی می‌کند روی همان thread اصلی (ASP.NET context) اجرا شود
- اگر thread اصلی مشغول باشد، continuation باید منتظر بماند
- این می‌تواند منجر به **thread pool starvation** شود
- در بار سنگین، برنامه نمی‌تواند درخواست‌های جدید را پردازش کند

### با ConfigureAwait(false):
- Continuation می‌تواند روی هر thread آزاد از thread pool اجرا شود
- کارایی بهتر و استفاده بهینه از منابع
- جلوگیری از deadlock در برخی سناریوها

## API Endpoints

### ۱. مقایسه پایه
```http
GET /configureawait/with
GET /configureawait/without
```

این endpoints تفاوت thread IDها را قبل و بعد از await نشان می‌دهند.

### ۲. شبیه‌سازی عملیات I/O
```http
GET /configureawait/simulation/good
GET /configureawait/simulation/bad
```

شبیه‌سازی ۱۰ عملیات I/O همزمان با و بدون ConfigureAwait.

### ۳. Load Testing
```http
GET /configureawait/loadtest/{count}
```

شبیه‌سازی تعداد زیادی عملیات همزمان (۱ تا ۱۰۰۰).

## مثال خروجی

### با ConfigureAwait(false):
```json
{
  "initialThreadId": 5,
  "continuationThreadId": 8,
  "usedConfigureAwait": true,
  "message": "This continuation can run on any thread pool thread"
}
```

### بدون ConfigureAwait:
```json
{
  "initialThreadId": 5,
  "continuationThreadId": 5,
  "message": "This continuation tries to resume on the original ASP.NET context thread"
}
```

## بهترین Practices

### ✅ استفاده صحیح:
```csharp
public async Task<IActionResult> MyAction()
{
    // I/O bound operation
    var data = await _service.GetDataAsync().ConfigureAwait(false);

    // CPU bound operation
    var result = await Task.Run(() => ProcessData(data)).ConfigureAwait(false);

    return Ok(result);
}
```

### ❌ استفاده نادرست:
```csharp
public async Task<IActionResult> MyAction()
{
    // بدون ConfigureAwait - بد!
    var data = await _service.GetDataAsync();

    return Ok(data);
}
```

## نکات مهم

۱. **همیشه از ConfigureAwait(false) استفاده کنید** مگر اینکه نیاز به context داشته باشید
۲. **در library codeها همیشه ConfigureAwait(false) استفاده کنید**
۳. **در ASP.NET Core MVC controllers، همیشه ConfigureAwait(false) استفاده کنید**
۴. **تنها در UI applications (WPF, WinForms) ممکن است نیاز به context داشته باشید**

## اجرای پروژه

```bash
cd ConfigureAwaitDemo
dotnet run
```

سپس به `https://localhost:5001/swagger` بروید تا API documentation را ببینید.

## آزمایش عملی

۱. برنامه را اجرا کنید
۲. از Swagger UI استفاده کنید
۳. ابتدا endpointهای ساده را تست کنید
۴. سپس loadtest را با تعداد بالا اجرا کنید
۵. رفتار threadها را مشاهده کنید

## نتیجه‌گیری

استفاده صحیح از `ConfigureAwait(false)` برای عملکرد بهتر برنامه‌های ASP.NET Core ضروری است. این کار به جلوگیری از thread pool starvation و بهبود scalability کمک می‌کند.
