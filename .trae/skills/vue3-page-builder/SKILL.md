---
name: vue3-page-builder
description: "Create Vue 3 pages in module-first structure. Invoke when adding list/detail/form pages, lazy routes with meta.requiresAuth, or wiring page to Pinia store."
---

# Vue3 Page Builder

## When to Use
- You need a new Vue 3 page quickly.
- You want consistent page naming and route conventions.
- You want page scaffolding aligned with client/src/modules/** rules.

## Fixed Conventions
1. Page files are under client/src/modules/<module>/views/.
2. Page component names are PascalCase and end with View.
3. Routes are lazy-loaded.
4. Every route has meta.requiresAuth set explicitly.
5. Page logic uses Pinia setup store or composable, not direct axios calls.
6. Layout is PC-first by default (desktop control console style), not mobile-first.

## PC-First UI Rules (Mandatory)
1. Use desktop canvas with min-width >= 1280px for core console pages.
2. For task/console pages, prefer 2-3 column composition on desktop.
3. Do not collapse primary information architecture into single-column mobile layout by default.
4. Keep module navigation visible on desktop without drawer dependence.
5. Allow horizontal scroll fallback for narrow windows instead of rewriting to phone layout.

## Required Input
1. Module name in kebab-case, for example users.
2. Page type: list, detail, form, or custom.
3. Route path and route name.
4. Auth requirement: true or false.
5. Data source: existing store/composable or create TODO stub.

## Output Targets
- client/src/modules/<module>/views/<ModulePascal><PageTypePascal>View.vue
- client/src/modules/<module>/routes.ts

## Procedure Checklist
1. Create page view file from template.
2. Bind route params using useRoute when needed.
3. Add loading, error, empty, success UI states.
4. Wire to store/composable actions.
5. Register lazy route in module routes.
6. Set meta.requiresAuth explicitly.
7. Verify naming matches module conventions.

## Assets
- [page.template.vue](./assets/page.template.vue)
- [route-record.template.ts](./assets/route-record.template.ts)
- [index-export.template.ts](./assets/index-export.template.ts)

## Placeholder Tokens
- __module__: kebab-case module name, example users.
- __ModulePascal__: PascalCase module name, example Users.
- __PageTypePascal__: PascalCase page type, example List.
- __route_path__: route path, example /users.
- __route_name__: route name, example users.list.
- __requires_auth__: true or false.

## Done Criteria
1. View path and naming follow client/src/modules/<module>/views/<ModulePascal><PageTypePascal>View.vue.
2. Route is lazy-loaded and includes meta.requiresAuth.
3. View has deterministic loading/error/empty states.
4. View does not directly call axios.
5. Page passes PC-first checks (desktop multi-column layout and visible navigation).

## Example Prompts
- /vue3-page-builder create users list page at /users with requiresAuth true
- /vue3-page-builder add orders detail page at /orders/:id and route name orders.detail
- /vue3-page-builder create products form page at /products/create with store wiring
