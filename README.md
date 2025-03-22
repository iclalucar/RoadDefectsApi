Tabii! İşte bu proje için bir `README.md` dosyası örneği:

```markdown
# Road Defects Detection API

Bu proje, yol bozukluklarını tespit etmek için bir REST API sağlar. Kullanıcılar, yol bozukluğu görüntülerini base64 formatında yükleyebilir ve bu görüntüler üzerinden RoboFlow kullanarak bozukluk tespiti yapılır. API, kullanıcı kayıt ve giriş işlemleri ile güvenli JWT token doğrulaması sağlar.

## Özellikler

- **Kullanıcı Kayıt ve Giriş:** 
  - E-posta ve şifre ile kullanıcı kaydı ve giriş işlemleri.
  - JWT token ile güvenli kimlik doğrulama.

- **Yol Bozukluğu Tespiti:**
  - Base64 formatındaki görseller RoboFlow API'sine gönderilir.
  - Yol bozukluğu tespit edilen görseller için bir JSON yanıtı döndürülür.
  - Bozuklukların detayları, kullanıcı ve tarih bilgileri ile birlikte veri tabanına kaydedilir.

- **Yol Bozukluğu Listesi:** 
  - Kullanıcıların yüklediği bozukluklar ve detayları sorgulanabilir.

## API Sonuçları

### 1. Kullanıcı Kayıt
- **Endpoint:** `POST /RegisterUser`
- **Veri:** 
  ```json
  {
    "Email": "user@example.com",
    "Password": "password123",
    "FullName": "John Doe"
  }
  ```
- **Yanıt:**
  ```json
  {
    "accessToken": "jwt_token_here",
    "user": {
      "Id": "user_id",
      "FullName": "John Doe",
      "Email": "user@example.com"
    }
  }
  ```

### 2. Kullanıcı Giriş
- **Endpoint:** `POST /Login`
- **Veri:**
  ```json
  {
    "Email": "user@example.com",
    "Password": "password123"
  }
  ```
- **Yanıt:**
  ```json
  {
    "accessToken": "jwt_token_here",
    "user": {
      "Id": "user_id",
      "FullName": "John Doe",
      "Email": "user@example.com"
    }
  }
  ```

### 3. Yol Bozukluğu Görseli Yükleme
- **Endpoint:** `POST /UploadImageBase64`
- **Veri:**
  ```json
  {
    "UserId": "user_id",
    "ImageBase64": "base64_encoded_image",
    "Location": "GPS coordinates"
  }
  ```
- **Yanıt:**
  ```json
  {
    "Success": true
  }
  ```

### 4. Yol Bozukluğu Listesi Sorgulama
- **Endpoint:** `POST /GetDefectList`
- **Yanıt:**
  ```json
  [
    {
      "UserName": "John Doe",
      "Location": "GPS coordinates",
      "CreatedAt": "2025-01-01T00:00:00",
      "Confidence": 0.95,
      "ImageBase64": "base64_encoded_image"
    }
  ]
  ```

## Kullanıcı Yetkilendirme

API, JWT token ile kimlik doğrulaması yapmaktadır. Kimlik doğrulama işlemi şu şekilde yapılır:

1. Kullanıcı **kayıt** olduktan sonra JWT token alır.
2. Kullanıcı **giriş** yaptıktan sonra JWT token alır.
3. API'ye yapılacak her istek için **Authorization** başlığına bu token eklenmelidir:
   ```text
   Authorization: Bearer <JWT Token>
   ```

## Proje Kurulumu

### 1. Gereksinimler
- .NET 6 veya daha yeni bir sürümü.
- Visual Studio veya uygun bir IDE.

### 2. Proje Yapılandırması
- Proje, `RoadDefectsDetection.Server` olarak yapılandırılmıştır.
- Veritabanı bağlantıları ve kullanıcı yönetimi için **ASP.NET Identity** kullanılmıştır.

### 3. Veritabanı Ayarları
- Veritabanı bağlantı dizesi `appsettings.json` dosyasındaki `ConnectionStrings` bölümünde belirtilmiştir.
- Veritabanı, EF Core migrations kullanılarak oluşturulmuştur.
  - İlk migration ve veritabanı oluşturmak için terminalde aşağıdaki komutları çalıştırın:
    ```bash
    dotnet ef migrations add InitialCreate
    dotnet ef database update
    ```

## RoboFlow API

Proje, RoboFlow API'yi kullanarak yol bozukluğu tespiti yapmaktadır. RoboFlow API'yi kullanabilmek için geçerli bir API anahtarına ihtiyacınız vardır. API anahtarınızı [RoboFlow](https://roboflow.com/) hesabınız üzerinden alabilirsiniz.
