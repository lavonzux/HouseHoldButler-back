# 🏠 AI Housekeeper

*A self-hosted, open-source smart household inventory & reminder system*

---

## 🌟 Vision

**AI Housekeeper** aims to be a privacy-first, self-hostable “household butler” that helps individuals and families:

* Track household inventory and consumables
* Understand consumption and spending patterns
* Receive timely reminders for purchases and expiring items
* Reduce waste through smart, explainable AI suggestions
* Seamlessly integrate software, hardware, and real-life context

This project is designed with **FOSS**, **self-hosting**, and **modular extensibility** in mind — users fully control their data and deployment.

---

## 🧱 Tech Stack

### Frontend

* **React + vite**
* **Ant Design** (temporary choice, subject to change)

### Backend

* **C# / .NET Framework**
* RESTful API architecture

### Database

* **PostgreSQL**

### Infrastructure / Integration

* **Containerization**: Docker
* **SSO / Identity Provider**: Google Workspace, Authentik
* **Notification Channels**: SMTP, Ntfy, Gotify
* **LLM Providers**: Cloud-based or self-hosted (pluggable)

---

## ✨ Features

### Core Features

* **Inventory Tracker**

  * Item registration, quantity tracking
  * Consumption query & history
* **Purchase Reminder**

  * Low-stock alerts based on consumption patterns
* **Budget & Spending Tracking**

  * Category-based spending overview
  * Period-based budget monitoring

### Advanced Features

* **Expiring Reminder**

  * Time-based alerts for expiring items
* **LLM-based Suggestions**

  * Meal planning suggestions
  * Shopping list generation
  * Waste reduction tips with explainable reasoning
* **Family Household**

  * Multiple users per household
  * Shared inventory
  * Duplicate purchase prevention
* **Smartphone Support**

  * Push notifications
  * Location-based reminders
  * Time-based reminders
* **Barcode Scanner**

  * Quick item input
* **Hardware Integration**

  * ESP32 devices
  * Image recognition
  * Sensor-driven inventory updates

### Cross-cutting / System-level Features

* **Settings & Preferences**

  * User-level and household-level configuration
* **Role & Permission Model**

  * Owner / Admin / Member / Viewer
* **Audit & History Logs**

  * Track inventory changes and actions
* **Data Import / Export**

  * CSV / JSON for backup and portability

---

## 🛣️ Roadmap (Ordered by Implementation Difficulty)

> Difficulty ordering reflects **engineering complexity + system impact**, not business priority.

---

### Phase 1 — Core Inventory & Backend Foundation

**Difficulty:** ⭐⭐☆☆☆

**Features**

* Inventory CRUD
* Consumption tracking
* PostgreSQL schema & migrations
* REST API design

**Skills You’ll Learn**

* Backend API design (.NET)
* Relational data modeling
* Repository / service layer separation
* Database migration strategies

**Resume / Interview Keywords**

> RESTful API, PostgreSQL, Domain Modeling, Backend Architecture

---

### Phase 2 — Frontend Dashboard & UX

**Difficulty:** ⭐⭐⭐☆☆

**Features**

* Inventory list & detail views
* Add / edit / consume items
* Basic filtering & sorting

**Skills You’ll Learn**

* Vue component architecture
* State management & API integration
* UI/UX design for data-heavy apps
* Frontend-backend contract design

**Resume / Interview Keywords**

> Vue.js, SPA Architecture, API Integration, UX Design

---

### Phase 3 — Scheduling, Reminders & Notifications

**Difficulty:** ⭐⭐⭐⭐☆

**Features**

* Expiring reminder
* Purchase reminder
* Notification delivery (SMTP / Ntfy / Gotify)

**Skills You’ll Learn**

* Background jobs & schedulers
* Event-driven design
* External service integration
* Notification system design

**Resume / Interview Keywords**

> Background Jobs, Event Systems, Notification Architecture

---

### Phase 4 — Budgeting, Analytics & History

**Difficulty:** ⭐⭐⭐⭐☆

**Features**

* Spending tracking
* Budget monitoring
* Audit / history logs

**Skills You’ll Learn**

* Aggregation queries
* Time-series style data analysis
* Designing audit-safe systems
* Read vs write optimized data models

**Resume / Interview Keywords**

> Data Analytics, Audit Logs, Financial Data Modeling

---

### Phase 5 — LLM-based Intelligence

**Difficulty:** ⭐⭐⭐⭐⭐

**Features**

* Meal planning suggestions
* Smart shopping list generation
* Waste reduction tips
* Explainable AI output

**Skills You’ll Learn**

* LLM API integration
* Prompt engineering
* AI output validation & safety
* Cost control & caching strategies

**Resume / Interview Keywords**

> LLM Integration, Prompt Engineering, AI-assisted Systems

---

### Phase 6 — Family, Permissions & SSO

**Difficulty:** ⭐⭐⭐⭐⭐

**Features**

* Family household model
* Role-based access control
* SSO integration (Authentik / Google Workspace)

**Skills You’ll Learn**

* OAuth2 / OpenID Connect
* RBAC design
* Multi-tenant system modeling
* Identity provider integration

**Resume / Interview Keywords**

> OAuth2, RBAC, SSO Integration, Multi-user Systems

---

### Phase 7 — Mobile, Hardware & Context Awareness

**Difficulty:** ⭐⭐⭐⭐⭐⭐ (Very Advanced)

**Features**

* Smartphone notifications
* Location-based reminders
* ESP32 & sensor integration
* Image recognition

**Skills You’ll Learn**

* IoT communication patterns
* Context-aware system design
* Hardware-software integration
* Event ingestion pipelines

**Resume / Interview Keywords**

> IoT Integration, Context-aware Systems, Edge Devices

---

## 🚫 Non-Goals

To keep the project focused and maintainable, the following are **explicitly out of scope**:

* Social networking features (sharing with other households)
* E-commerce or affiliate marketing
* Cloud-only or vendor-locked deployments
* Premature microservice decomposition

---

## 📌 Philosophy

* **Privacy-first**
* **Self-hosting friendly**
* **Explainable AI**
* **Incremental complexity**
* **Real-life usefulness over feature bloat**
