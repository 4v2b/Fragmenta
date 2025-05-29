# Інструкція з розгортання веб-застосунку

## Опис системи
Веб-застосунок "для організації та планування завдань побудований на архітектурі ASP.NET Core Web API + React SPA з використанням Docker для контейнеризації. Система включає базу даних MS SQL Server, емулятор Azure Storage (Azurite) для роботи з файлами та SignalR для оновлень в реальному часі.

## Передумови
- Docker та Docker Compose встановлені на цільовій системі
- Доступ до репозиторію проекту

## Покрокова інструкція запуску

### Крок 1: Підготовка середовища
```bash
# Клонування репозиторію
git clone https://github.com/4v2b/Fragmenta.git
cd /Шлях_до_проекту/Fragmenta
```

### Крок 2: Створення конфігураційних файлів
1. **Backend конфігурація:**
   ```bash
   # Перейдіть до директорії Web API
   cd backend/Fagmenta.Api/
   # Створіть файл appsettings.Production.json з наведеною нижче конфігурацією
   ```

2. **Frontend конфігурація:**
   ```bash
   # Перейдіть до директорії React
   cd frontend/fragmentareact/
   # Створіть файл .env з наведеними нижче змінними
   ```

#### Вміст файлу appsettings.Production.json

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "DatabaseOptions": {
    "MigrateDatabaseOnStartup": true,
    "UseMsSql": true,
    "SeedTestData": false
  },
  "Frontend": {
    "BaseUrl": "http://localhost:3000"
  },
  "AzureStorage": {
    "ConnectionString": "DefaultEndpointsProtocol=http;AccountName=devstoreaccount1;AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;BlobEndpoint=http://azurite:10000/devstoreaccount1;",
    "ContainerName": "attachments"
  },
  "ConnectionStrings": {
    "DefaultConnection": "Server=mssql;Database=FragmentaDB;User Id=sa;Password=Password1234;TrustServerCertificate=true"
  },
  "AllowedCorsOrigins": ["http://localhost:5173", "http://localhost:3000"],
  "Jwt": {
    "Key": "Jwt secret key",
    "Issuer": "webapi",
    "Audience": "frontend",
    "ExpireMinutes": 15
  },
  "Smtp": {
    "ApiKey": "16566f256b3d9bed90d9605693fec463",
    "FromName": "Fragmenta App Support",
    "FromEmail": "support@fragmenta.digital",
    "RequestUrl": "https://send.api.mailtrap.io/api/send"
  }
}
```

**Пояснення ключових секцій:**
- `DatabaseOptions`:
  - `MigrateDatabaseOnStartup: true` - автоматично застосовує міграції БД при запуску
  - `SeedTestData: false` - встановіть в `true` для заповнення тестовими даними
- `AzureStorage` - налаштування емулятора Azure Storage для локальної розробки
- `Jwt` - конфігурація JWT токенів для автентифікації користувачів
- `Smtp` - налаштування доступу до Mailtrap сервісу надсилання листів:
  - `ApiKey` - токен доступу до Mailtrap (отриманий в особистому кабінеті)
  - `RequestUrl: "https://send.api.mailtrap.io/api/send"` - ендпоінт надсилання листів
  - `FromEmail` - електронна пошта відправника (верифікована в Mailtrap)
  - `FromName` - назва відправника, що відображається одержувачу листа

### Вміст файлу .env

```env
VITE_API_URL = "http://localhost:5000/api"
VITE_SIGNALR_URL = "http://localhost:5000/hub"
```

**Примітка:** Переконайтеся, що порт в `.env` файлі співпадає з портом у `docker-compose.override.yml`:

```yml
webapi:
  build:
    ...
  ports:
    - "127.0.0.1:5000:5000"
```


### Крок 3: Налаштування параметрів (якщо потрібно)
1. У `appsettings.Production.json` змініть:
   - JWT секретний ключ (`Jwt.Key`)
   - Email налаштування для іншого існужчого облікового запису (`Smtp` секція)
2. У `.env` файлі перевірте відповідність портів з `docker-compose.override.yml`

### Крок 4: Запуск застосунку за допомогою Docker Compose

**Збірка та запуск контейнерів:**
```bash
# Поверніться до кореневої директорії проекту
cd /Шлях_до_проекту/Fragmenta

# Запустіть всі сервіси
docker-compose up --build -d
```

**Перевірка статусу сервісів:**
```bash
# Переглянути стан всіх контейнерів
docker-compose ps

# Переглянути логи всіх сервісів
docker-compose logs -f

# Переглянути логи конкретного сервісу
docker-compose logs -f webapi
```

### Крок 4: Перевірка функціональності

Відкрийте `http://localhost:3000` - має відобразитися React застосунок

## Управління застосунком

### Зупинка сервісів
```bash
docker-compose down
```

### Оновлення застосунку
```bash
git pull origin main
docker-compose down
docker-compose up --build -d
```

## Проблеми та рішення

**Проблема:** Інтерфейс застосунку відображається, але не відповідає на дії користувача
**Причина:** Web API сервіс не працює або недоступний
**Рішення:** 
1. Перевірте статус усіх контейнерів: `docker-compose ps`
2. Якщо сервіс `webapi` має статус "Exited", перевірте логи: `docker-compose logs webapi`
3. Переконайтеся, що `mssql` та `azurite` контейнери запущені - Web API залежить від них
4. При необхідності перезапустіть сервіс: `docker-compose restart webapi`

**Проблема:** Контейнер Web API постійно перезапускається
**Причина:** База даних ще не готова до прийому з'єднань
**Рішення:** 
- Зачекайте 30-60 секунд після запуску - MS SQL Server потребує часу для ініціалізації
- Перевірте логи бази даних: `docker-compose logs mssql`

**Проблема:** Контейнери не запускаються взагалі
**Рішення:** 
1. Перевірте логи конкретного сервісу: `docker-compose logs [Назва_Сервісу]`
2. Переконайтеся, що порти не зайняті іншими додатками
3. Перевірте наявність достатнього місця на диску

**Проблема:** База даних недоступна або помилки з'єднання
**Рішення:** 
1. Переконайтеся, що паролі в `appsettings.Production.json` та `docker-compose.override.yml` співпадають
2. Перевірте правильність назви сервера (`mssql`) у connection string
3. Переконайтеся, що `TrustServerCertificate=true` присутнє у connection string

**Проблема:** CORS помилки при взаємодії frontend з API
**Рішення:** 
1. Перевірте наявність URL React застосунку в `AllowedCorsOrigins` у файлі `appsettings.Production.json`
2. Переконайтеся, що `VITE_API_URL` у `.env` файлі вказує на правильний порт API

**Проблема:** Файли не завантажуються (Azure Storage помилки)
**Рішення:** 
1. Перевірте, що контейнер `azurite` запущений: `docker-compose ps`
2. Переконайтеся, що connection string для Azure Storage правильний
3. Контейнер `attachments` має бути створений автоматично при першому запуску