namespace ConfigureAwaitWinForms;

public partial class Form1 : Form
{
    public Form1()
    {
        InitializeComponent();
        UpdateThreadInfo();
    }

    private void UpdateThreadInfo()
    {
        if (InvokeRequired)
        {
            Invoke(new Action(UpdateThreadInfo));
            return;
        }
        lblThreadInfo.Text = $"UI Thread ID: {Environment.CurrentManagedThreadId} | " +
                           $"Thread Pool Threads: {ThreadPool.ThreadCount}";
    }

    private void Log(string message)
    {
        if (InvokeRequired)
        {
            Invoke(new Action<string>(Log), message);
            return;
        }
        txtOutput.AppendText($"[{DateTime.Now:HH:mm:ss.fff}] {message}{Environment.NewLine}");
        txtOutput.ScrollToCaret();
    }

    // ✅ مثال ۱: استفاده صحیح از ConfigureAwait(false)
    private async void BtnTestWithConfigureAwait_Click(object? sender, EventArgs e)
    {
        Log("=== ✅ تست با ConfigureAwait(false) ===");
        var initialThreadId = Environment.CurrentManagedThreadId;
        Log($"Thread اولیه (UI): {initialThreadId}");

        // شبیه‌سازی عملیات async با ConfigureAwait(false)
        await SimulateAsyncOperationWithConfigureAwait().ConfigureAwait(false);

        var continuationThreadId = Environment.CurrentManagedThreadId;
        Log($"Thread بعد از await (continuation): {continuationThreadId}");
        Log($"نتیجه: Continuation می‌تواند روی هر thread اجرا شود (بهتر برای performance)");
        Log($"اما برای دسترسی به UI باید Invoke استفاده کنیم");
        Log("");

        // برای دسترسی به UI از Invoke استفاده می‌کنیم
        if (InvokeRequired)
        {
            Invoke(new Action(() => UpdateThreadInfo()));
        }
        else
        {
            UpdateThreadInfo();
        }
    }

    // ❌ مثال ۲: بدون ConfigureAwait (بدتر برای library code)
    private async void BtnTestWithoutConfigureAwait_Click(object? sender, EventArgs e)
    {
        Log("=== ❌ تست بدون ConfigureAwait ===");
        var initialThreadId = Environment.CurrentManagedThreadId;
        Log($"Thread اولیه (UI): {initialThreadId}");

        // بدون ConfigureAwait - context را capture می‌کند
        await SimulateAsyncOperationWithoutConfigureAwait();

        var continuationThreadId = Environment.CurrentManagedThreadId;
        Log($"Thread بعد از await (continuation): {continuationThreadId}");
        Log($"نتیجه: Continuation سعی می‌کند روی UI thread برگردد");
        Log($"این می‌تواند باعث thread pool starvation شود");
        Log($"در WinForms برای UI code OK است، اما برای library code بد است");
        Log("");
        UpdateThreadInfo();
    }

    // ⚠️ مثال ۳: Deadlock واقعی با synchronous method
    private void BtnTestDeadlock_Click(object? sender, EventArgs e)
    {
        Log("=== ⚠️ تست Deadlock با Synchronous Method ===");
        Log("این مثال نشان می‌دهد که چگونه از deadlock جلوگیری کنیم");
        Log("");

        Log("=== ❌ Unsafe Version (بدون ConfigureAwait) ===");
        Log("⚠️ هشدار: این تست deadlock می‌کند!");
        Log("شروع تست...");

        // این یک synchronous method است که async method را با .Result صدا می‌زند
        // این باعث deadlock می‌شود اگر async method بدون ConfigureAwait(false) باشد
        try
        {
            // این خط deadlock می‌کند چون:
            // 1. UI thread منتظر GetDataUnsafeAsync().Result است
            // 2. GetDataUnsafeAsync() بدون ConfigureAwait است، پس می‌خواهد روی UI thread برگردد
            // 3. اما UI thread منتظر Result است و block شده
            // 4. پس deadlock!
            
            var result = GetDataUnsafeSync(); // این deadlock می‌کند!
            Log($"✅ نتیجه: {result}"); // این خط هرگز اجرا نمی‌شود
        }
        catch (Exception ex)
        {
            Log($"❌ خطا: {ex.Message}");
        }

        Log("");
        Log("=== ✅ Safe Version (با ConfigureAwait) ===");
        try
        {
            // این version deadlock نمی‌کند چون ConfigureAwait(false) استفاده می‌کند
            Log("شروع تست safe version...");
            var result = GetDataSafeSync(); // این deadlock نمی‌کند
            Log($"✅ نتیجه: {result}");
            Log("✅ این version کار می‌کند چون ConfigureAwait(false) استفاده می‌کند");
            Log("✅ Continuation روی thread دیگری اجرا می‌شود و UI thread آزاد می‌ماند");
        }
        catch (Exception ex)
        {
            Log($"❌ خطا: {ex.Message}");
        }
        Log("");
    }

