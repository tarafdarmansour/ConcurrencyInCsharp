# ConfigureAwait در WinForms - اهمیت استفاده صحیح

این پروژه یک نمونه عملی از اهمیت استفاده از `ConfigureAwait(false)` در برنامه‌های WinForms است.

## تفاوت WinForms با ASP.NET Core

در WinForms، وضعیت کمی متفاوت از ASP.NET Core است:

### ✅ در UI Code (Event Handlers):
- می‌توانید بدون `ConfigureAwait(false)` استفاده کنید
- چون نیاز به UI context دارید
- اما برای **library code** همیشه از `ConfigureAwait(false)` استفاده کنید

### ❌ در Library Code:
- **همیشه** از `ConfigureAwait(false)` استفاده کنید
- چون library نمی‌داند در چه context‌ای استفاده می‌شود
- این باعث می‌شود library در هر جا قابل استفاده باشد

## مثال‌های موجود در پروژه

### ۱. ✅ با ConfigureAwait(false)
- نشان می‌دهد که continuation می‌تواند روی هر thread اجرا شود
- برای دسترسی به UI باید از `Invoke` استفاده کنیم
- بهتر برای performance و جلوگیری از thread pool starvation

### ۲. ❌ بدون ConfigureAwait
- Continuation سعی می‌کند روی UI thread برگردد
- در WinForms برای UI code OK است
- اما برای library code بد است

### ۳. ⚠️ تست Deadlock
- نشان می‌دهد که چگونه `ConfigureAwait(false)` از deadlock جلوگیری می‌کند
- وقتی از synchronous context یک async method صدا زده می‌شود
- بدون `ConfigureAwait(false)` ممکن است deadlock رخ دهد

### ۴. 📊 مقایسه عملکرد
- مقایسه زمان اجرا با و بدون `ConfigureAwait(false)`
- نشان می‌دهد که `ConfigureAwait(false)` معمولاً سریع‌تر است

### ۵. 📚 مثال Library Code
- نشان می‌دهد که library code باید همیشه `ConfigureAwait(false)` استفاده کند
- این باعث می‌شود library در هر context‌ای قابل استفاده باشد

## بهترین Practices برای WinForms

### ✅ UI Event Handlers:
```csharp
private async void Button_Click(object sender, EventArgs e)
{
    // می‌توانید بدون ConfigureAwait استفاده کنید
    var data = await GetDataAsync();
    
    // برای دسترسی به UI از Invoke استفاده نکنید (نیازی نیست)
    textBox.Text = data; // این OK است چون روی UI thread هستیم
}
```

### ✅ Library Code:
```csharp
public static class MyLibrary
{
    public static async Task<string> GetDataAsync()
    {
        // همیشه ConfigureAwait(false) استفاده کنید
        var data = await FetchDataAsync().ConfigureAwait(false);
        var processed = await ProcessDataAsync(data).ConfigureAwait(false);
        return processed;
    }
}
```

### ✅ UI Code که از Library استفاده می‌کند:
```csharp
private async void Button_Click(object sender, EventArgs e)
{
    // Library method را فراخوانی کنید
    var result = await MyLibrary.GetDataAsync(); // Library از ConfigureAwait استفاده می‌کند
    
    // برای دسترسی به UI، اگر از ConfigureAwait(false) استفاده کردید:
    if (InvokeRequired)
    {
        Invoke(new Action(() => textBox.Text = result));
    }
    else
    {
        textBox.Text = result;
    }
}
```

## نکات مهم

### ۱. Deadlock Prevention
```csharp
// ❌ بد - ممکن است deadlock کند
public string GetData()
{
    return GetDataAsync().Result; // Deadlock!
}

// ✅ خوب - از ConfigureAwait(false) استفاده می‌کند
public string GetData()
{
    return GetDataAsync().GetAwaiter().GetResult();
}

// یا بهتر است:
public async Task<string> GetDataAsync()
{
    await SomeOperation().ConfigureAwait(false); // Safe
}
```

### ۲. Invoke برای دسترسی به UI
```csharp
private async void Button_Click(object sender, EventArgs e)
{
    // اگر از ConfigureAwait(false) استفاده کردید:
    await SomeAsyncOperation().ConfigureAwait(false);
    
    // برای دسترسی به UI باید Invoke استفاده کنید:
    if (InvokeRequired)
    {
        Invoke(new Action(() => {
            textBox.Text = "Updated";
        }));
    }
}
```

### ۳. Performance
- استفاده از `ConfigureAwait(false)` باعث می‌شود thread pool بهتر استفاده شود
- از thread pool starvation جلوگیری می‌کند
- در library code همیشه استفاده کنید

## اجرای پروژه

```bash
cd ConfigureAwaitDemo/ConfigureAwaitWinForms
dotnet run
```

یا از Visual Studio:
1. پروژه را باز کنید
2. F5 را فشار دهید

## آزمایش

1. هر دکمه را کلیک کنید
2. خروجی را در text box مشاهده کنید
3. Thread IDها را بررسی کنید
4. تفاوت‌های عملکرد را مشاهده کنید

## نتیجه‌گیری

- **در UI code (WinForms):** می‌توانید بدون ConfigureAwait استفاده کنید
- **در Library code:** همیشه ConfigureAwait(false) استفاده کنید
- **برای Performance:** ConfigureAwait(false) معمولاً بهتر است
- **برای Deadlock Prevention:** ConfigureAwait(false) ضروری است

این پروژه به صورت عملی نشان می‌دهد که چگونه ConfigureAwait(false) در WinForms استفاده می‌شود و چرا مهم است.
