# MooBank Web App - Agent Guidelines

This document provides guidance for AI agents working on the MooBank React frontend.

## Technology Stack

- **React 19** with TypeScript
- **Vite** - Build tool
- **React Router 7** - Client-side routing
- **React Query (TanStack Query v5)** - Server state management
- **Redux Toolkit** - UI state only (filters, pagination)
- **React Bootstrap** - UI components
- **React Hook Form** - Form handling
- **Chart.js / react-chartjs-2** - Data visualisation
- **date-fns** - Date manipulation
- **@andrewmclachlan/moo-ds** - Custom design system components
- **@andrewmclachlan/moo-app** - Application framework wrapper
- **@andrewmclachlan/mooicons** - Custom icon library

## Project Structure

```
src/
├── api/                    # Auto-generated API types (not used directly)
├── components/             # Shared reusable components
├── css/                    # Feature-specific stylesheets
├── helpers/                # Utility functions
├── hooks/                  # Custom React hooks
├── models/                 # TypeScript interfaces
├── pages/                  # Feature modules (organised by feature)
├── services/               # React Query hooks for API calls
├── store/                  # Redux state (UI state only)
├── App.tsx                 # App entry point
├── Layout.tsx              # Main layout with navigation
└── Routes.tsx              # Route definitions
```

## Adding a New Feature

### 1. Create Models (`src/models/`)

Define TypeScript interfaces matching backend DTOs:

```typescript
// src/models/MyFeature.ts
export interface MyEntity {
    id: string;
    name: string;
    // ... other properties
}

export interface CreateMyEntity {
    name: string;
    // ... creation properties
}
```

Export from barrel file:
```typescript
// src/models/index.ts
export * from "./MyFeature";
```

### 2. Create Service (`src/services/`)

Use the custom hooks from `@andrewmclachlan/moo-app`:

```typescript
// src/services/MyFeatureService.ts
import { useQueryClient } from "@tanstack/react-query";
import { useApiDelete, useApiGet, useApiPatch, useApiPost } from "@andrewmclachlan/moo-app";
import { MyEntity, CreateMyEntity } from "../models";

interface MyVariables {
    id: string;
}

export const myFeatureKey = ["myFeature"];

// Query hook
export const useMyEntities = () =>
    useApiGet<MyEntity[]>([myFeatureKey], `api/myfeature`);

export const useMyEntity = (id: string) =>
    useApiGet<MyEntity>([myFeatureKey, id], `api/myfeature/${id}`, {
        enabled: !!id
    });

// Mutation hooks
export const useCreateMyEntity = () => {
    const queryClient = useQueryClient();

    const { mutate, mutateAsync, isPending } = useApiPost<MyEntity, null, CreateMyEntity>(
        () => `api/myfeature`,
        {
            onSettled: () => {
                queryClient.invalidateQueries({ queryKey: [myFeatureKey] });
            }
        }
    );

    const create = (entity: CreateMyEntity) => mutate([null, entity]);
    return { create, mutateAsync, isPending };
};

export const useUpdateMyEntity = () => {
    const queryClient = useQueryClient();

    const { mutate, isPending } = useApiPatch<MyEntity, MyVariables, Partial<MyEntity>>(
        (variables) => `api/myfeature/${variables.id}`,
        {
            onSettled: () => {
                queryClient.invalidateQueries({ queryKey: [myFeatureKey] });
            }
        }
    );

    const update = (id: string, entity: Partial<MyEntity>) => mutate([{ id }, entity]);
    return { update, isPending };
};

export const useDeleteMyEntity = () => {
    const queryClient = useQueryClient();

    const { mutate } = useApiDelete<MyVariables>(
        (variables) => `api/myfeature/${variables.id}`,
        {
            onSuccess: () => {
                queryClient.invalidateQueries({ queryKey: [myFeatureKey] });
            }
        }
    );

    return (id: string) => mutate({ id });
};
```

Export from barrel file:
```typescript
// src/services/index.ts
export * from "./MyFeatureService";
```

### 3. Create Page Components (`src/pages/myfeature/`)

#### Page Wrapper
```typescript
// src/pages/myfeature/MyFeaturePage.tsx
import React, { PropsWithChildren } from "react";
import { Page } from "@andrewmclachlan/moo-app";

export const MyFeaturePage: React.FC<PropsWithChildren<MyFeaturePageProps>> = ({ children, breadcrumbs = [] }) => (
    <Page title="My Feature" breadcrumbs={[{ text: "My Feature", route: `/myfeature` }, ...breadcrumbs]}>
        {children}
    </Page>
);

export interface MyFeaturePageProps {
    breadcrumbs?: { text: string; route: string }[];
}
```

#### Main Component
```typescript
// src/pages/myfeature/MyFeature.tsx
import { Section, SectionTable } from "@andrewmclachlan/moo-ds";
import { useMyEntities } from "services/MyFeatureService";
import { MyFeaturePage } from "./MyFeaturePage";

export const MyFeature: React.FC = () => {
    const { data: entities, isLoading } = useMyEntities();

    if (isLoading) {
        return (
            <MyFeaturePage>
                <Section>Loading...</Section>
            </MyFeaturePage>
        );
    }

    return (
        <MyFeaturePage>
            <SectionTable header="Items" striped>
                <thead>
                    <tr>
                        <th>Name</th>
                        <th className="row-action"></th>
                    </tr>
                </thead>
                <tbody>
                    {entities?.map((entity) => (
                        <tr key={entity.id}>
                            <td>{entity.name}</td>
                            <td className="row-action">
                                <DeleteIcon onClick={() => handleDelete(entity.id)} />
                            </td>
                        </tr>
                    ))}
                </tbody>
            </SectionTable>
        </MyFeaturePage>
    );
};
```