    // 📊 مثال ۴: مقایسه عملکرد
    private async void BtnTestPerformance_Click(object? sender, EventArgs e)
    {
        Log("=== 📊 مقایسه عملکرد ===");
        const int iterations = 200;

        // Test با ConfigureAwait(false)
        var sw1 = System.Diagnostics.Stopwatch.StartNew();
        var tasks1 = new List<Task>();
        for (int i = 0; i < iterations; i++)
        {
            tasks1.Add(SimulateAsyncOperationWithConfigureAwait());
        }
        await Task.WhenAll(tasks1);
        sw1.Stop();

        // Test بدون ConfigureAwait
        var sw2 = System.Diagnostics.Stopwatch.StartNew();
        var tasks2 = new List<Task>();
        for (int i = 0; i < iterations; i++)
        {
            tasks2.Add(SimulateAsyncOperationWithoutConfigureAwait());
        }
        await Task.WhenAll(tasks2);
        sw2.Stop();

        Log($"تعداد تکرار: {iterations}");
        Log($"✅ با ConfigureAwait(false): {sw1.ElapsedMilliseconds} ms");
        Log($"❌ بدون ConfigureAwait: {sw2.ElapsedMilliseconds} ms");
        var difference = sw2.ElapsedMilliseconds - sw1.ElapsedMilliseconds;
        var percentage = sw1.ElapsedMilliseconds > 0 
            ? (difference * 100.0 / sw1.ElapsedMilliseconds) 
            : 0;
        Log($"تفاوت: {difference} ms ({percentage:F1}%)");
        Log("");
    }

    // 📚 مثال ۵: Library Code مثال
    private async void BtnTestLibraryCode_Click(object? sender, EventArgs e)
    {
        Log("=== 📚 مثال Library Code ===");
        Log("در library code همیشه باید ConfigureAwait(false) استفاده شود");
        Log("چون library نمی‌داند که در چه context‌ای استفاده می‌شود");

        try
        {
            // مثال: فراخوانی یک library method
            var result = await LibraryHelper.GetDataAsync();
            Log($"✅ نتیجه از library: {result}");
            Log($"Library method از ConfigureAwait(false) استفاده می‌کند");
            Log($"این باعث می‌شود library در هر context‌ای قابل استفاده باشد");
        }
        catch (Exception ex)
        {
            Log($"❌ خطا: {ex.Message}");
        }
        Log("");
    }

    private void BtnClear_Click(object? sender, EventArgs e)
    {
        txtOutput.Clear();
        UpdateThreadInfo();
    }

    // 🔴 مثال کلاسیک Deadlock - این واقعاً deadlock می‌کند!
    // این مثال در WinForms واقعاً deadlock می‌کند چون:
    // 1. از UI thread یک async method را با .Result صدا می‌زنیم
    // 2. UI thread block می‌شود و منتظر Result می‌ماند
    // 3. async method بدون ConfigureAwait می‌خواهد continuation را روی UI thread اجرا کند
    // 4. اما UI thread block شده و منتظر Result است
    // 5. پس deadlock!
    private void BtnTestDeadlockClassic_Click(object? sender, EventArgs e)
    {
        Log("=== 🔴 Deadlock کلاسیک در WinForms ===");
        Log("⚠️ هشدار: این تست واقعاً deadlock می‌کند!");
        Log("برنامه hang می‌شود و باید Force Close کنید");
        Log("");
        Log("سناریو:");
        Log("1. UI thread منتظر GetDataUnsafeAsync().Result است (block شده)");
        Log("2. GetDataUnsafeAsync() بدون ConfigureAwait است");
        Log("3. پس می‌خواهد continuation را روی UI thread اجرا کند");
        Log("4. اما UI thread block شده و منتظر Result است");
        Log("5. پس deadlock!");
        Log("");
        Log("شروع تست...");

        try
        {
            // این deadlock می‌کند چون از UI thread با .Result صدا می‌زنیم
            // و async method بدون ConfigureAwait می‌خواهد به UI thread برگردد
            var result = GetDataUnsafeAsync().Result; // ❌ Deadlock!
            Log($"✅ نتیجه: {result}"); // این خط هرگز اجرا نمی‌شود
        }
        catch (Exception ex)
        {
            // این exception هم ممکن است هرگز نیاید
            Log($"❌ خطا: {ex.Message}");
        }
    }

