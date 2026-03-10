
# 🏠 AI Housekeeper

**一個可自架（self-hosted）、開源的智慧家庭庫存與提醒系統**

---

## 🌟 專案願景（Vision）

**AI Housekeeper** 的目標是打造一個**隱私優先、可自架的「家庭管家（Household Butler）」系統**，協助個人與家庭：

* 追蹤家庭中的物品庫存與消耗品
* 理解日常消耗與支出模式
* 即時收到補貨與物品即將過期的提醒
* 透過可解釋的 AI 建議來減少浪費
* 無縫整合軟體、硬體與真實生活情境

本專案以 **FOSS（自由與開源軟體）**、**自架友善（self-hosting）** 與 **模組化擴充** 為核心設計理念，使用者可以完全掌控自己的資料與部署方式。

---

## 🧱 技術棧（Tech Stack）

### 前端（Frontend）

* **React + Vite**
* **Ant Design**（暫定，未來可能更換）

### 後端（Backend）

* **C# / .NET Framework**
* RESTful API 架構

### 資料庫（Database）

* **PostgreSQL**

### 基礎設施 / 整合（Infrastructure / Integration）

* **容器化**：Docker
* **SSO / 身分識別系統**：Google Workspace、Authentik
* **通知管道**：SMTP、Ntfy、Gotify
* **LLM 提供者**：雲端或自架模型（可插拔式設計）

---

## ✨ 功能（Features）

### 核心功能（Core Features）

* **庫存追蹤（Inventory Tracker）**

  * 物品註冊與數量追蹤
  * 消耗查詢與歷史紀錄
* **購買提醒（Purchase Reminder）**

  * 根據消耗模式發送庫存不足提醒
* **預算與支出追蹤（Budget & Spending Tracking）**

  * 依分類的支出總覽
  * 依週期（週 / 月）的預算監控

---

### 進階功能（Advanced Features）

* **過期提醒（Expiring Reminder）**

  * 依時間觸發的物品過期通知
* **AI（LLM）智慧建議**

  * 餐點規劃建議
  * 智慧購物清單產生
  * 降低浪費的建議，並附帶可解釋原因
* **家庭模式（Family Household）**

  * 每個家庭可有多位使用者
  * 共用庫存管理
  * 防止重複購買
* **智慧型手機支援（Smartphone Support）**

  * 推播通知
  * 位置型提醒
  * 時間型提醒
* **條碼掃描（Barcode Scanner）**

  * 快速新增物品
* **硬體整合（Hardware Integration）**

  * ESP32 裝置
  * 影像辨識
  * 感測器驅動的庫存更新

---

### 橫切 / 系統層級功能（Cross-cutting / System-level Features）

* **設定與偏好（Settings & Preferences）**

  * 使用者層級與家庭層級設定
* **角色與權限模型（Role & Permission Model）**

  * Owner / Admin / Member / Viewer
* **稽核與歷史紀錄（Audit & History Logs）**

  * 追蹤庫存變更與使用者操作
* **資料匯入 / 匯出（Data Import / Export）**

  * CSV / JSON，用於備份與資料可攜性

---

## 🛣️ 開發路線圖（Roadmap，依實作難度排序）

> 難度排序反映的是 **工程複雜度 + 對整體系統的影響程度**，而非商業優先順序。

---

### Phase 1 — 核心庫存與後端基礎

**難度：** ⭐⭐☆☆☆

**功能**

* 庫存 CRUD
* 消耗紀錄追蹤
* PostgreSQL Schema 與 Migration
* REST API 設計

**可學習技能**

* 後端 API 設計（.NET）
* 關聯式資料建模
* Repository / Service 分層設計
* 資料庫 migration 策略

**履歷 / 面試關鍵字**

> RESTful API、PostgreSQL、Domain Modeling、Backend Architecture

---

### Phase 2 — 前端儀表板與 UX

**難度：** ⭐⭐⭐☆☆

**功能**

* 庫存清單與詳細頁
* 新增 / 編輯 / 消耗物品
* 基本篩選與排序

**可學習技能**

* Vue 元件架構設計
* 狀態管理與 API 整合
* 資料導向介面的 UI/UX 設計
* 前後端契約（contract）設計

**履歷 / 面試關鍵字**

> Vue.js、SPA Architecture、API Integration、UX Design

---

### Phase 3 — 排程、提醒與通知系統

**難度：** ⭐⭐⭐⭐☆

**功能**

* 過期提醒
* 補貨提醒
* 通知發送（SMTP / Ntfy / Gotify）

**可學習技能**

* 背景排程任務
* 事件驅動設計
* 第三方服務整合
* 通知系統架構設計

**履歷 / 面試關鍵字**

> Background Jobs、Event Systems、Notification Architecture

---

### Phase 4 — 預算、分析與歷史紀錄

**難度：** ⭐⭐⭐⭐☆

**功能**

* 支出追蹤
* 預算監控
* 稽核 / 歷史紀錄

**可學習技能**

* 聚合查詢（Aggregation）
* 類時間序列資料分析
* 可稽核系統設計
* 讀寫效能導向的資料模型

**履歷 / 面試關鍵字**

> Data Analytics、Audit Logs、Financial Data Modeling

---

### Phase 5 — AI（LLM）智慧層

**難度：** ⭐⭐⭐⭐⭐

**功能**

* 餐點規劃建議
* 智慧購物清單
* 降低浪費建議
* 可解釋的 AI 輸出

**可學習技能**

* LLM API 整合
* Prompt Engineering
* AI 輸出驗證與安全性
* 成本控管與快取策略

**履歷 / 面試關鍵字**

> LLM Integration、Prompt Engineering、AI-assisted Systems

---

### Phase 6 — 家庭、多使用者、SSO

**難度：** ⭐⭐⭐⭐⭐

**功能**

* 家庭資料模型
* 角色式存取控制（RBAC）
* SSO 整合（Authentik / Google Workspace）

**可學習技能**

* OAuth2 / OpenID Connect
* RBAC 設計
* 多租戶系統建模
* 身分識別系統整合

**履歷 / 面試關鍵字**

> OAuth2、RBAC、SSO Integration、Multi-user Systems

---

### Phase 7 — 行動裝置、硬體與情境感知

**難度：** ⭐⭐⭐⭐⭐⭐（非常進階）

**功能**

* 手機通知
* 位置型提醒
* ESP32 與感測器整合
* 影像辨識

**可學習技能**

* IoT 通訊模式
* 情境感知系統設計
* 軟硬體整合
* 事件攝取（event ingestion）管線

**履歷 / 面試關鍵字**

> IoT Integration、Context-aware Systems、Edge Devices

---

## 🚫 非目標（Non-Goals）

為了維持專案聚焦與可維護性，以下功能**明確不在範圍內**：

* 社交功能（跨家庭分享）
* 電商或聯盟行銷
* 僅支援雲端、無法自架的部署模式
* 過早的微服務拆分

---

## 📌 設計哲學（Philosophy）

* **隱私優先（Privacy-first）**
* **自架友善（Self-hosting friendly）**
* **可解釋的 AI（Explainable AI）**
* **循序漸進的複雜度（Incremental complexity）**
* **實際生活價值優先，避免功能膨脹**
