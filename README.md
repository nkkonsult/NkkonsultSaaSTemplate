# NkkonsultSaaSTemplate

Template .NET 10 SaaS multi-tenant avec authentification complète.

## Stack
- .NET 10 / ASP.NET Core
- EF Core + PostgreSQL (snake_case naming)
- ASP.NET Identity + JWT Bearer + Refresh Tokens + Magic Link OTP
- Clean Architecture (Domain / Application / Infrastructure / Web)
- Mediator (source generator), FluentValidation, AutoMapper
- MailKit (SMTP), Scalar (OpenAPI)
- Azure Key Vault (optionnel)

## Fonctionnalités incluses
- Auth : register, login (mot de passe + OTP magic link), refresh token, forgot/reset password
- Multi-tenant : isolation par QueryFilter EF Core, TenantMiddleware
- Users : profil utilisateur, onboarding
- Team : invitation par email, acceptation, suppression de membre
- Admin système : liste des tenants, détail tenant, bootstrap admin
- Rôles : Member, Owner, SystemAdmin

## Prise en main

1. Copier `appsettings.json` → `appsettings.Development.json` et renseigner :
   - `ConnectionStrings:NkkonsultDb` — votre connexion PostgreSQL
   - `JwtSettings:SecretKey` — clé secrète JWT (min. 32 caractères)
   - `SmtpSettings` — configuration SMTP pour les emails

2. Créer la base PostgreSQL :
   ```sql
   CREATE DATABASE nkkonsult;
   ```

3. Appliquer les migrations :
   ```bash
   dotnet ef database update --project src/Infrastructure --startup-project src/Web
   ```

4. Lancer :
   ```bash
   dotnet run --project src/Web
   ```

5. Ouvrir Scalar : `http://localhost:5000/scalar`

## Structure du projet
```
src/
├── Domain/           # Entités (Tenant, Invitation), Common
├── Application/      # CQRS handlers (Auth, Users, Team, Tenants, Admin)
├── Infrastructure/   # EF Core, Identity, Services (Auth, Email, Token...)
├── Nkkonsult.Shared/ # DTOs et requests partagés
└── Web/              # Controllers + Middleware
```

## Ajouter votre domaine métier
1. Créer vos entités dans `Domain/Entities/`
2. Créer vos modules CQRS dans `Application/`
3. Ajouter vos DbSets dans `Infrastructure/Data/AppDbContext.cs`
4. Créer vos controllers dans `Web/Controllers/v1/`
5. Générer une migration : `dotnet ef migrations add <NomMigration>`
