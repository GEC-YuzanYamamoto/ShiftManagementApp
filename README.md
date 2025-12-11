# CalenShift（シフト管理アプリ）
<img src="https://github.com/user-attachments/assets/14c92894-0535-44ca-a64e-d31c77d9bd4f" width="auto">
従業員が希望シフトを提出し、管理者がシフトを確認・確定できるシフト管理システムです。

バックエンドは ASP.NET Core / .NET 8（Aspire構成）、フロントエンドは Blazor（Interactive Server）、データベースは PostgreSQL を使用しています。

------------------------------------------------------------------------

## ✨ 主な機能

### 👤 認証・ユーザー管理

-   JWT によるログイン／ログアウト
-   管理者（Admin）と一般ユーザー（User）のロール分け
-   管理者のみユーザー一覧の閲覧・追加・削除

### 🗓 シフトカレンダー（一般ユーザー）

-   月ごとのシフトカレンダーを表示
-   希望シフト（勤務時間）の登録
-   登録済みシフト一覧表示
-   シフト確定時に希望シフト自動削除

### 🛠 シフト管理（管理者）

-   全ユーザーの希望シフト一覧取得
-   カレンダー上でユーザーごとの希望を確認
-   シフト確定（ShiftSchedules へ登録）
-   確定後の希望シフト削除

------------------------------------------------------------------------

## 🏗 技術スタック

| 領域       | 技術 |
|------------|-------|
| 言語       | C# / .NET 8 |
| フレームワーク | ASP.NET Core / Blazor |
| インフラ   | .NET Aspire |
| DB         | PostgreSQL（EF Core） |
| 認証       | JWT（Bearer Token） |
| UI         | Bootstrap / Blazor Components |


------------------------------------------------------------------------

## 📁 プロジェクト構成

    /ShiftManagement
     ├── ShiftApi.AppHost             // Aspire構成ルート
     ├── ShiftApi.ApiService          // Web API
     └── ShiftFrontend                // Blazor Frontend

------------------------------------------------------------------------

## ⚙️ 環境構築手順

### 1. リポジトリクローン

    git clone https://github.com/GEC-YuzanYamamoto/ShiftManagementApp.git
    cd ShiftManagementApp

### 2. PostgreSQL セットアップ

    CREATE DATABASE shift_db;

### 3. マイグレーション反映

    cd ShiftApi.ApiService
    dotnet ef database update

### 4. Aspire 起動

    cd ShiftApi.AppHost
    dotnet run

------------------------------------------------------------------------

## 🗄 データモデル概要（ER図）

![ER](https://github.com/user-attachments/assets/b4b39d30-20a6-4765-b959-fc0ed1400584)

------------------------------------------------------------------------

## 🚀 今後の実装予定（ToDo）

-   [ ] カレンダー UI の改善
-   [ ] 通知機能（メール／アラート）
-   [ ] テストコード追加

------------------------------------------------------------------------

## 📄 ライセンス

MIT License
