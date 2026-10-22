# İmtahan qeydiyyatı

Orta məktəb şagirdlərinin dərslər üzrə imtahan nəticələrinin qeydiyyatı üçün sadə CRUD proqramı.

- **Dərslər:** kod, ad, sinif, müəllimin adı və soyadı.
- **Şagirdlər:** nömrə, ad, soyad və sinif.
- **İmtahanlar:** dərs, şagird, tarix və qiymət.

Hər bölmədə əlavə etmə, redaktə və silmə mümkündür. İmtahan nəticəsi olan dərs və ya şagird silinmir. Eyni dərs, şagird və tarix üzrə təkrar imtahan qəbul edilmir.

Backend C# / ASP.NET Core ilə monolit quruluşda, interfeys Angular və TypeScript ilə yazılıb. Məlumat bazası MS SQL Server-dir.

## Başlatmaq

Docker Desktop açıq olmalıdır. Layihə qovluğunda:

```powershell
.\run.ps1
```

Yaxud:

```sh
docker-compose up --build
```

Proqram: http://localhost:4200

İlk açılışda baza və cədvəllər avtomatik yaradılır. Məlumatlar Docker volume-da saxlanılır. Dayandırmaq üçün `Ctrl+C`.
