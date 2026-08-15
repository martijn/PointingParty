# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

Pointing Party is a real-time agile story point estimation tool built with .NET 10 Blazor WebAssembly. The application has no backend database - instead, each client maintains its own game state based on events published through a central SignalR hub.

## Build and Run Commands

```bash
# Build the entire solution
dotnet build

# Run the application (default port 5174)
dotnet run --project PointingParty

# Run unit tests
dotnet test

# Run a specific test
dotnet test --filter "FullyQualifiedName~GameUiTests"

# Build Tailwind CSS (development)
npx tailwindcss -i PointingParty/wwwroot/app.css -o PointingParty/wwwroot/app.min.css --watch

# Build Tailwind CSS (production)
npx tailwindcss -i PointingParty/wwwroot/app.css -o PointingParty/wwwroot/app.min.css --minify

# Run end-to-end tests (requires app to be running)
cd e2e
npm install
BASE_URL=http://localhost:5174 npx playwright test --ui

# Build Docker image
docker build -t pointingparty .
```

## Architecture

### Project Structure

The solution consists of 4 projects:

- **PointingParty** - Server project hosting the Blazor WebAssembly app and SignalR hub
- **PointingParty.Client** - Blazor WebAssembly client application
- **PointingParty.Domain** - Shared domain models and events
- **PointingParty.Client.Tests** - Unit tests using bunit and xUnit

### Event-Driven State Management

The application uses an event-sourcing pattern without persistence:

1. **GameAggregate** (PointingParty.Domain/GameAggregate.cs) - Core domain logic that maintains game state by applying events. Public methods create events which are added to `EventsToPublish` list.

2. **IGameEvent** - Base interface for all game events (PlayerJoinedGame, VoteCast, VotesShown, GameReset, PlayerLeftGame, Sync)

3. **GameEventHub** (PointingParty/GameEventHub.cs) - SignalR hub on the server that broadcasts events to all clients in a game group. When clients disconnect, it automatically broadcasts a PlayerLeftGame event.

4. **GameContext** (PointingParty.Client/GameContext.cs) - Client-side service that:
   - Manages SignalR connection to the hub
   - Maintains local GameAggregate instance
   - Receives events from other clients via SignalR
   - Publishes local events to the hub
   - Handles reconnection by resetting local state and syncing with other players

### State Flow

1. User action → GameAggregate method called (e.g., `VoteCast()`)
2. GameAggregate creates event and adds to `EventsToPublish`
3. Event is applied locally via `Apply()` method
4. GameContext publishes events to SignalR hub
5. Hub broadcasts to other clients in the same game group
6. Other clients receive event via `ReceiveGameEvent()`
7. Each client applies event to their local GameAggregate

### Blazor Rendering

The app uses Blazor's Interactive WebAssembly render mode. The server project includes a MockGameContext for prerendering purposes, while the actual client uses the full GameContext implementation.

### Styling

The UI is styled with plain CSS in `PointingParty/wwwroot/app.css`, built into `app.min.css`. Colours, spacing and elevation come from CSS custom properties defined on `:root` (dark, the default) and `[data-theme="light"]`. Component classes are prefixed `pp-`.

Tailwind is still in the pipeline but only contributes its preflight reset - no utility classes are used in the Razor files. Its config lives in `tailwind.config.js` at the repository root.

### Theming

`data-theme` is stamped on `<html>` by an inline script in `App.razor` before Blazor boots, so there is no flash of the wrong scheme. The choice is persisted in `localStorage` under `pp-theme`; `prefers-color-scheme` is only the initial value. The toggle (`ThemeToggle.razor`) calls `ppTheme.toggle()` directly rather than going through Blazor, because it renders in statically rendered chrome on every page.

### Player avatars

Avatar colours are a pure function of the player name (`PlayerAvatar.cs`): FNV-1a hash spread by the golden angle gives the hue, while lightness and chroma come from theme tokens. No server state, and the same player is the same colour on every client.

## Key Files and Components

- **PointingParty/Program.cs** - Server startup, configures SignalR with Azure SignalR Service support
- **PointingParty.Client/Program.cs** - Client startup, registers GameContext
- **PointingParty.Client/Components/Pages/Game.razor** - Main game page component
- **PointingParty.Domain/Events/** - All game event definitions
- **GameState.cs** - Immutable record holding current game state (votes, show/hide status)
- **Vote.cs** - Domain model for player votes

## Testing

Unit tests use bunit for Blazor component testing and NSubstitute for mocking. Tests are located in PointingParty.Client.Tests.

End-to-end tests use Playwright and are located in the `e2e/` directory. The application must be running before executing e2e tests.

## Deployment

The application is containerized and deployed to Fly.io (see `fly.toml`). The Dockerfile uses multi-stage builds with .NET 10 SDK and runtime images.
