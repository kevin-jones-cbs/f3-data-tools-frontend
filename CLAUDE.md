# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

F3 South Fork Tools is a Blazor WebAssembly frontend application for managing F3 (Fitness, Fellowship, Faith) workout data. The application allows uploading PAX (participant) data, tracking attendance, viewing analytics, and managing challenges across multiple regions.

## Technology Stack

- **.NET 6.0** - Target framework
- **Blazor WebAssembly** - SPA framework running entirely in the browser
- **Blazorise** - UI component library with Bootstrap providers
- **xUnit** - Testing framework
- **F3Core.dll** - External shared library referenced from `F3Wasm/Packages/`

## Common Commands

### Build
```bash
dotnet build F3Wasm/F3Wasm.csproj
```

### Run Development Server
```bash
dotnet run --project F3Wasm/F3Wasm.csproj
```

### Watch Mode (auto-reload on changes)
```bash
dotnet watch run --project F3Wasm/F3Wasm.csproj
```

### Publish for Production
```bash
dotnet publish F3Wasm/F3Wasm.csproj -c Release -o F3Wasm/release
```

### Run Tests
```bash
dotnet test F3Wasm.Tests/F3Wasm.Tests.csproj
```

### Run Tests (from solution root)
```bash
dotnet test
```

## Architecture

### External Dependencies

**F3Core.dll** - A critical external library located at `F3Wasm/Packages/F3Core.dll`. This DLL is referenced by both the main project and tests. It contains shared models and business logic including:
- Core data models (Pax, Ao, Post, AllData, InitialViewData, DisplayRow, etc.)
- Region-specific logic (RegionList, RegionInfo classes)
- Challenge data (TerracottaChallenge, etc.)
- Data calculation helpers (DataHelper.SetCurrentRows, etc.)

The F3Core library is built from a separate repository (`f3-data-tools-core`) and copied into the Packages directory.

### Backend Integration

The app communicates with an **AWS Lambda backend** via HTTP:
- Primary Lambda URL: `https://s6oww3m3a5svbuxq5pf35pjigu0xxaqk.lambda-url.us-west-1.on.aws/`
- Exicon search Lambda: `https://wdd5t63r4ve5btbdv5yypcnugq0fgeow.lambda-url.us-west-1.on.aws/`

All backend interactions go through `F3Wasm/Helpers/LambdaHelper.cs`, which provides methods for:
- **GetInitialViewAsync** - Fetches pre-calculated AllTime view with metadata (validYears, validMonths, lastUpdatedDate, firstNonHistoricalDate) for fast initial page load
- **GetAllDataAsync** - Fetches complete dataset (AllData) with all posts, PAX, and AOs
- Fetching PAX data
- Uploading workout posts
- Managing locations (AOs)
- Cache management

**Performance Optimizations**:
- `GetInitialView` returns cached `InitialViewData` object for instant page load (<200ms)
- `GetAllData` returns GZip compressed and base64-encoded data for full dataset (slower, ~2-5s)
- Initial page load uses cached InitialView ONLY - full data is NOT loaded until user clicks a view that requires it
- True lazy loading: many users never load full data, saving bandwidth and processing time

### Project Structure

```
F3Wasm/                   - Main Blazor WebAssembly project
├── Pages/                - Routable page components
│   ├── Index.razor       - Data upload page (/upload/{region})
│   ├── Data.razor        - Analytics dashboard (/data/{region})
│   ├── Ceremonies.razor  - Ceremonies/special events
│   ├── Downrange.razor   - Downrange PAX tracking
│   ├── Exicon.razor      - F3 terminology search
│   ├── Maintainence.razor- Admin/maintenance tools
│   └── SectorsData.razor - Multi-region sector view
├── Components/           - Reusable UI components
│   ├── Challenges/       - Challenge-specific components
│   │   ├── PaxOnFire.razor
│   │   ├── HighWaterMark.razor
│   │   └── GoldStandard.razor
│   ├── PaxSummaryTable.razor
│   ├── PaxCalendar.razor
│   ├── PaxLocations.razor
│   ├── PaxPostedWith.razor
│   ├── OverallStatDisplay.razor
│   ├── AoChallengeModal.razor
│   └── ProgressContainer.razor
├── Helpers/              - Utility classes
│   ├── LambdaHelper.cs   - Backend API integration
│   ├── ColorHelpers.cs   - Color generation utilities
│   ├── StreakHelpers.cs  - Streak calculation logic
│   └── ExiconData.cs     - F3 terminology data
├── Models/               - Data models and DTOs
│   ├── Enums/
│   │   ├── OverallView.cs
│   │   └── SortView.cs
│   ├── DisplayRow.cs
│   ├── ExiconEntry.cs
│   ├── OverallStat.cs
│   └── WorkoutDay.cs
├── Shared/               - Layout components
│   ├── MainLayout.razor
│   ├── NavMenu.razor
│   └── F3BaseLayout.cs   - Base layout with region extraction
├── Packages/             - External DLLs
│   └── F3Core.dll        - Shared business logic library
└── Program.cs            - App startup and DI configuration

F3Wasm.Tests/            - xUnit test project
```

