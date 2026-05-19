---
name: vue3-api-model-router-store
description: 'Checklist skill for Vue 3 module scaffolding. Use for: create api/model/router/store with fixed stack (axios + pinia setup store + route meta.requiresAuth) and enforced module-first folder structure.'
argument-hint: 'Describe module goal and pages (e.g. users list+detail+form)'
---

# Vue3 API + Model + Router + Store Checklist

## When to Use
- You want a fast, repeatable checklist to scaffold a Vue 3 module.
- You want fixed technical choices instead of per-task architecture debates.
- You need API, model, router, and store delivered in one pass.

## Fixed Rules (Do Not Change)
1. HTTP client is axios.
2. Route guard convention uses meta.requiresAuth.
3. Store uses Pinia setup store syntax.
4. Folder layout is module-first and mandatory.
5. View implementation is PC-first for console-like pages (desktop layout required).

## PC-First View Constraints
1. Default viewport target is desktop; use min-width >= 1280px for main consoles.
2. Console pages should use multi-column information architecture.
3. Do not optimize first for phone-size single-column flow unless explicitly requested.
4. Preserve desktop navigation visibility and module switching efficiency.

## Input Checklist
1. Module name and business goal.
2. Required pages: list, detail, form.
3. API endpoints: list/get/create/update/delete.
4. Route paths and param names.
5. Auth requirement per route (true/false).

## Enforced Output Structure
All module artifacts must live under one directory:

- client/src/modules/<module>/api/<module>.api.ts
- client/src/modules/<module>/api/<module>.types.ts
- client/src/modules/<module>/models/<module>.model.ts
- client/src/modules/<module>/models/<module>.mapper.ts
- client/src/modules/<module>/store/use<Module>Store.ts
- client/src/modules/<module>/routes.ts
- client/src/modules/<module>/views/<Module>ListView.vue
- client/src/modules/<module>/views/<Module>DetailView.vue
- client/src/modules/<module>/views/<Module>FormView.vue
- client/src/modules/<module>/index.ts

## Template Assets
Use these templates to keep generated output consistent:

- [api.template.ts](./assets/api.template.ts)
- [mapper.template.ts](./assets/mapper.template.ts)
- [store.template.ts](./assets/store.template.ts)
- [routes.template.ts](./assets/routes.template.ts)
- [view.template.vue](./assets/view.template.vue)
- [index-export.template.ts](./assets/index-export.template.ts)

Placeholder tokens in templates:
- __module__: kebab-case module name, example user-profiles.
- __ModulePascal__: PascalCase module name, example UserProfiles.

## Execution Checklist
1. Define DTO and domain model.
- Keep DTO aligned to backend contract.
- Keep model UI-friendly and normalized.
- Implement dtoToModel and modelToDto.

2. Build API client module.
- Create a typed axios instance usage for list/get/create/update/delete.
- Keep request and response types in api/<module>.types.ts.
- Return DTO from API layer.

3. Create mapping layer.
- Implement null-safe mapping defaults.
- Keep mapping functions pure and testable.

4. Create Pinia setup store.
- Use defineStore with setup-style state/actions.
- Include loading, error, and success state transitions.
- Add actions for fetch list, fetch detail, create, update, delete.

5. Add routes.
- Create lazy-loaded module routes.
- Set meta.requiresAuth explicitly on each route.
- Use typed dynamic params for detail/form pages.

6. Wire views to store/API.
- Views consume store actions and store state.
- Do not call axios directly inside views.
- Render loading, error, empty, and success states.
- Apply PC-first layout constraints for console pages.

7. Add baseline tests (minimum).
- Mapper tests for null and roundtrip behavior.
- Store tests for success and failure transitions.
- One route or view smoke test.

8. Run verification.
- npm run build
- npm run test:unit
- Fix type errors and failing tests before completion.

## Companion Instruction
For strict naming/export enforcement in project files, pair this skill with:

- [.github/instructions/modules-naming-exports.instructions.md](../../instructions/modules-naming-exports.instructions.md)

## Minimal Branching Rules
1. If backend contract is incomplete:
- Use provisional DTO types and document assumptions in code comments.

2. If no auth is needed:
- Keep meta.requiresAuth set to false (still required).

3. If a page is out of scope:
- Keep file placeholders or TODO markers so structure remains complete.

## Quality Gates (Definition of Done)
1. Module follows enforced directory layout exactly.
2. DTO, model, and mapper are separated and type-safe.
3. API calls are centralized in axios API module.
4. Pinia setup store handles loading/success/error states.
5. Routes are lazy-loaded and include meta.requiresAuth.
6. Build and unit tests pass.
7. Console-related views satisfy PC-first layout constraints.

## Common Pitfalls
- Putting files outside client/src/modules/<module>/.
- Using options-style Pinia store instead of setup store.
- Forgetting meta.requiresAuth on one route.
- Bypassing mapper and passing DTO directly to views.

## Example Prompts
- /vue3-api-model-router-store create users module with list detail form and full checklist
- /vue3-api-model-router-store scaffold products module using fixed axios model mapper router pinia setup store
- /vue3-api-model-router-store build orders module with requiresAuth routes and baseline tests
