# Flowingly Software Development Challenge

A full stack web application that parses structured data from semi-structured email content, validates required fields, calculates tax values, and returns a clean JSON representation.

---

## Overview

The application accepts a block of pasted text, extracts XML-style tagged fields, applies validation rules, calculates GST, and displays the result as formatted JSON. Validation errors are surfaced clearly when required fields are missing or malformed.

The implementation prioritises deterministic behaviour, strict validation rules, and separation of concerns - ensuring the system is predictable, testable, and straightforward to extend.

---

## Tech Stack

### Backend
- .NET 8 / ASP.NET Core Web API
- C#
- xUnit + FluentAssertions
- Swagger / OpenAPI

### Frontend
- React 19 + TypeScript
- Vite
- Plain CSS

### Testing
- Backend: xUnit unit tests
- End-to-end: Playwright

### Runtime
- Docker + Docker Compose
- Nginx (frontend production container)

---

## Why .NET 8?

ASP.NET Core Web API on .NET 8 is a mature LTS platform widely used in production SaaS environments. It provides strong support for dependency injection, clean API design, and testability without ceremony. The focus of this challenge is maintainable, testable code - .NET 8 delivers that without requiring the newest runtime.

---

## Why React + TypeScript?

React and TypeScript align with modern frontend development practices and reflect the kind of stack used in production workflow automation tools. The frontend separates view, state, and API concerns across components, hooks, and a service layer - keeping each piece focused and independently testable.

---

## Architecture

### Backend pipeline

```
ImportController
  └── ImportApplicationService
        ├── MarkupParser           - extracts XML-style tags, detects unmatched pairs
        ├── ImportValidator        - applies business rules, resolves cost centre default
        ├── TaxCalculator          - computes GST-inclusive breakdown
        ├── WorkflowInsightBuilder - classifies workflow type
        └── Response DTO mapping
```

Each service has a single responsibility and is injected via interface. The application service orchestrates the pipeline linearly - parse, validate, calculate, classify, map.

### Frontend structure

```
App.tsx
  ├── TextInputPanel   - textarea, Submit, Clear, loading state
  ├── ResultPanel      - formatted JSON output
  └── ErrorPanel       - validation errors and network errors

hooks/useImportParser  - manages state and API calls
services/importApi     - isolated fetch wrapper
types/importTypes      - TypeScript contracts matching the API response
```

---

## API

### Endpoint

```
POST /api/import/parse
```

### Request

```json
{
  "text": "Hi Patricia, please create an expense claim <expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>",
  "taxRatePercent": 15
}
```

`taxRatePercent` is optional. When omitted or null, the rate defaults to 15.

### Success response

```json
{
  "success": true,
  "data": {
    "costCentre": "DEV632",
    "totalIncludingTax": 35000.00,
    "totalExcludingTax": 30434.78,
    "salesTax": 4565.22,
    "paymentMethod": "personal card",
    "vendor": null,
    "description": null,
    "date": null
  },
  "metadata": {
    "parser": "deterministic",
    "workflowClassification": "expense_claim",
    "aiExtensionReady": true
  },
  "errors": []
}
```

### Validation error response

```json
{
  "success": false,
  "data": null,
  "metadata": {
    "parser": "deterministic",
    "workflowClassification": "unknown",
    "aiExtensionReady": true
  },
  "errors": [
    {
      "code": "MISSING_TOTAL",
      "message": "The required <total> field was not found."
    }
  ]
}
```

---

## Validation Rules

| Condition | Behaviour | Error code |
|---|---|---|
| Opening tag with no matching closing tag | Reject entire message | `UNMATCHED_TAG` |
| `<total>` absent or blank | Reject entire message | `MISSING_TOTAL` |
| `<total>` present but not a valid positive number | Reject entire message | `INVALID_TOTAL` |
| `<cost_centre>` absent or blank | Accept - default to `UNKNOWN` | - |

---

## Tax Calculation

The extracted `<total>` is treated as a tax-inclusive amount. The tax rate defaults to **15%** but can be overridden per request via the UI or the API.

```
tax rate:            configurable (default 15%)
totalExcludingTax  = totalIncludingTax / (1 + rate)
salesTax           = totalIncludingTax - totalExcludingTax
```

All currency values are rounded to 2 decimal places. The rounding approach - deriving `salesTax` from the rounded `totalExcludingTax` rather than rounding independently - ensures the two components always reconcile to the original total.

