# Publish to GitHub — Clearly Prepare Screen Addon

**Статус:** `ready`  
**GitHub:** Release + zip  
**Версия:** `1.1.2`  
**Deployment:** `(client_only)`

## 1. Подготовка (уже сделано этим скриптом)

Папка: `github-repos/ClearlyPrepareScreenAddon/`

## 2. Создать репозиторий и запушить

```powershell
cd github-repos/ClearlyPrepareScreenAddon
git init
git add .
git commit -m "Source backup Clearly Prepare Screen Addon v1.1.2"
git branch -M main
git remote add origin https://github.com/kabzon93region/ClearlyPrepareScreenAddon.git
git push -u origin main
```

Или автоматически:

```powershell
python CURSORAIMODING/tools/publish/publish_github_release.py ClearlyPrepareScreenAddon --create-repo
```

## 3. GitHub Release

Прикрепить zip (только игровые файлы, без INSTALL.md):

`\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\ClearlyPrepareScreenAddon_(client_only)_v1.1.2_2026-06-26.zip`

```powershell
gh release create v1.1.2 "\\Servant\data\Games\EscapeFromTarkov4\CURSORAIMODING\releases\ClearlyPrepareScreenAddon_(client_only)_v1.1.2_2026-06-26.zip" ^
  --title "Clearly Prepare Screen Addon v1.1.2" ^
  --notes-file CHANGELOG.md
```

## Описание репозитория (suggested)

Аддон к ClearlyPrepareScreen — UI матчмейкера.

SPT 4.0 + Fika 2.3 headless stack. Deployment: `(client_only)`.
