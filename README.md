# Projekt4 – MVC + Identity + Rezerwacje

## Wymagania
- .NET SDK: `net8.0`
- Baza danych: SQL Server (LocalDB domyślnie)

## Konfiguracja
- Connection string: `appsettings.json` → `DefaultConnection`
- Opcje czyszczenia rezerwacji: `ReservationCleanup` w `appsettings.json`

## Uruchomienie
1. `dotnet restore`
2. `dotnet ef database update` lub aplikacja utworzy DB przy starcie
3. `dotnet run` w folderze `Projekt4`
4. Wejdź na `https://localhost:5001` lub `http://localhost:5000` (port może się różnić zgodnie z `launchSettings.json` lub parametrem `--urls`)


## Role i konta testowe
- Role: `User`, `Manager`
- Manager: email `manager@test.pl`, hasło `Manager123!`
- User: email `user@test.pl`, hasło `User123!`
- Seeding wykonywany automatycznie przy starcie aplikacji

## Walidacje klienta
- Widoki formularzy (`Room/Create`, `Room/Edit`, `Reservation/Create`) wczytują `_ValidationScriptsPartial.cshtml`
- Skrypty: `jquery.validate` + `jquery.validate.unobtrusive`

## Style i Bootstrap
- Bootstrap podłączony w `_Layout.cshtml`
- Ogólne style i nadpisania: `wwwroot/css/site.css`
- Uploady plików: `wwwroot/uploads`

## Podstawowe scenariusze
- Tworzenie pokoju i edycja parametrów
- Tworzenie rezerwacji z walidacją i przeglądanie szczegółów
- Lista rezerwacji: moje oraz wszystkie (dla managera)
- Logowanie, rejestracja, role (Manager ma dostęp do zarządzania pokojami i rezerwacjami)

## Migrations
- Katalog: `Projekt4/Migrations` zawiera pełny zestaw migracji dla Room/Reservation/Attachments
- Dodatkowe migracje Identity mogą znajdować się w `Projekt4/Data/Migrations`