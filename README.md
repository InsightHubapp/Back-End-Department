# 🎯 InsightHub — Career Guidance Platform
 
> **Graduation Project** | ASP.NET Core 10 · Entity Framework Core · SQL Server
 
InsightHub bridges the gap between computer science graduates and the real job market by combining psychometric assessment with empirical data from employed professionals — replacing abstract career advice with measurable, data-driven recommendations.
 
---
 
## 📌 Table of Contents
 
- [Overview](#overview)
- [Features](#features)
- [System Architecture](#system-architecture)
- [How It Works](#how-it-works)
- [Algorithm Details](#algorithm-details)
- [API Reference](#api-reference)
- [Data Models](#data-models)
- [Career Tracks](#career-tracks)
- [Tech Stack](#tech-stack)
- [Getting Started](#getting-started)
- [Testing](#testing)
---
 
## Overview
 
InsightHub targets **unemployed CS graduates and students** who struggle to choose a career path. Instead of generic personality tests, the platform:
 
1. Assesses aptitude across 10 tech tracks using 40 Yes/No questions
2. Compares the graduate's preferences against **real data** from employed professionals
3. Returns a **similarity score** showing how well the graduate fits each track's real-world environment
---
 
## Features
 
| Feature | Description |
|---|---|
| 🧠 Career Quiz | 40 Yes/No questions across 10 tech tracks |
| 📊 Market Similarity | Cosine-style slot similarity vs. employed professionals |
| 👥 Professional Persona | Behavioral benchmarks from real employees (teamwork, resilience, etc.) |
| 🏷️ Category Labels | Tracks grouped by industry category (IT, Creative, Engineering, Scientific) |
| 🔐 JWT Auth | Secure endpoints with role-aware question delivery |
| ♻️ Upsert Logic | Re-submissions update existing answers, no duplicates |
 
---
 
## System Architecture
 
```
┌─────────────────┐
│   Client App    │
│  (React/Mobile) │
└────────┬────────┘
         │ HTTP/JSON
         ▼
┌─────────────────────────────────────┐
│   ASP.NET Core Web API              │
│  ┌──────────────────────────────┐   │
│  │  CareerQuizController        │   │
│  │  - GET  /questions           │   │
│  │  - POST /full-match          │   │
│  └──────────┬───────────────────┘   │
│             ▼                       │
│  ┌──────────────────────────────┐   │
│  │  CareerQuizService           │   │
│  │  - CalculateTopTracks()      │   │
│  │  - TrackSimilarity()         │   │
│  │  - GetMarketInsights()       │   │
│  └──────────┬───────────────────┘   │
│             ▼                       │
│  ┌──────────────────────────────┐   │
│  │  Entity Framework Core       │   │
│  └──────────┬───────────────────┘   │
└─────────────┼───────────────────────┘
              ▼
    ┌──────────────────┐
    │   SQL Server     │
    └──────────────────┘
```
 
---
 
## How It Works
 
### User Journey
 
```
Graduate Registers
       ↓
Answers 50 Questions
  ├── Q111-120 → Shared Insights (Scale: work preferences & expectations)
  └── Q121-160 → Career Quiz (Yes/No: aptitude per track)
       ↓
Algorithm Processing
  ├── Score 10 tracks → Pick Top 3
  ├── Calculate Similarity vs. employed professionals (per track)
  └── Aggregate Market Insights (salary, environment, behavioral traits)
       ↓
Results Returned
  ├── Top 3 Career Tracks (ranked by aptitude %)
  ├── Similarity Score per Track (0-100%)
  └── Market Insights (real averages from employed users)
```
 
### Question Breakdown
 
| ID Range | Type | Audience | Purpose |
|---|---|---|---|
| 101–110 | Scale (1–5) | Employed only | Behavioral assessment |
| 111–120 | Scale / MultiChoice | Both | Shared work preferences |
| 121–160 | Yes/No | Unemployed | Career aptitude per track |
 
**Questions 113, 114, 115** use `MultiChoice` with explicit options:
 
| Q# | Question | Options |
|---|---|---|
| 113 | Preferred work environment | Remote · Office · Hybrid |
| 114 | Preferred company size | Startup · Mid-size · Corporate |
| 115 | Preferred role style | Technical · Managerial · Balanced |
 
---
 
## Algorithm Details
 
### Track Scoring
 
For each track (IDs 121–160, 4 questions per track):
 
```
Score      = count of "Yes" answers (answerValue = 1)
Percentage = (Score / 4) × 100
Top 3      = OrderByDescending(Percentage).ThenByDescending(Score).Take(3)
```
 
### Market Similarity
 
For each of the Top 3 tracks, compares the graduate's answers (Q111–120) against the average answers of employed professionals in that track:
 
```
For each question Q in [111-120]:
  TrackMean[Q]     = avg(employed users in track, answer to Q)
  MaxRange[Q]      = Q.MaxValue - 1
  SlotSimilarity   = 1 - min(|Graduate[Q] - TrackMean[Q]| / MaxRange[Q], 1)
 
FinalSimilarity = mean(SlotSimilarity[Q]) × 100
```
 
**Example:**
 
| Question | Graduate | Track Avg | Slot Similarity |
|---|---|---|---|
| Q116 – Technical Level | 4 | 4.5 | 87.5% |
| Q117 – Soft Skills | 3 | 4.0 | 75.0% |
| Q118 – Salary Satisfaction | 4 | 3.2 | 80.0% |
 
---
 
## API Reference
 
### `GET /api/careerquiz/questions`
 
Returns the question set based on user type.
 
| Parameter | Type | Description |
|---|---|---|
| `isEmployed` | `bool` (query) | `true` → Q101–120 · `false` → Q111–160 |
 
**Response:**
```json
[
  {
    "id": 113,
    "text": "Preferred work environment.",
    "type": "Choice",
    "options": [
      { "id": 1, "text": "Remote", "numericValue": 1 },
      { "id": 2, "text": "Office", "numericValue": 2 },
      { "id": 3, "text": "Hybrid", "numericValue": 3 }
    ]
  }
]
```
 
---
 
### `POST /api/careerquiz/full-match`
 
Submits answers and triggers the full matching pipeline.
 
**Request Body:**
```json
{
  "answers": [
    { "questionId": 111, "answerValue": 4 },
    { "questionId": 121, "answerValue": 1 },
    ...
  ]
}
```
 
**Response:**
```json
{
  "topTracks": [
    {
      "track": {
        "trackId": 2,
        "trackName": "Backend",
        "score": 4,
        "maxScore": 5,
        "percentage": 80.0
      },
      "trackSimilarityScore": 83.5,
      "similarityMessage": "",
      "marketInsights": {
        "totalEmployeesInTrack": 24,
        "avgTechnicalLevel": 4.2,
        "avgSoftSkills": 3.8,
        "avgSalarySatisfaction": 3.5,
        "avgWorkLifeBalance": 3.9,
        "mostCommonEnvironment": "Hybrid",
        "mostCommonCompanySize": "Mid-size",
        "avgYearsExperience": 2.7,
        "avgConsistency": 4.1,
        "avgAdaptability": 3.9,
        "avgTeamwork": 4.3,
        "avgProblemSolving": 4.5,
        "avgCommunication": 3.8,
        "avgResilience": 4.0
      }
    }
  ],
  "message": "Top track: Backend."
}
```
 
---
 
## Data Models
 
### Core Entities
 
```csharp
ApplicationUser   // ASP.NET Identity + IsEmployed + TrackId
Question          // Id, Text, Type, AppliesTo, MaxValue, TrackId, IsCareerQuiz
SurveyResponse    // UserId, QuestionId, AnswerValue
Track             // Id, Name, Description, RequiredSkills
CategoryLabel     // Id, Name → many-to-many with Track
QuestionOption    // QuestionId, Text, NumericValue (for MultiChoice questions)
```
 
### Question Types
 
| Enum Value | Usage |
|---|---|
| `Scale` | Slider 1–N (configurable MaxValue) |
| `YesNo` | Binary 0 or 1 |
| `MultiChoice` | Predefined options with NumericValue |
 
---
 
## Career Tracks
 
| ID | Track | Category |
|---|---|---|
| 2 | Backend | IT Jobs |
| 3 | Frontend | IT Jobs · Creative & Design |
| 4 | Mobile | IT Jobs |
| 5 | Game Dev | IT Jobs · Creative & Design |
| 6 | Cybersecurity | IT Jobs |
| 7 | Embedded | IT Jobs · Engineering |
| 8 | AI/ML | IT Jobs · Scientific & QA |
| 9 | QA/Testing | IT Jobs · Scientific & QA |
| 10 | Data Analysis | IT Jobs · Scientific & QA |
 
---
 
## Tech Stack
 
| Layer | Technology |
|---|---|
| Language | C# (.NET 10) |
| Framework | ASP.NET Core Web API |
| ORM | Entity Framework Core (Code-First) |
| Database | SQL Server |
| Auth | ASP.NET Identity + JWT Bearer |
| Architecture | Controller → Service → Repository pattern |
 
---
 
## Getting Started
 
### Prerequisites
 
- .NET 10 SDK
- SQL Server (local or Docker)
### Setup
 
```bash
# Clone the repo
git clone https://github.com/your-username/InsightHub.git
cd InsightHub
 
# Configure connection string in appsettings.json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=InsightHubDb;Trusted_Connection=True;"
}
 
# Apply migrations
dotnet ef database update
 
# Run the project (SeedData runs automatically on startup)
dotnet run
```
 
> The app seeds all tracks, category labels, questions (101–160), and question options automatically on first run.
 
---
 
## Testing
 
### Key Scenarios
 
| Scenario | Input | Expected |
|---|---|---|
| Happy Path | 50 valid answers | 3 tracks with similarity scores |
| Incomplete submission | Missing Q111–120 | `400 Bad Request` |
| No market data | No employed users in top track | `similarityScore = 0`, message = "No market data yet" |
| Duplicate submission | Same user submits twice | Latest values overwrite, no duplicates |
| All "No" answers | answerValue = 0 for all quiz Qs | Track score = 0%, excluded from top 3 |
 
### Performance Targets
 
| Endpoint | Target |
|---|---|
| `GET /questions` | < 200ms |
| `POST /full-match` | < 2000ms |
| DB queries per match | ≤ 5 |
 
---
 
## Version History
 
| Version | Highlights |
|---|---|
| 1.0 | Boolean-flag filtering, lenient validation, static score mapping |
| 2.0 | ID-range filtering (deterministic), strict 50-question contract, payload-driven scoring, 93% error reduction |
 
---
 
> **Graduation Project** — InsightHub Development Team · 2025
 
