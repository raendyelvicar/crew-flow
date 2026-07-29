# Crew Flow

A community management platform for a dance studio — landing page + CMS, member management (with rich community profiles), activity scheduling, class booking with waitlists, and Stripe-backed memberships/credit packs.

## Stack

- **Frontend**: Next.js 16 (App Router, TypeScript), Tailwind CSS v4, shadcn/ui (Base UI), NextAuth v5
- **Backend**: ASP.NET Core 9 Web API, Onion Architecture (Domain / Application / Infrastructure / Api)
- **Database**: PostgreSQL 18, EF Core / Npgsql
- **Payments**: Stripe (subscriptions + one-time credit-pack purchases)
- **Auth**: ASP.NET Identity + JWT (access/refresh) issued by the API; Google OAuth via NextAuth exchanging its token for the API's own JWT

## Project layout

```
crew-flow/
├── api/                      .NET solution
│   ├── src/
│   │   ├── CrewFlow.Domain/          entities, enums — no external deps
│   │   ├── CrewFlow.Application/     services, DTOs, interfaces for infra
│   │   ├── CrewFlow.Infrastructure/  EF Core, Identity, Stripe, JWT, seeding
│   │   └── CrewFlow.Api/             controllers, Program.cs
│   └── tests/
├── web/                       Next.js app
│   ├── app/
│   │   ├── (marketing)/       public site — CMS-driven, pricing, community directory
│   │   ├── (auth)/            login / register
│   │   ├── (portal)/          member area — dashboard, classes, bookings, membership
│   │   └── admin/             back office — members, schedule, cashflow, CMS editor
│   ├── components/            shared UI + shadcn/ui primitives
│   └── lib/                   auth.ts (NextAuth config), api-client.ts, types.ts
└── docker-compose.yml
```

## Roles

- **Admin** — every feature
- **Finance** — cashflow, membership plans, subscriptions, credit packs
- **Operational** — activities, class schedules, occurrences, instructors, roster/check-in, members
- **Member** — the single community-facing role (books classes, manages own subscription/credits; a member with no active subscription just books drop-in via credit packs)

## Running locally

1. Copy the env file and adjust secrets as needed:
   ```bash
   cp .env.example .env
   ```
2. Start everything:
   ```bash
   docker compose up -d --build
   ```
   This builds and starts Postgres, the API (auto-migrates + seeds on startup), and the web app.
3. Open:
   - Marketing site: http://localhost:3000
   - API health check: http://localhost:5080/health
   - API Swagger (dev only): http://localhost:5080/swagger

A default Admin account is seeded on first run:
- **Email**: `admin@crewflow.dev`
- **Password**: `ChangeMe123!`

Five sample dance styles (Salsa, Bachata, Hip-Hop, Contemporary, Ballet) are also seeded.

### Running the API outside Docker

`api/src/CrewFlow.Api/appsettings.Development.json` points at `localhost:5432` so you can also run:
```bash
docker compose up -d postgres
cd api && dotnet run --project src/CrewFlow.Api
```

### Running the web app outside Docker

Requires **Node 20.9+** (Next.js 16 no longer supports Node 18). An `.nvmrc` is provided:
```bash
cd web && nvm use
npm install
npm run dev
```
`web/.env.local` already points `API_BASE_URL` at `http://localhost:5080`.

## Stripe

The scaffold's Stripe integration (Checkout Sessions for subscriptions and credit packs, webhook handling for `customer.subscription.*` / `invoice.paid` / `checkout.session.completed`) is fully wired but needs your own test-mode keys to actually run:

1. Set `STRIPE_SECRET_KEY` and `STRIPE_WEBHOOK_SECRET` in `.env` (or `Stripe:SecretKey`/`Stripe:WebhookSecret` in `appsettings.Development.json`).
2. Forward webhooks locally with the [Stripe CLI](https://stripe.com/docs/stripe-cli):
   ```bash
   stripe listen --forward-to localhost:5080/api/v1/webhooks/stripe
   ```
3. Create a membership plan or credit pack via the admin UI (or `POST /api/v1/membership-plans` / `POST /api/v1/credit-packs`) — this calls Stripe to create the Product/Price.
4. From the member portal's Membership page, subscribing/buying a pack redirects to a real Stripe Checkout session.

Without real keys, the checkout/webhook endpoints will return errors from Stripe's API — everything else in the app works independently of Stripe.

## Google OAuth

To enable "Continue with Google":
1. Create an OAuth client in the [Google Cloud Console](https://console.cloud.google.com/apis/credentials), authorized redirect URI `http://localhost:3000/api/auth/callback/google`.
2. Set `GOOGLE_CLIENT_ID` / `GOOGLE_CLIENT_SECRET` in `.env` (used by both the API's token-verification and the web app's NextAuth provider).

The flow: NextAuth handles the Google redirect in the browser, then the API's `POST /api/v1/auth/external` verifies the Google ID token server-side and issues Crew Flow's own JWT pair — so downstream code only ever deals with one token contract regardless of how a user signed in.

## Tests

```bash
cd api
dotnet test
```

## Known follow-ups (not implemented in this scaffold)

- CMS media is stored as external URL strings — no upload/asset library.
- The "Add weekly class" admin form asks for an instructor's raw user ID (no staff picker UI yet).
- No automated occurrence-generation job — new `ClassSchedule`s generate the next 8 weeks on creation; call `POST /api/v1/class-schedules/{id}/generate-occurrences` periodically (e.g. a nightly cron) to keep the horizon rolling forward.
- No email delivery (password reset, booking confirmations, waitlist-promotion notifications).
