# Listing Search

Full-stack search over property listings ingested from multiple MLS feeds: filter, rank by relevance, and page through results.

- **Backend:** ASP.NET Core Web API on .NET 10, Clean Architecture, controllers
- **Frontend:** React 19 + TypeScript, Vite, MUI, TanStack Query, axios, bun
- **Data:** the provided `sample_listings.json`, loaded and validated into memory at startup

## Quick start

### One command (Docker)

```bash
docker compose up --build
# open http://localhost:8080
```

That builds and starts both containers. The UI waits until the API reports ready.

| Service | Image | Role |
|---|---|---|
| `web` | nginx (unprivileged) + React build | Serves the UI; proxies `/api` and `/health` to `api`. The only port published (8080) |
| `api` | ASP.NET Core runtime | The search API. Internal network only, not reachable from the host |

Useful variants:

```bash
docker compose up --build -d --wait     # background; returns once both are healthy
WEB_PORT=9000 docker compose up --build # different host port
docker compose logs -f api              # follow API logs (structured JSON)
docker compose down                     # stop and remove
```

To change settings: `cp .env.example .env` and edit (see [Configuration](#configuration)).

### One command (local, hot reload)

Prerequisites: .NET SDK 10, bun 1.3+.

```bash
bun run setup    # first time only: root tools + git hooks, client packages, .NET restore
bun run dev      # API (dotnet watch) + UI (Vite) together; Ctrl+C stops both
# open http://localhost:5173
```

Output from both processes appears in one terminal, prefixed `[api]` and `[web]`. Saving a C# file hot-reloads the API, and saving a TS/TSX file hot-reloads the UI. Vite proxies `/api` to the API on port 5001, so the browser sees one origin, as with nginx in Docker.

- **Startup order:** the UI starts only once the API's `/health/ready` reports healthy (`wait-on`). An open browser tab can't hit a half-started API while it compiles.
- **If the API is briefly down** (e.g. `dotnet watch` restarting it after a change hot reload can't apply), the proxy answers `503` ProblemDetails. The UI treats that as temporary and retries by itself, and the terminal shows one line instead of a stack trace.

### Root commands

Run everything from the repository root:

| Command | What it does |
|---|---|
| `bun run setup` | Install and restore everything (idempotent) |
| `bun run dev` | API + UI with hot reload |
| `bun run dev:api` / `bun run dev:web` | Just one side |
| `bun run test` | Backend and frontend test suites in parallel |
| `bun run check` | Everything CI checks: `dotnet format`, typecheck, lint, Prettier, all tests |
| `bun run docker:up` / `bun run docker:down` | The Docker stack (same as `docker compose up --build` / `down`) |

### Tests and checks

```bash
bun run test     # 130 backend tests (unit + HTTP integration) and 72 frontend tests, in parallel
bun run check    # the full CI gate locally
```

Backend tests build in Release, so they never collide with the Debug API that `bun run dev` keeps running and locked. You can run tests, or push and trigger the pre-push hook, while developing.

## Configuration

Real values live in git-ignored `.env` files. The committed `.env.example` files document every setting, and every setting has a working default, so nothing needs configuring to run the app.

| File | Read by | Settings |
|---|---|---|
| `.env` (copy of `.env.example`) | Docker Compose | `WEB_PORT`, `RATE_LIMIT_PER_MINUTE`, `SCORING_BUDGET_WEIGHT`, `SCORING_RECENCY_WEIGHT`, `ASPNETCORE_ENVIRONMENT` |
| `client/.env.local` (copy of `client/.env.example`) | Vite | `API_URL`: dev proxy target. `VITE_API_BASE_URL`: API origin if not same-origin |
| `src/ListingSearch.Api/appsettings*.json` | ASP.NET Core | Defaults per environment; override any key with an env var, `Section__Key` (e.g. `Scoring__BudgetWeight`) |

- **Backend:** follows the .NET convention of appsettings plus environment variables, not `.env` files. Compose passes values through as environment variables.
- **`VITE_*` values are compiled into the public JavaScript bundle,** so they must never hold secrets. Local `.env` files are excluded from Docker build contexts so they can't leak into an image.
- **No secrets today.** If secrets are added: `dotnet user-secrets` locally, a secret store (Key Vault, AWS Secrets Manager, Kubernetes secrets) in production, never a committed file.

## Code quality gates

| Layer | Tooling |
|---|---|
| TypeScript | `strict` plus `noUncheckedIndexedAccess`, `exactOptionalPropertyTypes`, `noImplicitOverride`, `noPropertyAccessFromIndexSignature` |
| Lint | ESLint with typescript-eslint `strictTypeChecked` + `stylisticTypeChecked` (type-aware: floating promises, unsafe `any`, exhaustive switches), React Hooks, jsx-a11y (strict), import-x (`no-cycle` for barrels), Vitest, Testing Library. `../` imports are banned in favour of `@/`. Zero warnings allowed |
| Format | Prettier (frontend). `dotnet format` (backend) |
| .NET | Nullable enabled, warnings as errors, latest recommended analyzers, code style enforced in build |

**Git hooks (Husky).** `bun install` at the repo root installs them.

- **pre-commit** (seconds):
  - lint-staged runs ESLint `--fix` and Prettier on staged frontend files, and `dotnet format` on staged C# files. Fixes are re-staged automatically.
  - Then the frontend is type-checked as a whole, because a change in one file can break types in another.
- **pre-push:** `bun run check`, the same command you run by hand, so the hook and the manual check cannot drift apart.

CI runs the same checks, so a skipped hook (`--no-verify`) is still caught. `.gitattributes` pins LF line endings so the hooks work on every OS.

## API

`GET /api/listings/search`

| Parameter | Type | Notes |
|---|---|---|
| `minPrice`, `maxPrice` | decimal | Inclusive. `minPrice` must not exceed `maxPrice` |
| `minBedrooms` | int | Inclusive |
| `city` | string | Exact match, ignoring case and extra whitespace |
| `keyword` | string | Every word must start a word in the description |
| `targetBudget` | decimal | Enables budget-fit ranking; must be > 0 |
| `page` | int | 1-based, default 1 |
| `pageSize` | int | 1–100, default 10 |

| Status | When | Body |
|---|---|---|
| 200 | Valid query, including one with **no matches** | `{ items, page, pageSize, totalCount, totalPages }`, each item has `relevanceScore` |
| 400 | Any invalid parameter, including non-numeric values | `application/problem+json` with `errors` keyed by parameter name, all errors at once |
| 429 | Rate limit exceeded | ProblemDetails + `Retry-After` |
| 500 | Unexpected fault | Generic ProblemDetails with `traceId`; no internals leaked |

Also: `GET /health/live`, `GET /health/ready`, and the OpenAPI document at `/openapi/v1.json` in Development.

## Scoring

Score is 0–100, rounded to two decimals.

```
budgetFit = max(0, 1 − |price − targetBudget| / targetBudget)    1.0 at target, 0 at ±100% or further
recency   = 0.5 ^ (daysOnMarket / 30)                             1.0 today, 0.5 at 30 days, 0.25 at 60
score     = 100 × (0.7 × budgetFit + 0.3 × recency)               with targetBudget
score     = 100 × recency                                          without targetBudget
```

**Why this shape**

- **Budget dominates (70%).** A listing far outside the budget is not useful however new it is.
- **Linear budget fall-off.** Easy to explain: 10% off budget costs 10% of the budget component.
- **Half-life recency.** It never goes negative and has no arbitrary cut-off. Listings age smoothly instead of falling off a cliff at day N.
- **Weights and half-life are configuration** (`Scoring` section), validated at startup. Weights must sum to 1, so bad values stop the app instead of producing nonsense scores.

**Ties** are ordered deterministically: newer `listedDate`, then lower price, then `source`, then `id`. Sorting uses the rounded score, so listings that *look* tied on screen are ordered by these rules, and pages never shuffle between requests.

Example from the sample data (today = 2026-09-05, target $450k): A5 ($470k, listed yesterday) scores 96.20 and edges out A1 (exactly $450k, listed 7 days ago) at 95.52. A 4% price gap costs less than a week of age.

## Architecture

```
src/
  ListingSearch.Domain           Listing entity (invariants enforced on creation), ListingKey(source, id)
  ListingSearch.Application      Search use case: validate → filter → score → rank → paginate
                                 Ports: IListingRepository, IRelevanceScorer. Result<T> for expected failures
  ListingSearch.Infrastructure   JSON feed parser, in-memory repository, readiness health check
  ListingSearch.Api              Controllers, contracts (DTOs), ProblemDetails, rate limiting, security headers
tests/
  ListingSearch.UnitTests        Domain, scoring, validation, filtering, ranking, pagination, feed parsing
  ListingSearch.IntegrationTests Full HTTP pipeline via WebApplicationFactory, clock pinned
client/src/
  app/         Providers, MUI theme, React Query client (retry policy)
  api/         axios client, ApiError mapping, typed endpoint
  components/  Shared UI: Loading/Empty/Error states, VisuallyHidden
  lib/         Formatting
  features/listing-search/
    model/       Pure logic: form values, validation, URL ↔ params
    hooks/       useSearchUrlState, useSearchForm, useListingSearch (React Query)
    components/  SearchFilters, ResultsTable, ResultsPagination, SearchResults (presentational)
    ListingSearchPage.tsx   Container: wires hooks to components
```

Dependencies point inward: Api → Application → Domain, and Infrastructure → Application. Each frontend folder exposes a barrel (`index.ts`). Cross-module imports use `@/…` absolute paths through those barrels; files in the same folder import each other with `./`.

## User experience

- **Light / System / Dark theme** in the header. "System" follows the OS live, and the choice is remembered. Colours are CSS variables, so switching repaints without re-rendering React.
  - **No flash of the wrong theme:** `public/theme-init.js` sets the theme class before first paint. It is a same-origin file rather than MUI's inline script, so the Content-Security-Policy can keep forbidding inline scripts.
- **Filters:**
  - Two compact rows with input icons (`$`, search, location, bed) and a hint explaining what the target budget does.
  - Errors appear under the field that caused them, from client checks or the server.
- **Active filters as chips:** each one can be removed with a click or the Delete key. **Clear all** keeps your page size, because that is a viewing preference.
- **Results toolbar:** the result count, how results are ranked (by budget fit, or recency only), and the page size.
- **Results:**
  - **Wide screens:** a table with the price emphasised, a status chip, and a score bar with a tooltip explaining the formula.
  - **Phones:** cards instead of a nine-column table.
- **Loading:**
  - **First load:** a skeleton shaped like the results.
  - **Later pages:** the current results stay visible but dimmed, with a progress bar, so the layout never jumps.
  - **On page change:** the results scroll into view, instantly when the OS asks for reduced motion.
- **Empty states:** an icon and a **Clear filters** action when filters caused the empty result. A page past the end offers **Go to last page**.
- **Accessibility:**
  - A skip link and landmarks.
  - Labelled inputs with errors announced alongside them.
  - Result changes announced to screen readers.
  - Every control works by keyboard and has an accessible name.

## Decisions and edge cases

| Case | Behaviour |
|---|---|
| `minPrice > maxPrice`, `pageSize ≤ 0` or `> 100`, `page < 1`, negative values, `targetBudget ≤ 0` | 400, all errors reported together |
| Non-numeric or overflowing values (`minPrice=abc`, `page=99999999999`) | 400 from model binding, same error shape |
| Keyword of only punctuation (`!!!`) | 400. It would otherwise silently behave like "no keyword" |
| City or filters with no matches | 200 with `totalCount: 0`; the UI shows an explicit empty state |
| Page beyond the last | 200 with empty `items` and correct `totalPages`; the UI offers "Go to last page" |
| `page=2147483647` | No overflow; offsets are computed in 64-bit |
| Empty query values (`minPrice=`) | Treated as not supplied |
| Same `id` in two feeds | Different listings; identity is `source + id` |
| Bad record in the feed | Rejected with a logged reason; other records still load |
| Missing or empty data file, invalid scoring config | App refuses to start |
| Listing dated in the future | Recency clamps to "today" |
| Client disconnects mid-request | Cancellation flows through; logged as debug (499), not as an error |
| Slow older response arriving after a newer one (UI) | Cannot overwrite it: React Query caches per search and aborts requests nobody is waiting for |
| Hand-edited URL with invalid values (UI) | Errors shown next to the fields; nothing is sent |
| Date display in US time zones | Formatted in UTC, so `2026-08-29` never shows as Aug 28 |

## Assumptions

- **All statuses are searchable** (active, pending, sold); status is shown on each result. The handout lists no status filter.
- **Cross-feed duplicates are not merged.** The sample contains the same property from MLS_A and MLS_B ("123 Main St, Apt 4B" vs "123 Main Street, Unit 4B"). Merging is a product decision; the pipeline has a clear place for it (below).
- **City is an exact match** (ignoring case and spacing), not a substring, so "Springfield" does not match "West Springfield".

## Trade-offs and known limitations

- **Keyword matching is lexical.** "pets" matches "No pets" and "pets not permitted". Fixing this needs negation handling or a structured `petsAllowed` field at ingestion, not cleverer substring logic.
- **Budget fit is symmetric.** Being 10% under budget scores the same as 10% over; many buyers would prefer under. This is a one-line change in `BudgetRecencyScorer`.
- **Keyword relevance does not affect the score**; it only filters.
- **In-process filtering.** Fine for this dataset. With a database, filters and paging move into the query behind the same `IListingRepository` port.
- **Rate limiting is per instance.** With several API replicas, use a distributed limiter or the gateway's.
- **The dataset is baked into the API image.** Fine for a fixed sample. Real feeds would be mounted or come from a data store.
- **CSP allows inline styles** because MUI's styling engine (Emotion) injects `<style>` tags. Scripts remain restricted to `'self'`.

## Production notes

- **Two images:** `src/ListingSearch.Api/Dockerfile` (built from the repo root) and `client/Dockerfile`. Both are multi-stage, run as non-root, and have health checks.
- **One origin for the browser.** nginx serves the UI and proxies the API, so no CORS is needed. Hashed assets are cached for a year, and `index.html` is always revalidated, so a deploy takes effect immediately.
- **Hardening in compose:** read-only root filesystems (with a `/tmp` tmpfs), `no-new-privileges`, and the API port not published.
- **Client IPs:** the API trusts `X-Forwarded-*` (`ASPNETCORE_FORWARDEDHEADERS_ENABLED=true`), so rate limiting sees the real client IP. This is safe only because the API is reachable solely through nginx.
- **Probes:** `/health/live` for liveness, `/health/ready` for readiness, and `/healthz` for nginx itself.
- **Configuration:** API settings come from environment variables, e.g. `Scoring__BudgetWeight`, `RateLimiting__PermitLimit`, `ListingData__FilePath`, `AllowedHosts`.
- **Logs** are structured JSON in Production. Every error response carries a `traceId` that matches the server log.
- **CI** (`.github/workflows/ci.yml`) runs the backend tests and the frontend lint, typecheck, tests and build. It then brings the stack up with `docker compose` and smoke-tests the UI and the API through nginx.
- **TLS** is expected to terminate at the load balancer or ingress in front of `web`.

## What I would do next

1. Cross-feed de-duplication as a pipeline step: normalise address and coordinates, then keep the best record (or merge) per property.
2. A status filter, defaulting to active.
3. Negation-aware keyword handling, or structured amenity fields extracted at ingestion.
4. A real data store with indexes on city and price, and ingestion as a separate process.
5. Authentication, metrics (OpenTelemetry), and a distributed rate limiter.
