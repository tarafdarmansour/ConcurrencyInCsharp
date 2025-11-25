# Observable Web API - آموزش کاربردی System.Reactive

این پروژه یک Web API آموزشی است که مثال‌های کاربردی از استفاده از `IObservable<T>` و کتابخانه `System.Reactive` را نمایش می‌دهد. هدف این پروژه کمک به درک بهتر مفاهیم برنامه‌نویسی واکنشی (Reactive Programming) است.

---

# Observable Web API - Practical System.Reactive Tutorial

This project is an educational Web API that demonstrates practical examples of using `IObservable<T>` and the `System.Reactive` library. The goal is to help developers better understand reactive programming concepts.

## 🚀 شروع سریع / Getting Started

### پیش‌نیازها / Prerequisites
- .NET 8.0 یا بالاتر / .NET 8.0 or higher
- Visual Studio 2022 یا VS Code / Visual Studio 2022 or VS Code

### اجرای پروژه / Running the Project

```bash
# کلون کردن پروژه / Clone the project
git clone <repository-url>
cd ObservableWebApi

# بازسازی و اجرای پروژه / Build and run
dotnet build
dotnet run
```

پروژه روی `https://localhost:5001` اجرا خواهد شد و Swagger UI در دسترس خواهد بود.

The project will run on `https://localhost:5001` with Swagger UI available.

## 📚 کنترلرها و مثال‌ها / Controllers and Examples

### 1. StreamingController - جریان داده بلادرنگ / Real-time Data Streaming

این کنترلر مثال‌هایی از جریان داده‌های بلادرنگ را نمایش می‌دهد.

This controller demonstrates real-time data streaming examples.

#### API Endpoints:

- `GET /api/streaming/stock-prices` - جریان قیمت های بورس / Stock price stream
- `GET /api/streaming/sensor-data` - جریان داده های سنسور / Sensor data stream
- `GET /api/streaming/system-logs` - جریان لاگ های سیستم / System logs stream
- `GET /api/streaming/stock-prices/filtered/{threshold}` - قیمت های بورس فیلتر شده / Filtered stock prices
- `GET /api/streaming/sensor-data/transformed` - داده های سنسور تبدیل شده (فارنهایت) / Transformed sensor data (Fahrenheit)

#### مفاهیم کلیدی / Key Concepts:
- تولید داده‌های پیوسته / Continuous data generation
- فیلتر کردن جریان داده / Data stream filtering
- تبدیل داده‌ها (Map) / Data transformation (Map)

---

### 2. EventController - مدیریت رویدادها / Event Handling

این کنترلر چگونگی مدیریت رویدادها به صورت واکنشی را نمایش می‌دهد.

This controller demonstrates reactive event handling.

#### API Endpoints:

- `GET /api/event/user-actions` - جریان رویدادهای کاربر / User action events
- `GET /api/event/system-errors` - خطاهای سیستم فیلتر شده / Filtered system errors
- `GET /api/event/order-summaries` - خلاصه سفارشات / Order summaries
- `GET /api/event/user-stats/{windowSeconds}` - آمار کاربران در بازه زمانی / User statistics over time
- `POST /api/event/button-click` - شبیه‌سازی کلیک دکمه / Simulate button click
- `GET /api/event/button-clicks/debounced` - کلیک های دکمه با Debounce / Debounced button clicks
- `GET /api/event/combined-events` - رویدادهای ترکیبی / Combined events

#### مفاهیم کلیدی / Key Concepts:
- مدیریت رویدادها / Event handling
- Debounce و Throttle / Debounce and Throttle
- گروه‌بندی و تجمیع داده‌ها / Data grouping and aggregation
- ترکیب جریان‌های مختلف / Combining different streams

---

### 3. TransformationController - تبدیل داده‌ها / Data Transformation

این کنترلر عملگرهای مختلف تبدیل داده‌ها را نمایش می‌دهد.

This controller demonstrates various data transformation operators.

#### API Endpoints:

- `GET /api/transformation/products/expensive/{minPrice}` - محصولات گران‌قیمت / Expensive products
- `GET /api/transformation/products/convert-currency/{rate}` - تبدیل ارز / Currency conversion
- `GET /api/transformation/sensor/cleaned` - داده‌های سنسور پاک‌سازی شده / Cleaned sensor data
- `GET /api/transformation/user-activity/stats` - آمار فعالیت کاربران / User activity statistics
- `GET /api/transformation/sensor/aggregated` - داده‌های سنسور تجمیع شده / Aggregated sensor data
- `GET /api/transformation/products/discount/{discountPercent}` - محصولات با تخفیف / Discounted products
- `GET /api/transformation/user-activity/summary` - خلاصه فعالیت کاربران / User activity summary
- `GET /api/transformation/products/sorted-by-price` - محصولات مرتب شده / Sorted products

#### مفاهیم کلیدی / Key Concepts:
- Where (فیلتر) / Where (Filter)
- Select (نقشه‌برداری) / Select (Map)
- Distinct (حذف تکراری) / Distinct (Remove duplicates)
- GroupBy (گروه‌بندی) / GroupBy (Grouping)
- Scan (تجمیع running) / Scan (Running aggregation)

---

### 4. ErrorHandlingController - مدیریت خطا / Error Handling

این کنترلر استراتژی‌های مختلف مدیریت خطا در observables را نمایش می‌دهد.

This controller demonstrates various error handling strategies in observables.

#### API Endpoints:

