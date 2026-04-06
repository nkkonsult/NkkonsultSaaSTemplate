# dotnet new Template Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add `.template.config/template.json` to NkkonsultSaaSTemplate so that `dotnet new nkkonsult-saas -n MonProjet` generates a fully working project with all `Nkkonsult` references replaced.

**Architecture:** Single `template.json` with `sourceName: "Nkkonsult"` for PascalCase substitutions, plus a `derived` symbol for lowercase (`nkkonsult`) substitutions. Migrations are excluded via `sources/modifiers/exclude`.

**Tech Stack:** .NET 10, dotnet template engine (Microsoft.TemplateEngine), C# / JSON

---

## Substitution Map

| In template files | Generated value | Mechanism |
|---|---|---|
| `Nkkonsult` (PascalCase) | `MonProjet` | `sourceName` |
| `NkkonsultDb` | `MonProjetDb` | `sourceName` (substring) |
| `nkkonsult` (lowercase) | `monprojet` | derived symbol `nameLower` |
| `nkkonsult-api` | `monprojet-api` | derived symbol `nameLower` |
| `nkkonsult-clients` | `monprojet-clients` | derived symbol `nameLower` |
| Folder `Nkkonsult.Shared/` | `MonProjet.Shared/` | `sourceName` (dir rename) |
| File `NkkonsultSaaSTemplate.slnx` | `MonProjetSaaSTemplate.slnx` | `sourceName` (file rename) |

## File Structure

- **Create:** `.template.config/template.json` — template descriptor at root of repo

---

## Task 1: Create `.template.config/template.json`

**Files:**
- Create: `c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate/.template.config/template.json`

- [ ] **Step 1: Create the directory and file**

```bash
mkdir -p c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate/.template.config
```

Write `c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate/.template.config/template.json`:

```json
{
  "$schema": "http://json.schemastore.org/template",
  "author": "Nkkonsult",
  "classifications": ["Web", "API", "SaaS", "Clean Architecture"],
  "identity": "Nkkonsult.SaaSTemplate",
  "name": "Nkkonsult SaaS Template",
  "shortName": "nkkonsult-saas",
  "sourceName": "Nkkonsult",
  "preferNameDirectory": true,
  "symbols": {
    "nameLower": {
      "type": "derived",
      "valueSource": "name",
      "valueTransform": "lowerCase",
      "replaces": "nkkonsult"
    }
  },
  "sources": [
    {
      "modifiers": [
        {
          "exclude": [
            ".git/**",
            ".template.config/**",
            "artifacts/**",
            "src/Infrastructure/Migrations/**",
            "src.sln",
            "**/*.user",
            "**/bin/**",
            "**/obj/**"
          ]
        }
      ]
    }
  ]
}
```

**Why each key:**
- `sourceName: "Nkkonsult"` — replaces every literal `Nkkonsult` in file contents AND renames files/directories containing `Nkkonsult` in their name (e.g., `Nkkonsult.Shared/` → `MonProjet.Shared/`).
- `nameLower` derived symbol — lowercase of the `-n` value; `replaces: "nkkonsult"` substitutes lowercase occurrences in `appsettings.json` (database name, JWT issuer, JWT audience) and `DependencyInjection.cs` guard message.
- `sources/modifiers/exclude` — prevents `.git/`, `artifacts/`, EF Core migration files, and build outputs from being copied into generated projects.
- `preferNameDirectory: true` — `dotnet new nkkonsult-saas -n Foo` creates `Foo/` output dir automatically.

- [ ] **Step 2: Verify the file exists and is valid JSON**

```bash
cat c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate/.template.config/template.json | python3 -c "import sys,json; json.load(sys.stdin); print('valid JSON')"
```

Expected output: `valid JSON`

- [ ] **Step 3: Commit the template config**

```bash
cd c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate && rtk git add .template.config/template.json && rtk git commit -m "feat: add dotnet new template configuration"
```

---

## Task 2: Install and generate a test project

**Files:**
- Generated output: `/tmp/TestProjet/` (ephemeral — for testing only, not committed)

- [ ] **Step 1: Uninstall any previous version of the template (if any)**

```bash
dotnet new uninstall c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate 2>/dev/null || true
```

Expected: either uninstall confirmation or "not installed" message — both are fine.

- [ ] **Step 2: Install the template from the local repo root**

```bash
dotnet new install c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate
```

Expected output contains:
```
Success: Nkkonsult.SaaSTemplate::1.0.0 installed the following templates:
  nkkonsult-saas
```

- [ ] **Step 3: Confirm the template appears in the list**

```bash
dotnet new list nkkonsult-saas
```