### Routing and Regions

Most pages are **region-aware** and use route parameters like `/upload/{region}` or `/data/{region}`. The `F3BaseLayout` base class extracts the region from route data and query strings, making it available to child components.

Region codes (e.g., "southfork", "sacramento", "terracotta") correspond to different F3 chapters/areas.

### Key Workflows

**Data Upload Flow** (Index.razor):
1. User selects a date and location (AO)
2. User pastes PAX comment from Slack/social media
3. App calls Lambda to parse and match PAX names
4. User reviews/corrects matches, marks Q and FNGs
5. Data is uploaded to backend (Google Sheets via Lambda)

**Analytics View** (Data.razor):
1. **Fast Initial Load** - App calls `GetInitialView` Lambda endpoint to fetch pre-calculated AllTime view with metadata (validYears, validMonths, lastUpdatedDate, firstNonHistoricalDate)
2. Page displays immediately with AllTime stats and functional dropdowns
3. **True Lazy Loading** - Full AllData is ONLY loaded when user clicks a view that requires it (Year, Month, Kotter, QKotter, Challenges, or opens a PAX modal)
4. Loading indicator displayed when full data is needed but not yet loaded
5. Various views: overall stats, calendar, locations, challenges
6. Filtering and sorting options available

**Initial Load Optimization Strategy**:
- `OnInitializedAsync()` - Calls `GetInitialView` for instant display of cached AllTime data with metadata
- NO background loading - full data stays unloaded until needed
- `EnsureFullDataLoadedAsync()` - Helper method that all views requiring full data call; shows loading indicator and loads data on first access
- This approach ensures the page is interactive immediately and many users never need to load the full dataset (saving bandwidth and time)

### Component Communication

Pages use `.razor.cs` code-behind files for logic. Key patterns:
- `@inject HttpClient Http` for API calls
- `@inject NavigationManager` for routing
- `@code` blocks for component state and methods
- Blazorise components for UI (`<DataGrid>`, `<Modal>`, `<Button>`, etc.)

### Styling

- Bootstrap 5 via Blazorise
- FontAwesome icons via Blazorise.Icons.FontAwesome
- Custom CSS in `<style>` blocks within `.razor` files
- Some SpinKit loading animations

## Development Notes

### F3Core.dll Updates

When the F3Core library is updated, you need to:
1. Build the F3Core project from its separate repository
2. Copy the resulting DLL to `F3Wasm/Packages/F3Core.dll`
3. Rebuild the F3Wasm project

The VS Code tasks include `buildCoreToWasm` for this workflow.

### Region Configuration

Region-specific behavior is controlled by the `F3Core.RegionInfo` class. Features that vary by region:
- `HasQSourcePosts` - Whether region supports Q Source meetings
- `HasExtraActivity` - Whether region tracks pre-workout activity

### Deployment

The app is deployed to AWS Amplify. See `amplify.yml` for the build configuration. The deployment:
1. Installs .NET 6.0
2. Publishes the Blazor WASM app
3. Serves the `wwwroot` output as static files

### Testing

Tests are minimal but exist in `F3Wasm.Tests/`. They reference F3Core.dll and test basic functionality like date calculations and data transformations.
