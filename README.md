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
  "text": "Hi Patricia, please create an expense claim <expense><cost_centre>DEV632</cost_centre><total>35,000</total><payment_method>personal card</payment_method></expense>"
}
```

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

The extracted `<total>` is treated as a GST-inclusive amount.

```
GST rate:            15%
totalExcludingTax  = totalIncludingTax / 1.15
salesTax           = totalIncludingTax - totalExcludingTax
```

All currency values are rounded to 2 decimal places. The rounding approach - deriving `salesTax` from the rounded `totalExcludingTax` rather than rounding independently - ensures the two components always reconcile to the original total.

---

## AI Extension Point

The parser is intentionally deterministic. It does not use AI, and the challenge does not require it.

In a production workflow automation platform, an AI-assisted layer could be introduced *after* deterministic validation to support:

- workflow classification beyond simple rule matching
- missing field suggestions for human review
- natural language summarisation of the parsed result
- routing decisions based on extracted context

The response includes `"aiExtensionReady": true` to identify where this layer would integrate - after the deterministic pipeline has validated and structured the data - without compromising the reliability of the core parsing logic.

---

## Running Locally

### Prerequisites
- .NET 8 SDK
- Node.js 20+

### Backend

```bash
cd backend/Flowingly.Import.Api
dotnet restore
dotnet run --urls http://localhost:5000
```

API: http://localhost:5000  
Swagger: http://localhost:5000/swagger

### Frontend

```bash
cd frontend
npm install
npm run dev
```

Frontend: http://localhost:5173

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

Both the frontend dev server and backend API must be running first (see Running Locally above), then:

```bash
cd e2e
npm test
```

Tests cover:

- Full valid input flow - submits sample email, verifies JSON output with correct field values
- Missing `<total>` - verifies `MISSING_TOTAL` error is displayed and result panel is absent
- Clear button - verifies input and result are reset

---

## Design Decisions and Tradeoffs

**Deterministic parsing over regex flexibility**  
The parser uses an iterative innermost-first regex strategy rather than a full XML parser. This keeps the dependency footprint minimal and the behaviour predictable for the tag formats defined in the challenge. It handles both nested blocks (`<expense>...</expense>`) and inline tags correctly.

**Separation of parsing from validation**  
`MarkupParser` is responsible only for extraction and tag-pair integrity. It returns `null` fields for absent tags - it does not decide whether absence is an error. `ImportValidator` owns that decision. This makes each service independently testable and keeps responsibilities clear.

**`ResolveCostCentre` on the validator**  
The defaulting rule for `cost_centre` is a business rule, not a parsing concern. Placing `ResolveCostCentre` on `IImportValidator` keeps the rule co-located with the validation logic that governs it, without mutating the immutable `ParsedImportData` domain model.

**No mocking library in tests**  
All backend services are pure functions with no I/O. The application service tests use real implementations throughout, which exercises the full pipeline end-to-end and avoids the overhead of a mocking framework for a project of this scope.

**Plain CSS over a UI framework**  
The frontend uses plain CSS. The UI requirements are simple and well-defined - introducing a component library would add complexity without benefit for a focused challenge submission.

**HTTP 422 for validation failures**  
The API returns `422 Unprocessable Entity` for validation errors rather than `400 Bad Request`. The request is syntactically valid JSON - the content fails business rules, which is the semantic distinction `422` is designed for.

---

## Future Improvements

- Configurable tax rates (currently hardcoded at 15% GST)
- Support for additional field tags without code changes (e.g. a tag registry)
- Richer date normalisation across multiple input formats
- Enhanced workflow classification using additional field combinations
- AI-assisted review layer for ambiguous or incomplete inputs (see AI Extension Point above)
- CI pipeline for automated test execution on pull requests
- Integration test coverage for the full HTTP layer