    // Helper methods
    private async Task SimulateAsyncOperationWithConfigureAwait()
    {
        // شبیه‌سازی عملیات I/O
        await Task.Delay(100).ConfigureAwait(false);
        
        // شبیه‌سازی CPU work
        await Task.Run(() => Thread.Sleep(50)).ConfigureAwait(false);
    }

    private async Task SimulateAsyncOperationWithoutConfigureAwait()
    {
        // شبیه‌سازی عملیات I/O (بدون ConfigureAwait)
        await Task.Delay(100);
        
        // شبیه‌سازی CPU work
        await Task.Run(() => Thread.Sleep(50));
    }

    private async Task<string> GetDataAsyncSafe()
    {
        // Safe version - استفاده از ConfigureAwait(false)
        await Task.Delay(50).ConfigureAwait(false);
        return "Data (Safe)";
    }

    private async Task<string> GetDataAsyncUnsafe()
    {
        // Unsafe version - بدون ConfigureAwait
        // اگر از synchronous context صدا زده شود، ممکن است deadlock کند
        await Task.Delay(50);
        return "Data (Unsafe)";
    }

    // ⚠️ این method deadlock می‌کند!
    // این یک synchronous method است که async method را با .Result صدا می‌زند
    private string GetDataUnsafeSync()
    {
        // ❌ این deadlock می‌کند!
        // چون:
        // 1. UI thread منتظر Result است (block شده)
        // 2. GetDataUnsafeAsync() بدون ConfigureAwait است
        // 3. پس می‌خواهد continuation را روی UI thread اجرا کند
        // 4. اما UI thread block شده و منتظر Result است
        // 5. پس deadlock!
        return GetDataUnsafeAsync().Result; // ❌ Deadlock!
    }

    // ✅ این method deadlock نمی‌کند
    // چون async method از ConfigureAwait(false) استفاده می‌کند
    private string GetDataSafeSync()
    {
        // ✅ این deadlock نمی‌کند
        // چون GetDataSafeAsync() از ConfigureAwait(false) استفاده می‌کند
        // پس continuation روی thread دیگری اجرا می‌شود
        // و UI thread آزاد می‌ماند
        return GetDataSafeAsync().Result; // ✅ Safe
    }

    private async Task<string> GetDataSafeAsync()
    {
        // ✅ Safe: استفاده از ConfigureAwait(false)
        // این باعث می‌شود continuation روی thread دیگری اجرا شود
        // و UI thread آزاد بماند
        await Task.Delay(100).ConfigureAwait(false);
        return "Data (Safe Sync)";
    }

    private async Task<string> GetDataUnsafeAsync()
    {
        // ❌ Unsafe: بدون ConfigureAwait
        // این می‌خواهد continuation را روی UI thread اجرا کند
        // اما اگر از .Result صدا زده شود، UI thread block است
        // پس deadlock!
        await Task.Delay(100); // ❌ بدون ConfigureAwait
        return "Data (Unsafe Sync)";
    }
}

// ⚠️ نکته مهم درباره Deadlock در WinForms:
// 
// در WinForms، deadlock معمولاً زمانی رخ می‌دهد که:
// 1. یک synchronous method (از UI thread) async method را با .Result یا .Wait() صدا می‌زند
// 2. async method بدون ConfigureAwait(false) است
// 3. async method می‌خواهد continuation را روی UI thread اجرا کند
// 4. اما UI thread block شده و منتظر Result است
// 5. پس deadlock!
//
// نکته: در برخی شرایط خاص در WinForms، ممکن است deadlock رخ ندهد
// اگر message pump در حال اجرا باشد و بتواند continuation را پردازش کند.
// اما در اکثر موارد، deadlock رخ می‌دهد.
//
// راه حل: همیشه از ConfigureAwait(false) در library code استفاده کنید
// و هرگز از .Result یا .Wait() در UI thread استفاده نکنید.

// 📚 Library Helper Class - مثال Library Code
public static class LibraryHelper
{
    /// <summary>
    /// مثال یک library method که همیشه ConfigureAwait(false) استفاده می‌کند
    /// </summary>
    public static async Task<string> GetDataAsync()
    {
        // در library code همیشه ConfigureAwait(false) استفاده کنید
        await Task.Delay(100).ConfigureAwait(false);
        
        // شبیه‌سازی عملیات دیگر
        await Task.Run(() => Thread.Sleep(50)).ConfigureAwait(false);
        
        return "Library Data";
    }
}
