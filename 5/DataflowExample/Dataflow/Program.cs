using System.Text;
using System.Threading.Tasks.Dataflow;

namespace DataflowExample;

class Program
{
    static async Task Main(string[] args)
    {
        Console.WriteLine("=== Dataflow Example for Parallel Processing ===\n");

        // مثال 1: پردازش پایپ لاین ساده
        //await SimplePipelineExample();

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // مثال 2: پردازش موازی با بلوک‌های متعدد
        //await ParallelProcessingExample();

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // مثال 3: پردازش تصاویر شبیه‌سازی شده
        //await ImageProcessingPipelineExample();

        Console.WriteLine("\n" + new string('=', 60) + "\n");

        // مثال 4: پردازش داده با فیلتر و تبدیل
        await DataTransformationPipelineExample();
    }

    /// <summary>
    /// مثال 1: یک پایپ لاین ساده که اعداد را دریافت کرده، 
    /// محاسبه می‌کند و نتیجه را نمایش می‌دهد
    /// </summary>
    static async Task SimplePipelineExample()
    {
        Console.WriteLine("Example 1: Simple Pipeline - Calculate Square of Numbers\n");

        // بلوک ورودی: دریافت اعداد
        var inputBlock = new BufferBlock<int>();

        // بلوک تبدیل: محاسبه مربع
        var transformBlock = new TransformBlock<int, int>(n =>
        {
            var result = n * n;
            Console.WriteLine($"  cal: {n}² = {result}");
            Thread.Sleep(100); // شبیه‌سازی کار محاسباتی
            return result;
        }, new ExecutionDataflowBlockOptions
        {
            MaxDegreeOfParallelism = 3 // پردازش موازی با حداکثر 3 ترد
        });

        // بلوک خروجی: نمایش نتیجه
        var outputBlock = new ActionBlock<int>(result =>
        {
            Console.WriteLine($"  Final result: {result}");
        });

        // اتصال بلوک‌ها به یکدیگر
        inputBlock.LinkTo(transformBlock, new DataflowLinkOptions { PropagateCompletion = true });
        transformBlock.LinkTo(outputBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // ارسال داده‌ها به پایپ لاین
        for (int i = 1; i <= 10; i++)
        {
            await inputBlock.SendAsync(i);
        }

        // اعلام اتمام ورودی‌ها
        inputBlock.Complete();
        await outputBlock.Completion;
    }

    /// <summary>
    /// مثال 2: پردازش موازی با چندین بلوک تبدیل
    /// </summary>
    static async Task ParallelProcessingExample()
    {
        Console.WriteLine("Example 2: Parallel Processing with Multiple Blocks\n");

        // بلوک ورودی
        var inputBlock = new BufferBlock<int>();

        // بلوک Broadcast: ارسال داده به همه بلوک‌های پردازش
        var broadcastBlock = new BroadcastBlock<int>(n => n);

        // بلوک تبدیل 1: محاسبه فاکتوریل
        var factorialBlock = new TransformBlock<int, (int input, long factorial)>(n =>
        {
            long fact = 1;
            for (int i = 2; i <= n; i++)
                fact *= i;
            Console.WriteLine($"  Factorial of {n} = {fact}");
            return (n, fact);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        // بلوک تبدیل 2: محاسبه مجموع ارقام
        var sumDigitsBlock = new TransformBlock<int, (int input, int sum)>(n =>
        {
            int sum = 0;
            int temp = n;
            while (temp > 0)
            {
                sum += temp % 10;
                temp /= 10;
            }
            Console.WriteLine($"  Sum of digits of {n} = {sum}");
            return (n, sum);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        // بلوک تبدیل 3: بررسی اول بودن
        var isPrimeBlock = new TransformBlock<int, (int input, bool isPrime)>(n =>
        {
            if (n < 2)
            {
                Console.WriteLine($"  {n} is not a prime number");
                return (n, false);
            }

            bool isPrime = true;
            for (int i = 2; i * i <= n; i++)
            {
                if (n % i == 0)
                {
                    isPrime = false;
                    break;
                }
            }
            Console.WriteLine($"  {n} is {(isPrime ? "a prime number" : "not a prime number")}");
            return (n, isPrime);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 4 });

        // بلوک جمع‌آوری: ترکیب نتایج
        var joinBlock = new JoinBlock<(int input, long factorial), (int input, int sum), (int input, bool isPrime)>();

        // بلوک نمایش نتیجه نهایی
        var resultBlock = new ActionBlock<Tuple<(int input, long factorial), (int input, int sum), (int input, bool isPrime)>>(tuple =>
        {
            var (factorial, sumDigits, prime) = tuple;
            Console.WriteLine($"\n  Result for {factorial.input}:");
            Console.WriteLine($"    Factorial: {factorial.factorial}");
            Console.WriteLine($"    Sum of digits: {sumDigits.sum}");
            Console.WriteLine($"    Is prime: {prime.isPrime}");
        });

        // اتصال بلوک‌ها: inputBlock -> broadcastBlock -> همه بلوک‌های پردازش
        inputBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = true });
        broadcastBlock.LinkTo(factorialBlock, new DataflowLinkOptions { PropagateCompletion = true });
        broadcastBlock.LinkTo(sumDigitsBlock, new DataflowLinkOptions { PropagateCompletion = true });
        broadcastBlock.LinkTo(isPrimeBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // اتصال به بلوک Join با PropagateCompletion
        var target1Block = new ActionBlock<(int input, long factorial)>(x => joinBlock.Target1.Post(x));
        var target2Block = new ActionBlock<(int input, int sum)>(x => joinBlock.Target2.Post(x));
        var target3Block = new ActionBlock<(int input, bool isPrime)>(x => joinBlock.Target3.Post(x));
        
        factorialBlock.LinkTo(target1Block, new DataflowLinkOptions { PropagateCompletion = true });
        sumDigitsBlock.LinkTo(target2Block, new DataflowLinkOptions { PropagateCompletion = true });
        isPrimeBlock.LinkTo(target3Block, new DataflowLinkOptions { PropagateCompletion = true });

        joinBlock.LinkTo(resultBlock);

        // ارسال داده‌ها
        var numbers = new[] { 5, 7, 12, 13, 17 };
        foreach (var num in numbers)
        {
            await inputBlock.SendAsync(num);
        }

        inputBlock.Complete();
        await resultBlock.Completion;
    }

    /// <summary>
    /// مثال 3: پایپ لاین پردازش تصویر شبیه‌سازی شده
    /// </summary>
    static async Task ImageProcessingPipelineExample()
    {
        Console.WriteLine("Example 3: Image Processing Pipeline\n");

        // بلوک ورودی: دریافت نام فایل‌های تصویر
        var fileInputBlock = new BufferBlock<string>();

        // بلوک 1: بارگذاری تصویر
        var loadImageBlock = new TransformBlock<string, ImageData>(filename =>
        {
            Console.WriteLine($"  [Loading] {filename}");
            Thread.Sleep(200);
            return new ImageData { FileName = filename, Width = 1920, Height = 1080 };
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // بلوک 2: تغییر اندازه
        var resizeBlock = new TransformBlock<ImageData, ImageData>(image =>
        {
            Console.WriteLine($"  [Resizing] {image.FileName}");
            Thread.Sleep(150);
            image.Width = 800;
            image.Height = 600;
            return image;
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

        // بلوک 3: اعمال فیلتر
        var filterBlock = new TransformBlock<ImageData, ImageData>(image =>
        {
            Console.WriteLine($"  [Filter] {image.FileName}");
            Thread.Sleep(100);
            image.FilterApplied = "Sepia";
            return image;
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

        // بلوک 4: ذخیره‌سازی
        var saveBlock = new ActionBlock<ImageData>(image =>
        {
            Console.WriteLine($"  [Saving] {image.FileName} - Size: {image.Width}x{image.Height}, Filter: {image.FilterApplied}");
            Thread.Sleep(100);
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // اتصال بلوک‌ها
        fileInputBlock.LinkTo(loadImageBlock, new DataflowLinkOptions { PropagateCompletion = true });
        loadImageBlock.LinkTo(resizeBlock, new DataflowLinkOptions { PropagateCompletion = true });
        resizeBlock.LinkTo(filterBlock, new DataflowLinkOptions { PropagateCompletion = true });
        filterBlock.LinkTo(saveBlock, new DataflowLinkOptions { PropagateCompletion = true });

        // ارسال فایل‌های تصویر
        var imageFiles = new[] { "image1.jpg", "image2.jpg", "image3.jpg", "image4.jpg", "image5.jpg" };
        foreach (var file in imageFiles)
        {
            await fileInputBlock.SendAsync(file);
        }

        fileInputBlock.Complete();
        await saveBlock.Completion;
    }

    /// <summary>
    /// مثال 4: پایپ لاین تبدیل داده با فیلتر و شاخه‌بندی
    /// </summary>
    static async Task DataTransformationPipelineExample()
    {
        Console.WriteLine("Example 4: Data Transformation Pipeline with Filter and Branching\n");

        // بلوک ورودی: دریافت اعداد
        var inputBlock = new BufferBlock<int>();

        // بلوک تبدیل: تبدیل به رشته
        var toStringBlock = new TransformBlock<int, string>(n => n.ToString(),
            new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 5 });

        // بلوک شاخه‌بندی: ارسال به بلوک‌های مختلف بر اساس شرط
        var broadcastBlock = new BroadcastBlock<string>(s => s);

        // بلوک 1: پردازش اعداد زوج
        var evenBlock = new ActionBlock<string>(s =>
        {
            int n = int.Parse(s);
            if (n % 2 == 0)
            {
                Console.WriteLine($"  [Even] {n} -> {n * 2}");
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

        // بلوک 2: پردازش اعداد فرد
        var oddBlock = new ActionBlock<string>(s =>
        {
            int n = int.Parse(s);
            if (n % 2 == 1)
            {
                Console.WriteLine($"  [Odd] {n} -> {n * 3}");
            }
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 3 });

        // بلوک 3: ذخیره همه داده‌ها
        var logBlock = new ActionBlock<string>(s =>
        {
            Console.WriteLine($"  [Log] Processed number: {s}");
        }, new ExecutionDataflowBlockOptions { MaxDegreeOfParallelism = 2 });

        // اتصال بلوک‌ها
        inputBlock.LinkTo(toStringBlock, new DataflowLinkOptions { PropagateCompletion = true });
        toStringBlock.LinkTo(broadcastBlock, new DataflowLinkOptions { PropagateCompletion = true });
        
        broadcastBlock.LinkTo(evenBlock);
        broadcastBlock.LinkTo(oddBlock);
        broadcastBlock.LinkTo(logBlock);

        // ارسال داده‌ها
        for (int i = 1; i <= 15; i++)
        {
            await inputBlock.SendAsync(i);
        }

        inputBlock.Complete();
        
        // انتظار برای اتمام تمام بلوک‌ها
        await Task.WhenAll(evenBlock.Completion, oddBlock.Completion, logBlock.Completion);
    }
}

/// <summary>
/// کلاس کمکی برای شبیه‌سازی داده‌های تصویر
/// </summary>
class ImageData
{
    public string FileName { get; set; } = string.Empty;
    public int Width { get; set; }
    public int Height { get; set; }
    public string FilterApplied { get; set; } = string.Empty;
}