- `GET /api/errorhandling/api-requests/catch` - مدیریت خطای ساده / Simple error handling
- `GET /api/errorhandling/api-requests/retry` - تلاش مجدد / Retry logic
- `GET /api/errorhandling/db-operations/fallback` - استراتژی fallback / Fallback strategy
- `GET /api/errorhandling/file-operations/advanced` - مدیریت خطای پیشرفته / Advanced error handling
- `GET /api/errorhandling/combined/chain` - زنجیره مدیریت خطا / Chained error handling
- `GET /api/errorhandling/operations/timeout` - Timeout و مدیریت خطا / Timeout handling

#### مفاهیم کلیدی / Key Concepts:
- Catch / Catch
- Retry / Retry
- OnErrorResumeNext / OnErrorResumeNext
- Timeout / Timeout
- زنجیره مدیریت خطا / Error handling chains

---

### 5. CombiningController - ترکیب Observables / Combining Observables

این کنترلر روش‌های مختلف ترکیب چندین observable را نمایش می‌دهد.

This controller demonstrates various ways to combine multiple observables.

#### API Endpoints:

- `GET /api/combining/temperature/merged` - ترکیب سنسورهای دما با Merge / Merge temperature sensors
- `GET /api/combining/environment/combined-latest` - داده‌های محیطی با CombineLatest / Environmental data with CombineLatest
- `GET /api/combining/orders/payments/zipped` - سفارشات و پرداخت‌ها با Zip / Orders and payments with Zip
- `GET /api/combining/user-activity/combined-stats` - آمار فعالیت ترکیبی / Combined activity statistics
- `GET /api/combining/system/concatenated` - متریک های سیستم با Concat / System metrics with Concat
- `GET /api/combining/system-events/merged` - رویدادهای سیستم ترکیبی / Merged system events
- `GET /api/combining/temperature/amb` - سنسورها با Amb / Sensors with Amb
- `GET /api/combining/environment/switched` - داده‌ها با Switch / Data with Switch
- `GET /api/combining/user-system/joined` - همبستگی داده‌ها با Join / Data correlation with Join

#### مفاهیم کلیدی / Key Concepts:
- Merge / Merge
- CombineLatest / CombineLatest
- Zip / Zip
- Concat / Concat
- Amb / Amb
- Switch / Switch
- Join / Join

## 🔧 معماری پروژه / Project Architecture

### ساختار پروژه / Project Structure:
```
ObservableWebApi/
├── Controllers/
│   ├── StreamingController.cs      # جریان داده بلادرنگ
│   ├── EventController.cs          # مدیریت رویدادها
│   ├── TransformationController.cs # تبدیل داده‌ها
│   ├── ErrorHandlingController.cs  # مدیریت خطا
│   └── CombiningController.cs      # ترکیب observables
├── Program.cs                      # نقطه ورودی برنامه
└── README.md                       # این فایل
```

### الگوهای طراحی استفاده شده / Design Patterns Used:

1. **Subject Pattern**: استفاده از `Subject<T>` برای تولید و توزیع داده‌ها
2. **Observer Pattern**: پیاده‌سازی کامل الگوی Observer با IObservable
3. **Reactive Extensions**: استفاده از عملگرهای Rx برای تبدیل و ترکیب داده‌ها
4. **Streams Composition**: ترکیب چندین جریان داده به صورت declarative

## 📖 یادگیری بیشتر / Further Learning

### مفاهیم پایه Reactive Programming:
- **Observable**: جریان داده که می‌توان subscribe کرد
- **Observer**: مصرف‌کننده داده‌های observable
- **Operators**: توابع pure برای تبدیل observables
- **Subscription**: اتصال observer به observable
- **Hot vs Cold Observables**: تفاوت بین observables داغ و سرد

### عملگرهای مهم Rx:
- **Creation**: `Observable.Return`, `Observable.Interval`, `Observable.FromEvent`
- **Transformation**: `Select`, `SelectMany`, `Where`, `GroupBy`
- **Filtering**: `Distinct`, `Take`, `Skip`, `Throttle`
- **Combining**: `Merge`, `Concat`, `Zip`, `CombineLatest`
- **Error Handling**: `Catch`, `Retry`, `OnErrorResumeNext`
- **Time-based**: `Delay`, `Timeout`, `Window`, `Buffer`

## 🧪 تست API / Testing the API

برای تست API می‌توانید از Swagger UI استفاده کنید یا از ابزارهایی مانند Postman/curl:

```bash
# مثال تست جریان داده‌های بورس
curl -X GET "https://localhost:5001/api/streaming/stock-prices" \
     -H "accept: text/plain"

# مثال تست رویدادهای کاربر
curl -X GET "https://localhost:5001/api/event/user-actions" \
     -H "accept: text/plain"
```

## 🤝 مشارکت / Contributing

این پروژه برای اهداف آموزشی ایجاد شده است. برای بهبود آن می‌توانید:
- مثال‌های جدید اضافه کنید
- کد را بهینه‌سازی کنید
- مستندات را بهبود بخشید
- باگ‌ها را گزارش کنید

## 📄 مجوز / License

این پروژه تحت مجوز MIT منتشر شده است.

---

**نکته**: برای درک بهتر مفاهیم، کد هر کنترلر را مطالعه کنید و با Swagger UI آزمایش کنید. هر endpoint با کامنت‌های فارسی و انگلیسی توضیح داده شده است.

**Note**: To better understand the concepts, study the code of each controller and experiment with the Swagger UI. Each endpoint is documented with Persian and English comments.