The UI exposes a **Tax rate (%)** field in the input panel, pre-filled to 15. Changing it before submitting applies that rate to the calculation. The API accepts the rate as an optional `taxRatePercent` field in the request body (a percentage value, e.g. `10` for 10%). When omitted or null, 15% is used.

---

## AI Extension Point

The parser is intentionally deterministic. It does not use AI, and the challenge does not require it. That is the right call for a data extraction pipeline where correctness and predictability matter most - an AI model should never be the thing that decides whether a `<total>` tag is present or a number is valid.

Where AI genuinely adds value is in the layer *above* deterministic validation - handling the ambiguous, incomplete, and unstructured cases that rule-based systems cannot cover. The pipeline is designed with this in mind.

### Where AI fits in the pipeline

```
Raw email text
      |
      v
MarkupParser          - deterministic: extract tags, detect structural errors
      |
      v
ImportValidator       - deterministic: enforce business rules, reject invalid data
      |
      v
[AI Enrichment]       - probabilistic: fill gaps, classify intent, suggest corrections
      |
      v
TaxCalculator         - deterministic: arithmetic on validated data
      |
      v
WorkflowInsightBuilder - deterministic + AI-assisted: route to correct workflow
      |
      v
Response
```

The `aiExtensionReady: true` flag in every response marks the handoff point. Downstream consumers can use it to decide whether to invoke an AI enrichment step.

### Concrete integration points

**1. Missing field recovery**
When `cost_centre` is absent, the current behaviour defaults to `UNKNOWN`. An Azure OpenAI call with the raw email text and a structured prompt could infer the cost centre from context - department names, project references, sender patterns - and return a suggested value with a confidence score for human review rather than a hard default.

**2. Richer workflow classification**
`WorkflowInsightBuilder` currently classifies as `expense_claim` or `unknown` based on whether `<total>` is present. With an AI layer, the full email body could be passed to a classification prompt to distinguish between expense claims, purchase orders, reimbursement requests, and vendor invoices - enabling smarter downstream routing in a workflow automation platform.

**3. Ambiguous input handling**
Emails that partially match the expected format - fields present but not tagged, amounts written in prose, dates in non-standard formats - currently fail validation. An AI pre-processing step could attempt to normalise these inputs into the tagged format before the deterministic parser runs, dramatically increasing the range of inputs the system can handle without sacrificing the reliability of the core pipeline.

**4. Natural language summarisation**
The structured `ParsedDataDto` response could be passed to a completion model to generate a plain-English summary for display in a workflow task - "William Steele has submitted an expense claim of $35,000 for a team dinner at Seaside Steakhouse, charged to cost centre DEV632" - reducing the cognitive load on approvers.

### Implementation approach with Azure OpenAI

The enrichment layer would be introduced as a new service behind an interface, injected into `ImportApplicationService` between validation and tax calculation:

```csharp
public interface IAiEnrichmentService
{
    Task<EnrichmentResult> EnrichAsync(ParsedImportData data, string rawText);
}
```

This keeps the deterministic pipeline intact and makes the AI layer independently testable, replaceable, and opt-in. The Azure OpenAI SDK for .NET (`Azure.AI.OpenAI`) integrates cleanly with ASP.NET Core's DI container and supports both chat completions and structured output via JSON mode - which is the right approach for extracting typed field suggestions rather than free-form text.

Feature flags or a per-request `enableAiEnrichment` parameter would allow the AI layer to be toggled without redeployment, which is important in a production workflow platform where reliability guarantees need to be maintained independently of AI availability.

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Node.js 20+

### Quick start

**Windows (PowerShell):**
```powershell
.\start.ps1
```
Opens the API and frontend as separate tabs in the current Windows Terminal window. Falls back to separate windows if Windows Terminal is not available.

**Mac/Linux:**
```bash
chmod +x start.sh
./start.sh
```
Runs both servers in the same terminal. Ctrl+C stops both.

**VS Code:**
Run the `Start Dev Servers` task (`Ctrl+Shift+B` / `Cmd+Shift+B`) to launch both servers in the integrated terminal panel.

| Service | URL |
|---|---|
| Frontend | http://localhost:5173 |
| API | http://localhost:5000 |
| Swagger | http://localhost:5000/swagger |

### Manual start

If you prefer to start each server individually:

**Backend**
```bash
cd backend/Flowingly.Import.Api
dotnet restore
dotnet run --urls http://localhost:5000
```

**Frontend**
```bash
cd frontend
npm install
npm run dev
```

The Vite dev server proxies `/api` requests to `http://localhost:5000` automatically.

---

## Running with Docker

```bash
docker compose up --build
```