Expected output shows `nkkonsult-saas` with name `Nkkonsult SaaS Template`.

- [ ] **Step 4: Generate a test project**

```bash
rm -rf /tmp/TestProjet && dotnet new nkkonsult-saas -n TestProjet -o /tmp/TestProjet
```

Expected: `The template "Nkkonsult SaaS Template" was created successfully.`

- [ ] **Step 5: Verify namespace substitution in generated .cs files**

```bash
grep -r "Nkkonsult" /tmp/TestProjet --include="*.cs" | head -5
```

Expected: **no output** (zero matches — all `Nkkonsult` replaced by `TestProjet`).

```bash
grep -r "TestProjet" /tmp/TestProjet --include="*.cs" | head -5
```

Expected: multiple matches like `namespace TestProjet.Web;`, `using TestProjet.Application.Common.Interfaces;`, etc.

- [ ] **Step 6: Verify appsettings.json substitutions**

```bash
cat /tmp/TestProjet/src/Web/appsettings.json
```

Expected: `"TestProjetDb"` as connection string key, `"testprojet"` as database name, `"testprojet-api"` as JWT Issuer, `"testprojet-clients"` as JWT Audience.

```bash
cat /tmp/TestProjet/src/Web/appsettings.Development.json
```

Expected: `"TestProjetDb"` key, `"testprojet_dev"` database, `"testprojet-api"` issuer, `"testprojet-clients"` audience.

- [ ] **Step 7: Verify DependencyInjection.cs substitution**

```bash
grep "ConnectionString\|Connection string" /tmp/TestProjet/src/Infrastructure/DependencyInjection.cs
```

Expected:
```
var connectionString = builder.Configuration.GetConnectionString("TestProjetDb");
Guard.Against.Null(connectionString, message: "Connection string 'TestProjetDb' not found.");
```

- [ ] **Step 8: Verify Shared project folder rename**

```bash
ls /tmp/TestProjet/src/
```

Expected: `TestProjet.Shared/` present, NO `Nkkonsult.Shared/` directory.

- [ ] **Step 9: Verify migrations are excluded**

```bash
ls /tmp/TestProjet/src/Infrastructure/ 2>/dev/null
```

Expected: no `Migrations/` directory in the generated output.

---

## Task 3: Verify the generated project builds

- [ ] **Step 1: Restore dependencies**

```bash
dotnet restore /tmp/TestProjet/src/Web/Web.csproj
```

Expected: `Restore succeeded.`

- [ ] **Step 2: Build the Web project**

```bash
dotnet build /tmp/TestProjet/src/Web/Web.csproj --no-restore
```

Expected: `Build succeeded.` with 0 errors (warnings about missing DB connection at runtime are OK — we're just compiling).

If build fails with namespace errors, check:
1. Are there any remaining `Nkkonsult` references? Run: `grep -r "Nkkonsult" /tmp/TestProjet/src --include="*.cs"`
2. Are `.csproj` references correct? Check `<ProjectReference>` paths in `/tmp/TestProjet/src/Web/Web.csproj`

- [ ] **Step 3: Clean up test output**

```bash
rm -rf /tmp/TestProjet
```

---

## Task 4: Commit final state

- [ ] **Step 1: Check git status from repo root**

```bash
cd c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate && rtk git status
```

- [ ] **Step 2: Stage and commit docs/plans**

```bash
cd c:/Users/Nathanael/GitRepos/NkkonsultSaaSTemplate && rtk git add docs/superpowers/plans/2026-04-06-dotnet-new-template.md && rtk git commit -m "docs: add dotnet new template implementation plan"
```

Expected: `1 file changed`.

---

## Self-Review: Spec Coverage Check

| Spec requirement | Covered by |
|---|---|
| `.template.config/template.json` at repo root | Task 1 |
| `Nkkonsult` → project name (namespaces) | `sourceName: "Nkkonsult"` in Task 1 |
| `NkkonsultDb` → `MonProjetDb` | `sourceName` substring match in Task 1 |
| `nkkonsult-api` → `monprojet-api` | `nameLower` derived symbol in Task 1 |
| `nkkonsult-clients` → `monprojet-clients` | `nameLower` derived symbol in Task 1 |
| `nkkonsult` (DB name) → `monprojet` | `nameLower` derived symbol in Task 1 |
| `Nkkonsult.Shared` folder rename | `sourceName` dir rename in Task 1 |
| Migrations excluded | `exclude` in sources/modifiers in Task 1 |
| `dotnet new install ./ && dotnet new nkkonsult-saas -n TestProjet` works | Task 2 |
| `dotnet build` succeeds on generated project | Task 3 |
| Commit in repo | Tasks 1 + 4 |
