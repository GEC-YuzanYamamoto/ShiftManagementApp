# CalenShift（シフト管理アプリ）
<img src="https://github.com/user-attachments/assets/14c92894-0535-44ca-a64e-d31c77d9bd4f" width="auto">
従業員が希望シフトを提出し、管理者がシフトを確認・確定できるシフト管理システムです。

カレンダー表示でユーザーが使いやすいように工夫しました。

バックエンドは ASP.NET Core / .NET 8（Aspire構成）、フロントエンドは Blazor（Interactive Server）、データベースは PostgreSQL を使用しています。

------------------------------------------------------------------------

## ✨ 主な機能

### 👤 認証・ユーザー管理

-   初回セットアップ画面で初めにAdminユーザを追加
-   JWT によるログイン／ログアウト
-   管理者（Admin）と一般ユーザー（Staff）のロール分け
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

### 5.初回セットアップ

データベースにAdminユーザが存在しない場合だけ表示

/setupをURLに加えて、ページに表示される名前・メールアドレス・パスワード欄に入力

------------------------------------------------------------------------
## 🗄 データモデル概要

### Users

| Column        | Type   |
|---------------|--------|
| Id            | int    |
| Name          | string |
| Email         | string |
| PasswordHash  | string |
| Role          | byte   |

---

### ShiftRequests（希望シフト）

| Column     | Type     |
|------------|----------|
| Id         | int      |
| UserId     | int      |
| ShiftDate  | DateOnly |
| ShiftType  | byte     |
| Status     | byte     |
| CreatedAt  | DateTime |
| UpdatedAt  | DateTime |

---

### ShiftSchedules（確定シフト）

| Column     | Type     |
|------------|----------|
| Id         | int      |
| UserId     | int      |
| ShiftDate  | DateOnly |
| ShiftType  | byte     |
| ConfirmedAt | DateTime |
| ConfirmedBy | int      |

------------------------------------------------------------------------

## 🚀 今後の実装予定（ToDo）

-   [ ] カレンダー UI の改善
-   [ ] 通知機能（メール／アラート）
-   [ ] テストコード追加

------------------------------------------------------------------------

## 📄 ライセンス

MIT License
