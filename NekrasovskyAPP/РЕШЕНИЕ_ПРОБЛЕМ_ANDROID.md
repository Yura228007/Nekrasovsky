# Решение проблем с Android сборкой

## Проблема

При сборке проекта для Android возникают ошибки:
```
error CS0246: Не удалось найти тип или имя пространства имен "Android"
error CS0246: Не удалось найти тип или имя пространства имен "MauiAppCompatActivity"
error CS0246: Не удалось найти тип или имя пространства имен "JniHandleOwnership"
```

## Возможные решения

### 1. Обновление MAUI workload

Android workload установлен, но может быть устаревшим. Обновите workload:

```bash
dotnet workload update
```

Или установите конкретно для Android:

```bash
dotnet workload install maui-android --skip-sign-check
```

### 2. Очистка и восстановление проекта

Полностью очистите проект и восстановите пакеты:

```bash
cd NekrasovskyAPP/NekrasovskyAPP
dotnet clean
Remove-Item -Recurse -Force bin, obj -ErrorAction SilentlyContinue
dotnet restore --force
dotnet build
```

### 3. Проверка версий пакетов

Убедитесь, что версии MAUI пакетов совместимы. В `.csproj` должны быть:
- `Microsoft.Maui.Controls` Version="8.0.100"
- `.NET 8.0 SDK` установлен

Проверьте версию SDK:
```bash
dotnet --version
```
Должна быть `8.0.x` или выше.

### 4. Переустановка MAUI workload

Если обновление не помогло, попробуйте переустановить:

```bash
dotnet workload uninstall maui-android
dotnet workload install maui-android
```

### 5. Проверка конфигурации проекта

Убедитесь, что в `.csproj` файле есть:
```xml
<UseMaui>true</UseMaui>
<TargetFrameworks>net8.0-android;net8.0-windows10.0.19041.0</TargetFrameworks>
```

### 6. Проверка файлов Android

Убедитесь, что файлы `MainActivity.cs` и `MainApplication.cs` находятся в папке `Platforms/Android/` и содержат правильные using директивы:

**MainActivity.cs:**
```csharp
using Android.App;
using Android.Content.PM;
using Android.OS;
```

**MainApplication.cs:**
```csharp
using Android.App;
using Android.Runtime;
```

## Примечание

Windows сборка работает успешно, что означает, что проект настроен правильно. Проблема специфична для Android.

## Альтернативное решение

Если проблема не решается, можно временно исключить Android из сборки:

```xml
<TargetFrameworks Condition="$([MSBuild]::IsOSPlatform('Windows'))">net8.0-windows10.0.19041.0</TargetFrameworks>
```

Но это ограничит сборку только Windows версией.

## Дополнительная информация

- MAUI Android требует установки Android SDK
- Убедитесь, что Android SDK установлен через Visual Studio Installer или вручную
- Проверьте переменные окружения `ANDROID_HOME` и `ANDROID_SDK_ROOT`
