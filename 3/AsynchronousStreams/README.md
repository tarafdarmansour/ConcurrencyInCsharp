# Book API

یک API برای مدیریت کتاب‌ها با عملیات CRUD (ایجاد، خواندن، به‌روزرسانی، حذف).

## ویژگی‌ها

- ایجاد کتاب جدید
- نمایش لیست تمام کتاب‌ها
- نمایش جزئیات یک کتاب خاص
- ویرایش کتاب موجود
- حذف کتاب

## مدل Book

- `Id`: شناسه یکتای کتاب
- `Name`: نام کتاب
- `Price`: قیمت کتاب

## Endpoints

### GET /api/books
نمایش لیست تمام کتاب‌ها

### GET /api/books/{id}
نمایش جزئیات یک کتاب خاص

### POST /api/books
ایجاد کتاب جدید
```json
{
  "name": "نام کتاب",
  "price": 100.00
}
```

### PUT /api/books/{id}
ویرایش کتاب موجود
```json
{
  "name": "نام جدید کتاب",
  "price": 150.00
}
```

### DELETE /api/books/{id}
حذف کتاب

## اجرای پروژه

```bash
dotnet restore
dotnet run
```

پس از اجرا، API در `https://localhost:5001` یا `http://localhost:5000` در دسترس خواهد بود.

Swagger UI در آدرس `https://localhost:5001/swagger` در دسترس است.

