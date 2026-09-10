# Task Tracker API

ASP.NET Core Web API + Entity Framework Core (Code-First) + SQL Server + JWT Authentication kullanılarak geliştirilmiş bir görev/proje yönetim sistemi. Junior .NET Backend Developer pozisyonlarına başvururken portföyde gösterilmek amacıyla, gerçekçi bir katmanlı mimari (Controller → Service → Repository), DTO kullanımı, JWT tabanlı authentication/authorization ve merkezi hata yönetimi prensipleriyle inşa edilmiştir.

Gereksinimlerin orijinal hali için bkz. [task-tracker-gereksinimler.md](task-tracker-gereksinimler.md).

## Teknoloji Yığını

- **.NET 9** (ASP.NET Core Web API)
- **Entity Framework Core 9** (Code-First, SQL Server provider)
- **SQL Server** (local)
- **JWT Bearer Authentication** (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **BCrypt.Net-Next** — şifre hashleme
- **Swashbuckle.AspNetCore** (Swagger / OpenAPI)

## Mimari

```
Controller  →  Service  →  Repository  →  ApplicationDbContext (EF Core)  →  SQL Server
```

- **Controller**: sadece HTTP ile ilgilenir (route, model binding, response). İş kuralı içermez.
- **Service**: iş mantığı ve yetkilendirme kontrolleri burada (örn. "proje üyesi değilsen 403").
- **Repository**: veritabanı erişimini soyutlar, Service katmanı EF Core'a doğrudan bağımlı değildir.
- **DTO'lar**: Entity'ler API'den doğrudan dönmez (örn. `User.PasswordHash` asla dışarı sızmaz).
- **Global Exception Middleware**: Service katmanı `NotFoundException` / `ForbiddenException` / `InvalidOperationException` / `UnauthorizedAccessException` fırlatır, `Middleware/GlobalExceptionHandler.cs` bunları merkezi olarak 404/403/400/401'e çevirir — Controller'larda hiç `try/catch` yoktur.

### Proje yapısı

```
src/TaskTracker.Api/
├── Controllers/        AuthController, ProjectsController, TasksController
├── Services/            iş mantığı (+ Interfaces/)
├── Repositories/         EF Core erişimi (+ Interfaces/)
├── Models/
│   ├── Entities/        User, Project, ProjectMember, TaskItem
│   ├── Enums/            ProjectRole, TaskStatusEnum, TaskPriority
│   └── Dtos/             Request/response DTO'ları
├── Data/                 ApplicationDbContext
├── Exceptions/           NotFoundException, ForbiddenException
├── Middleware/           GlobalExceptionHandler (IExceptionHandler)
└── Migrations/           EF Core migration'ları
```

## Veri Modeli

- **User** ⟶ birden çok **Project** sahibi olabilir (1-N, `OwnerId`)
- **Project** ⟷ **User**: N-N ilişki, ara tablo **ProjectMember** (`Role`: `Owner` / `Member`). Proje oluşturulunca, oluşturan kullanıcı otomatik olarak `Owner` rolüyle bir `ProjectMember` kaydı da alır.
- **Project** ⟶ birden çok **TaskItem** (1-N)
- **TaskItem** ⟶ opsiyonel olarak bir **User**'a atanabilir (`AssignedToUserId`, nullable) ve her zaman bir **User** tarafından oluşturulmuştur (`CreatedByUserId`, zorunlu — bu alan orijinal gereksinim dokümanının veri modelinde yoktu, ama endpoint tablosunda "görevi silme yetkisi: oluşturan veya Owner" maddesi olduğu için sonradan eklendi)

Tüm `User` FK ilişkileri `DeleteBehavior.Restrict` ile kurulmuştur (SQL Server'ın "multiple cascade paths" hatasını önlemek için); `Project → ProjectMember` ve `Project → TaskItem` ilişkileri `Cascade`'dir.

## Kurulum

### Gereksinimler

- .NET 9 SDK
- SQL Server (local, Developer Edition yeterli)
- `dotnet-ef` aracı (proje manifestosunda tanımlı, aşağıdaki adımla kurulur)

### Adımlar

```bash
git clone <repo-url>
cd TaskTrackerApi

# EF Core CLI aracını yükle
dotnet tool restore

cd src/TaskTracker.Api

# Paketleri geri yükle
dotnet restore
```

**Connection string**: `appsettings.json` içinde `Server=localhost;Database=TaskTrackerDb;Trusted_Connection=True;TrustServerCertificate=True;` olarak ayarlıdır. Farklı bir SQL Server instance'ı kullanıyorsan bu değeri güncelle.

**JWT imzalama anahtarı (önemli — repoda yok, elle eklenmeli)**: Güvenlik nedeniyle `Jwt:Key`, `appsettings.json`'a değil **.NET User Secrets**'a kaydedilir, bu yüzden repoyu klonlayan herkesin kendi anahtarını oluşturması gerekir:

```bash
dotnet user-secrets set "Jwt:Key" "<en az 32 karakterlik rastgele bir string>"
```

Bu adım atlanırsa uygulama başlarken hemen hata verip kapanır (bilerek "fail fast" yapılmıştır — token imzalamadan önce değil, uygulama ayağa kalkarken hatayı görmen için).

```bash
# Veritabanını oluştur / migration'ları uygula
dotnet ef database update

# Uygulamayı başlat
dotnet run
```

Swagger arayüzü: **`http://localhost:5133/swagger`** (port `Properties/launchSettings.json`'da değişebilir).

## Örnek Kullanım (Swagger UI üzerinden)

1. **`POST /api/auth/register`** ile bir kullanıcı oluştur → response'taki `token`'ı kopyala.
2. Sayfanın sağ üstündeki **Authorize** butonuna tıkla, token'ı yapıştır (`Bearer` öneki yazmadan), Authorize → Close.
3. **`POST /api/projects`** ile bir proje oluştur (oluşturan kullanıcı otomatik `Owner` olur).
4. **`POST /api/projects/{id}/members`** ile başka bir kullanıcıyı projeye ekle.
5. **`POST /api/projects/{projectId}/tasks`** ile bir görev oluştur.
6. **`GET /api/projects/{projectId}/tasks?status=Todo&priority=High&page=1&pageSize=20&sortBy=dueDate`** ile filtreleme/sayfalamayı dene.
7. **`PATCH /api/tasks/{id}/status`** ve **`PATCH /api/tasks/{id}/assign`** ile durumu/atamayı güncelle.
8. Farklı bir kullanıcıyla register olup, üyesi olmadığı bir projeye erişmeyi dene → **403** almalısın. Token'sız istek atarsan → **401**.

## Endpoint Listesi

### Auth
| Method | Route | Yetki |
|---|---|---|
| POST | `/api/auth/register` | Herkese açık |
| POST | `/api/auth/login` | Herkese açık |

### Projects
| Method | Route | Yetki |
|---|---|---|
| GET | `/api/projects` | Giriş yapmış kullanıcı (üyesi olduğu projeler) |
| GET | `/api/projects/{id}` | Proje üyesi |
| POST | `/api/projects` | Giriş yapmış kullanıcı |
| PUT | `/api/projects/{id}` | Sadece Owner |
| DELETE | `/api/projects/{id}` | Sadece Owner |
| POST | `/api/projects/{id}/members` | Sadece Owner |
| DELETE | `/api/projects/{id}/members/{userId}` | Sadece Owner |

### Tasks
| Method | Route | Yetki |
|---|---|---|
| GET | `/api/projects/{projectId}/tasks` | Proje üyesi (filtreleme: `status`, `priority`, `assignedUserId`; sayfalama: `page`, `pageSize`; sıralama: `sortBy=dueDate\|priority\|createdAt`) |
| GET | `/api/tasks/{id}` | Proje üyesi |
| POST | `/api/projects/{projectId}/tasks` | Proje üyesi |
| PUT | `/api/tasks/{id}` | Proje üyesi |
| PATCH | `/api/tasks/{id}/status` | Proje üyesi |
| PATCH | `/api/tasks/{id}/assign` | Proje üyesi (sadece proje üyesi birine atanabilir) |
| DELETE | `/api/tasks/{id}` | Görevi oluşturan veya proje Owner'ı |

## Hata Formatı

Tüm hatalar `{ "error": "..." }` formatında döner (validasyon hataları hariç, onlar ASP.NET Core'un standart `ValidationProblemDetails` formatını kullanır). Beklenmeyen (kod hatası kaynaklı) exception'lar 500 olarak, detay sızdırmadan döner; gerçek detay sadece sunucu loglarına yazılır.

## Bu Projede Öğrendiklerim

Bu proje, backend'e çok az deneyimle başlayıp sıfırdan öğrenerek yazıldı. Bazı önemli noktalar:

- **Katmanlı mimari ve DTO kullanımının nedeni**: Entity'leri doğrudan API'den döndürmemek (özellikle `PasswordHash` gibi hassas alanlar için), iş mantığını Controller'dan ayırmak.
- **EF Core Code-First + Migrations**: `DeleteBehavior` ayarlanmazsa SQL Server'ın "multiple cascade paths" hatası verdiğini, birden fazla FK aynı tabloya (bu projede `User`) farklı yollardan cascade delete uygularsa bunun neden sorun olduğunu deneyerek öğrendim.
- **JWT authentication**: token üretimi (claims, signing key, expiration), `TokenValidationParameters` ile doğrulama, ve imzalama anahtarının neden asla `appsettings.json`'a değil User Secrets'a konması gerektiği.
- **Merkezi hata yönetimi**: `IExceptionHandler` (ASP.NET Core 8+) ile her Controller'da tekrar eden `try/catch` bloklarını nasıl tek bir yere topladığım.
- **Gerçek hayattaki debug deneyimi** — bu projede karşılaştığım ve çözdüğüm gerçek hatalar:
  - `Swashbuckle.AspNetCore 10.x`'in yeni `Microsoft.OpenApi 2.x` sürümüyle geldiğinde, eski tutorial'lardaki `Microsoft.OpenApi.Models` namespace'inin ve `OpenApiReference` API'sinin artık geçerli olmadığını, derleyici hatalarını okuyarak yeni API şekline (`OpenApiSecuritySchemeReference` vs.) nasıl uyum sağladığımı.
  - `ControllerBase.Forbid(string)`'in parametresinin bir mesaj değil, bir authentication *scheme adı* olduğunu — yanlış kullanınca 403 yerine 500 aldığımı, doğrusunun `StatusCode(403, ...)` olduğunu.
  - `System.Text.Json`'ın enum'ları varsayılan olarak sayı olarak serileştirdiğini, API'yi kullanılabilir kılmak için `JsonStringEnumConverter` eklemem gerektiğini.
  - Swagger'ın "Authorize" butonu çalışmıyor göründüğünde, sorunun arayüzde değil `/swagger/v1/swagger.json`'ın kendisinde (`"security": [{}]` gibi eksik/bozuk bir referans) olabileceğini — ham API çıktısına bakmanın, arayüzden tahmin yürütmekten çok daha hızlı bir debug yöntemi olduğunu.

## Kapsam Dışı

Refresh token, e-posta doğrulama, dosya/ek yükleme, real-time bildirimler (SignalR) ve frontend bu proje kapsamında yer almamaktadır (bkz. [task-tracker-gereksinimler.md](task-tracker-gereksinimler.md)).
