# Deepening Opportunity 4: Extract Frontend API Configuration

**Severity:** 🟡 HIGH  
**Status:** Not started  
**Complexity:** Low

## Problem Statement

API base URL is hardcoded as `'http://localhost:5000'` in multiple files across the frontend. This creates three problems:

1. **No environment detection:** Cannot switch between dev, staging, production without code changes
2. **Scattered configuration:** Same URL duplicated in 3+ places; changing it requires multiple edits
3. **Untestable:** Cannot mock or swap API backend in tests without refactoring components

### Current State

**Files with hardcoded URLs:**
- `frontend/src/context/TodoContext.jsx:4`
- `frontend/src/components/DashboardPage.jsx:3`
- `frontend/src/components/LogsPage.jsx:3`

```javascript
// frontend/src/context/TodoContext.jsx
const API_BASE_URL = 'http://localhost:5000'

// frontend/src/components/DashboardPage.jsx
const API_BASE_URL = 'http://localhost:5000'

// frontend/src/components/LogsPage.jsx
const API_BASE_URL = 'http://localhost:5000'
```

**Usage:**
```javascript
const res = await fetch(`${API_BASE_URL}/todos`, { ... })
```

### Why This Is a Seam Without Adapters

- **No abstraction:** Callers (TodoContext, Dashboard, Logs) hardcode URL directly
- **One URL value, no polymorphism:** There's only one implementation (hardcoded string), but the seam exists implicitly (the URL)
- **Future adapters:** Multiple backends (dev, staging, prod, mock for testing) will need different URLs → real seam needs one adapter per backend
- **No interface:** Currently no module defining "where the API lives"; scattered across callers

## Solution

Create a configuration module that detects environment and provides the correct API base URL.

### 1. Create Configuration Module

**File:** `frontend/src/config/api.js`

```javascript
/**
 * API configuration with environment detection.
 * Respects VITE_API_URL env var for override, falls back to localhost for dev.
 */

export const getApiBaseUrl = () => {
  // Check environment variable first (set in .env, .env.development, .env.production)
  if (import.meta.env.VITE_API_URL) {
    return import.meta.env.VITE_API_URL;
  }

  // Development fallback
  if (import.meta.env.DEV) {
    return 'http://localhost:5000';
  }

  // Production: derive from window.location
  const { protocol, hostname } = window.location;
  const port = window.location.port ? `:${window.location.port}` : '';
  return `${protocol}//${hostname}${port}`;
};

export const API_BASE_URL = getApiBaseUrl();
```

### 2. Create Environment Files

**File:** `frontend/.env`

```env
# Default dev URL (when VITE_API_URL is not set)
# The app will use http://localhost:5000
```

**File:** `frontend/.env.development`

```env
# Development environment
# VITE_API_URL defaults to http://localhost:5000
# Override here if needed
```

**File:** `frontend/.env.production`

```env
# Production environment
# VITE_API_URL must be set by your build/deployment process
# If not set, the app will derive from window.location
VITE_API_URL=https://api.example.com
```

**File:** `frontend/.env.example`

```env
# API base URL
# If not set, defaults to http://localhost:5000 in dev, or window.location in prod
# VITE_API_URL=http://localhost:5000
# VITE_API_URL=https://api.staging.example.com
# VITE_API_URL=https://api.example.com
```

### 3. Update TodoContext

**File:** `frontend/src/context/TodoContext.jsx`

```javascript
import { API_BASE_URL } from '../config/api';

// ← Remove: const API_BASE_URL = 'http://localhost:5000'

// Now use the configured URL throughout the context
```

### 4. Update Components

**File:** `frontend/src/components/DashboardPage.jsx`

```javascript
import { API_BASE_URL } from '../config/api';

// ← Remove: const API_BASE_URL = 'http://localhost:5000'
```

**File:** `frontend/src/components/LogsPage.jsx`

```javascript
import { API_BASE_URL } from '../config/api';