| Service | URL |
|---|---|
| Frontend | http://localhost:8080 |
| API | http://localhost:5000 |

The frontend Nginx container proxies `/api` requests to the backend service. No additional configuration is required.

---

## Live Demo

| Service | URL |
|---|---|
| Frontend | https://ambitious-meadow-06be83200.7.azurestaticapps.net |
| API | https://flowingly-api.politegrass-0f143e86.australiaeast.azurecontainerapps.io |

The application is hosted on Azure using Azure Static Web Apps (frontend) and Azure Container Apps (API). Every push to `main` triggers the GitHub Actions workflow which rebuilds and redeploys both services automatically. See `infra/README.md` for setup instructions.

---

## Running Backend Tests

```bash
cd backend
dotnet test
```

Tests cover:

- Tag extraction (total, cost_centre, payment_method, vendor, description, date)
- Comma-formatted total preserved as raw string
- Unmatched opening tag detection
- Missing and blank total validation
- Invalid total validation (non-numeric, zero, negative)
- Cost centre defaulting to `UNKNOWN`
- GST calculation correctness and rounding
- Full pipeline integration (success, parser failure, validation failure)

---

## Running E2E Tests

```bash
cd e2e
npm test
```

Playwright will start the backend API and frontend dev server automatically before the tests run and stop them afterwards. If either server is already running, it will be reused.

Tests cover:

- Full valid input flow - submits sample email, verifies JSON output with correct field values
- Missing `<total>` - verifies `MISSING_TOTAL` error is displayed and result panel is absent
- Clear button - verifies input and result are reset

---

## Design Decisions and Tradeoffs

**Playwright over Selenium for E2E tests**
Playwright was chosen over Selenium for several practical reasons. It has first-class TypeScript support and ships its own test runner, assertion library, and browser binaries as a single `npm` package — no separate WebDriver binaries, no version-matching friction. The `webServer` config lets Playwright start and stop both the API and frontend automatically, so `npm test` is a single command with no manual setup. Playwright's auto-waiting model eliminates most of the explicit `wait` calls that make Selenium tests brittle. For a React + Vite frontend, it is the more natural fit and the direction the industry has moved for modern web E2E testing.

**Deterministic parsing over regex flexibility**  
The parser uses an iterative innermost-first regex strategy rather than a full XML parser. This keeps the dependency footprint minimal and the behaviour predictable for the tag formats defined in the challenge. It handles both nested blocks (`<expense>...</expense>`) and inline tags correctly.

**Separation of parsing from validation**  
`MarkupParser` is responsible only for extraction and tag-pair integrity. It returns `null` fields for absent tags - it does not decide whether absence is an error. `ImportValidator` owns that decision. This makes each service independently testable and keeps responsibilities clear.

**`ResolveCostCentre` on the validator**  
The defaulting rule for `cost_centre` is a business rule, not a parsing concern. Placing `ResolveCostCentre` on `IImportValidator` keeps the rule co-located with the validation logic that governs it, without mutating the immutable `ParsedImportData` domain model.

**`<expense>` is treated as a structural container, not a data field**  
The parser exempts `<expense>` from the unmatched opening tag check. It acts as a grouping wrapper - like a root XML element - and carries no extractable value itself. Rejecting a message because the `<expense>` wrapper is unmatched would be overly strict: all inner fields can still be extracted successfully regardless of whether the wrapper is closed. If a future requirement introduces multiple expense blocks in a single message, the last-value-wins extraction rule already handles that gracefully, and the container tag list in `MarkupParser` can be extended without touching any other logic.

**No mocking library in tests**  
All backend services are pure functions with no I/O. The application service tests use real implementations throughout, which exercises the full pipeline end-to-end and avoids the overhead of a mocking framework for a project of this scope.

**Plain CSS over a UI framework**  
The frontend uses plain CSS. The UI requirements are simple and well-defined - introducing a component library would add complexity without benefit for a focused challenge submission.

**HTTP 422 for validation failures**  
The API returns `422 Unprocessable Entity` for validation errors rather than `400 Bad Request`. The request is syntactically valid JSON - the content fails business rules, which is the semantic distinction `422` is designed for.

---

## Future Improvements

- Support for additional field tags without code changes (e.g. a tag registry)
- Richer date normalisation across multiple input formats
- AI enrichment layer using Azure OpenAI for missing field recovery, workflow classification, and ambiguous input handling (see AI Extension Point above)
- CI pipeline for automated test execution on pull requests
- Integration test coverage for the full HTTP layer