#### Barrel Export
```typescript
// src/pages/myfeature/index.ts
export * from "./MyFeature";
```

### 4. Add Routes (`src/Routes.tsx`)

```typescript
export const routes: RouteDefinition = {
    layout: {
        path: "/", element: <Layout />, children: {
            // ... existing routes
            myfeature: { path: "/myfeature", element: <Pages.MyFeature /> },
        }
    }
};
```

### 5. Add Navigation (`src/Layout.tsx`)

```typescript
const sideMenu = [
    // ... existing items
    {
        text: "My Feature",
        image: <Icons.SomeIcon />,
        route: "/myfeature"
    },
];
```

### 6. Add Styles (`src/css/`)

```css
/* src/css/myfeature.css */
.my-feature-class {
    /* styles */
}
```

Import in App.css:
```css
@import "css/myfeature" layer(moobank);
```

### 7. Export Pages (`src/pages/index.ts`)

```typescript
export * from "./myfeature";
```

## Key Patterns

### API Hooks Pattern

The `useApi*` hooks from `@andrewmclachlan/moo-app` follow this pattern:

- `useApiGet<TResponse>(queryKey, url, options?)` - GET requests
- `useApiPost<TResponse, TVariables, TBody>(urlFn, options?)` - POST requests
- `useApiPatch<TResponse, TVariables, TBody>(urlFn, options?)` - PATCH requests
- `useApiDelete<TVariables>(urlFn, options?)` - DELETE requests

For mutations, the mutate function takes `[variables, body]` as a tuple.

### Inline Editing

Use `EditColumn` from moo-ds for inline editable table cells:

```typescript
<EditColumn
    value={entity.name}
    onChange={(v) => updateEntity(entity.id, { name: v.value })}
/>
```

### Table Actions

```typescript
import { DeleteIcon, SaveIcon, EditColumn } from "@andrewmclachlan/moo-ds";

<td className="row-action">
    <DeleteIcon onClick={handleDelete} />
</td>
```

### Form Controls

Use React Bootstrap Form components:

```typescript
import { Form, Button, Row, Col } from "react-bootstrap";

<Form.Group>
    <Form.Label>Name</Form.Label>
    <Form.Control
        type="text"
        value={name}
        onChange={(e) => setName(e.target.value)}
    />
</Form.Group>
```

### Section Components

```typescript
import { Section, SectionTable } from "@andrewmclachlan/moo-ds";

<Section header="Title">
    {/* Content */}
</Section>

<SectionTable header="Table Title" striped>
    {/* Table content */}
</SectionTable>
```

## Available Icons

From `@andrewmclachlan/mooicons`:
- `Dashboard`, `PiggyBank`, `TwoCoins`, `Budget`, `Stack`, `Tags`
- `Cog`, `Sliders`, `Reports`, `Rules`, `Transaction`, `Import`
- `Trendline`, `BarChart`, `PieChart`, `Hierarchy`
- And more...

## CSS Conventions

### Critical Rules

**NEVER use Bootstrap's utility CSS classes** such as:
- `d-flex`, `d-block`, `d-none`, etc.
- `justify-content-*`, `align-items-*`, `flex-*`
- `m-*`, `mb-*`, `mt-*`, `mx-*`, `my-*` (margin utilities)
- `p-*`, `pb-*`, `pt-*`, `px-*`, `py-*` (padding utilities)
- `text-center`, `text-muted`, `text-*`
- `w-*`, `h-*` (width/height utilities)
- `row`, `col`, `col-*` (grid utilities outside of proper layouts)

**Instead, use proper CSS with reusable classes:**
- Create semantic class names that describe the purpose (e.g., `filter-row`, `chart-container`, `tab-header`)
- Define layout using CSS Grid or Flexbox in stylesheet files
- Place styles in feature-specific CSS files under `src/css/`
- Import CSS files in `App.css` using `@import "css/featurename" layer(moobank);`

### Component Usage

**Use existing components from `@andrewmclachlan/moo-ds`:**
- Use `Section` with a header instead of Bootstrap's `Card` component
- Use `SectionTable` for tables with headers
- Use `SectionForm` for forms within sections

### General Guidelines

- Use CSS layers: `@layer mooapp, layout, moobank;`
- Feature styles go in `layer(moobank)`
- Column width classes: `column-15`, `column-20`, `column-25`, `column-50`
- Action columns: `row-action`
- Negative values: `negative` class
- Amount formatting: `amount` class
- Use nested CSS for component-scoped styles
- Prefer CSS Grid over Bootstrap's grid system for layouts

## State Management Philosophy

- **React Query** - All server/API data
- **Redux** - UI-only state (filters, pagination, sort)
- **useState/useEffect** - Component-local state
- **Custom hooks with localStorage** - Persisted preferences

## Date Handling

Use `date-fns` for all date operations:

```typescript
import { format, parseISO, addYears } from "date-fns";

const formatted = format(parseISO(dateString), "dd MMM yyyy");
const futureDate = addYears(new Date(), 2);
```

## Charts

Use Chart.js with react-chartjs-2:

```typescript
import { Line, Bar, Doughnut } from "react-chartjs-2";
import { Chart as ChartJS, registerables } from "chart.js";

ChartJS.register(...registerables);
```

## Build Commands

```bash
npm start          # Development server
npm run build      # Production build (runs tsc && vite build)
npm run lint       # ESLint
npm run generate   # Regenerate API types (not currently used)
```
