# Nekrasovsky

Система управления складом Nekrasovsky - комплексное решение для управления складскими операциями, включающее веб-сервер, нативное Android приложение и кроссплатформенное MAUI приложение.

## 📋 Описание проекта

Nekrasovsky - это полнофункциональная система управления складом, предназначенная для автоматизации складских процессов, управления номенклатурой, отслеживания перемещений материалов и продуктов, а также ведения отчетности о работе сотрудников.

## 🏗️ Архитектура проекта

Проект состоит из трех основных компонентов:

1. **Server** - Backend API на ASP.NET Core
2. **Android App** - Нативное Android приложение на Kotlin
3. **NekrasovskyAPP** - Кроссплатформенное приложение на .NET MAUI

## 🛠️ Технологии

### Server
- **.NET 8.0** - платформа разработки
- **ASP.NET Core Web API** - RESTful API
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - база данных
- **Swagger** - документация API
- **Npgsql** - драйвер PostgreSQL

### Android App
- **Kotlin** - основной язык программирования
- **Jetpack Compose** - современный UI фреймворк
- **Material Design 3** - дизайн система
- **Retrofit** - HTTP клиент для работы с API
- **Hilt** - dependency injection
- **Coroutines** - асинхронное программирование
- **MVVM** - архитектурный паттерн
- **Gson** - сериализация JSON

### MAUI App
- **.NET 8.0** - платформа разработки
- **.NET MAUI** - кроссплатформенный фреймворк
- **Entity Framework Core 9.0** - ORM
- **PostgreSQL** - база данных
- **XAML** - разметка интерфейса

## 📁 Структура проекта

```
Nekrasovsky-master/
├── server/                 # Backend API сервер
│   └── server/
│       ├── Controllers/    # API контроллеры
│       ├── Services/       # Бизнес-логика
│       ├── Models/         # Модели данных
│       ├── Data/           # DbContext и миграции
│       ├── Middleware/     # Промежуточное ПО
│       └── Migrations/     # Миграции БД
│
├── android-app/            # Android приложение
│   └── app/
│       └── src/main/java/com/nekrasovsky/android/
│           ├── data/       # API, модели, репозитории
│           └── ui/         # UI компоненты и экраны
│
├── NekrasovskyAPP/         # MAUI приложение
│   └── NekrasovskyAPP/
│       ├── Models/         # Модели данных
│       ├── Services/       # Сервисы
│       ├── Platforms/      # Платформо-специфичный код
│       └── Resources/      # Ресурсы приложения
│
└── Technical_Files/        # Техническая документация
```

## 🚀 Быстрый старт

### Предварительные требования

#### Для Server:
- .NET 8.0 SDK
- PostgreSQL 12+
- Visual Studio 2022 или VS Code

#### Для Android App:
- Android Studio Hedgehog | 2023.1.1 или новее
- Android SDK 24+ (Android 7.0)
- Kotlin 1.9.20+
- JDK 17

#### Для MAUI App:
- .NET 8.0 SDK
- Visual Studio 2022 с поддержкой MAUI
- Платформо-специфичные SDK (Android, iOS, Windows)

### Установка и запуск

#### 1. Настройка базы данных

1. Установите PostgreSQL
2. Создайте базу данных:
```sql
CREATE DATABASE Nekrasovsky;
```

3. Настройте строку подключения в `server/server/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=Nekrasovsky;Username=postgres;Password=your_password"
  }
}
```

#### 2. Запуск Server

```bash
cd server/server
dotnet restore
dotnet ef database update
dotnet run
```

Сервер будет доступен по адресу:
- HTTP: `http://localhost:5000`
- HTTPS: `https://localhost:5001`
- Swagger UI: `http://localhost:5000/swagger`

#### 3. Запуск Android App

1. Откройте проект в Android Studio
2. Настройте `BASE_URL` в `android-app/app/src/main/java/com/nekrasovsky/android/data/api/RetrofitModule.kt`:
   - Для эмулятора: `http://10.0.2.2:5000/`
   - Для реального устройства: `http://<IP_КОМПЬЮТЕРА>:5000/`
3. Запустите приложение на устройстве или эмуляторе

#### 4. Запуск MAUI App

```bash
cd NekrasovskyAPP/NekrasovskyAPP
dotnet restore
dotnet build
dotnet run
```

## 📱 Основной функционал

### Управление пользователями
- Регистрация и авторизация
- Управление профилями пользователей
- Система разрешений и ролей

### Управление складами
- Создание и редактирование складов
- Управление типами складов
- Отслеживание перемещений между складами

### Управление номенклатурой
- **Продукты** - управление готовой продукцией
- **Материалы** - управление сырьем и материалами
- **Рецепты** - управление рецептурами производства

### Складские операции
- **Запросы на детали (Part Requests)** - создание и обработка запросов
- **Доступные перемещения (Accessible Movements)** - управление перемещениями материалов
- **Заполнение складов (Filling Warehouses)** - учет заполненности складов
- **Сменные передачи (Shift Transfers)** - передача смены

### Отчетность
- **Отчеты о работе (Work Reports)** - учет рабочего времени
- **События тревоги (Alarm Events)** - отслеживание критических событий
- **Логи запросов (Request Logs)** - аудит всех запросов к API

## 🔌 API Endpoints

Основные API endpoints:

- `/api/users` - управление пользователями
- `/api/products` - управление продуктами
- `/api/materials` - управление материалами
- `/api/warehouses` - управление складами
- `/api/work-reports` - отчеты о работе
- `/api/part-requests` - запросы на детали
- `/api/alarm-events` - события тревоги
- `/api/recipes` - рецепты
- `/api/shift-transfers` - сменные передачи
- `/api/filling-warehouses` - заполнение складов
- `/api/accessible-movements` - доступные перемещения
- `/api/permissions` - разрешения
- `/api/user-permissions` - разрешения пользователей

Полная документация API доступна через Swagger UI после запуска сервера.

## 📚 Документация

Дополнительная техническая документация находится в папке `Technical_Files/`:
- Техническое задание
- Схема базы данных
- Списки номенклатуры и складов
- Описание ролей и разрешений

## 🔒 Безопасность

- Шифрование паролей пользователей
- Middleware для аудита запросов
- Система разрешений на уровне API
- Валидация входных данных

## 🧪 Разработка

### Миграции базы данных

```bash
cd server/server
dotnet ef migrations add MigrationName
dotnet ef database update
```

### Тестирование API

Используйте Swagger UI для тестирования API endpoints:
```
http://localhost:5000/swagger
```

## 📝 Лицензия

Проект создан для системы управления складом Nekrasovsky.

## 👥 Контакты

Для вопросов и предложений обращайтесь к команде разработки.