// ← Remove: const API_BASE_URL = 'http://localhost:5000'
```

### 5. Better: Extract to API Client (with Deepening 1)

If implementing **Deepening 1** (Extract Frontend API Layer), this becomes even simpler:

**File:** `frontend/src/services/apiClient.js`

```javascript
import { API_BASE_URL } from '../config/api';

export const apiClient = {
  listTodos: async (filters = {}) => {
    const res = await fetch(`${API_BASE_URL}/todos?...`);
    // ...
  },
  // ...
};
```

Now components don't import API_BASE_URL at all—they only import `apiClient`.

## Benefits

### Locality
- **Configuration centralized:** All API URL logic lives in one place (`config/api.js`)
- **Change efficiency:** To change API endpoint, edit one file; environment detection works everywhere
- **Environment handling:** No hardcoding for each environment

### Leverage
- **Environment-aware:** Dev uses localhost, prod derives from window.location or env var
- **Swappable in tests:** Tests can override `VITE_API_URL` to mock API
- **Reusable across frontend:** All components use same configuration without duplication

### Testing

**Before:** API URL hardcoded; cannot swap in tests
```javascript
// Cannot mock or change API endpoint
const API_BASE_URL = 'http://localhost:5000' // ← Locked in
```

**After:** API URL configurable via environment
```javascript
// In test setup:
process.env.VITE_API_URL = 'http://localhost:3001'; // ← Override for tests

// Or:
// Use mock API server (Mirage.js, MSW) listening on different port
```

**Vitest Example:**
```javascript
import { describe, it, expect, beforeAll, afterAll } from 'vitest';
import { getApiBaseUrl } from '../config/api';

describe('API Configuration', () => {
  const originalEnv = import.meta.env;

  it('uses VITE_API_URL if set', () => {
    import.meta.env.VITE_API_URL = 'https://custom.api.com';
    expect(getApiBaseUrl()).toBe('https://custom.api.com');
  });

  it('defaults to localhost in dev', () => {
    delete import.meta.env.VITE_API_URL;
    import.meta.env.DEV = true;
    expect(getApiBaseUrl()).toBe('http://localhost:5000');
  });

  it('derives from window.location in prod', () => {
    delete import.meta.env.VITE_API_URL;
    import.meta.env.DEV = false;
    // Mocking window.location...
    expect(getApiBaseUrl()).toMatch(/^https?:\/\/.+/);
  });
});
```

## Environment Setup for CI/CD

### GitHub Actions Example

**File:** `.github/workflows/deploy.yml`

```yaml
- name: Build Frontend
  env:
    VITE_API_URL: ${{ secrets.API_URL }}
  run: npm run build --prefix frontend
```

### Docker Example

**File:** `Dockerfile`

```dockerfile
FROM node:18 AS build
WORKDIR /app
COPY frontend/ .
ARG API_URL=http://localhost:5000
ENV VITE_API_URL=$API_URL
RUN npm install && npm run build

FROM nginx:latest
COPY --from=build /app/dist /usr/share/nginx/html
```

Build with:
```bash
docker build --build-arg API_URL=https://api.example.com -t my-frontend .
```

## Implementation Checklist

- [ ] Create `frontend/src/config/api.js` with `getApiBaseUrl()` and `API_BASE_URL` export
- [ ] Create `frontend/.env` (placeholder)
- [ ] Create `frontend/.env.development` (optional)
- [ ] Create `frontend/.env.production` with `VITE_API_URL` example
- [ ] Create `frontend/.env.example` with documentation
- [ ] Update `frontend/src/context/TodoContext.jsx` to import from config
- [ ] Update `frontend/src/components/DashboardPage.jsx` to import from config
- [ ] Update `frontend/src/components/LogsPage.jsx` to import from config
- [ ] Add `VITE_API_URL` to deployment/CI documentation
- [ ] Test with different API URLs (localhost, staging, production)

## Related Deepening Opportunities

- **Deepening 1:** Extract Frontend API Layer (combines with this for clean separation)
- **Deepening 5:** Extract Frontend Filter State to Hook (independent; can be done in parallel)
