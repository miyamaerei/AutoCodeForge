---
applyTo: 'src/modules/**/*.ts,src/modules/**/*.vue'
description: 'Enforce naming and export conventions for Vue 3 module-first structure under src/modules/**.'
---

# Module Naming And Export Rules

Use these rules for all files in src/modules/**.

## Folder And File Naming
- Module folder must be kebab-case: src/modules/<module-name>/.
- API files must be named <module-name>.api.ts and <module-name>.types.ts.
- Model files must be named <module-name>.model.ts and <module-name>.mapper.ts.
- Store file must be named use<ModulePascal>Store.ts.
- Route file must be named routes.ts.
- View files must be PascalCase and end with View.vue.

## Symbol Naming
- DTO types must end with Dto.
- Domain model types must end with Model.
- Mapper functions must be named dtoToModel and modelToDto.
- Store symbol must be named use<ModulePascal>Store.
- Route array must be named <moduleNameCamel>Routes.

## Export Rules
- Avoid default exports in src/modules/** TypeScript files.
- Use named exports for API functions, models, mappers, stores, and route arrays.
- Each module must have src/modules/<module-name>/index.ts as the public entry.
- index.ts must re-export only module public API:
  - API functions and request/response types.
  - Model and mapper functions.
  - Store function.
  - Route array.
- Views must not be exported from index.ts unless explicitly requested by the feature scope.

## Layer Boundaries
- Views and components must not call axios directly.
- API layer is the only place allowed to perform HTTP calls.
- Views consume store/composables, not raw DTO payloads.
- DTO objects must be converted by mapper before entering view state.

## Routing Rules
- Route records must be lazy-loaded.
- Every route must include meta.requiresAuth with explicit true or false.
- Route names must use module prefix: <module>.list, <module>.detail, <module>.create, <module>.edit.

## Store Rules
- Use Pinia setup store syntax with defineStore and setup function.
- Store must expose loading, error, and at least one computed state.
- Async actions must handle try/catch/finally with deterministic state transitions.

## PC-First Layout Rules
- Default target is desktop console experience, not mobile-first composition.
- Core module pages must use min-width >= 1280px unless explicitly waived by product requirement.
- Information-dense modules should use two-column or three-column desktop layout when appropriate.
- Do not hide primary navigation behind mobile drawers in default desktop flow.
- If viewport is narrower than desktop baseline, prefer horizontal overflow fallback over collapsing architecture into single-column phone layout.
- Keep key KPIs, task controls, and status panes visible above the fold on common desktop resolutions.

## Minimal Acceptance Checklist
1. Files are located under src/modules/<module-name>/ only.
2. Naming and suffix conventions are followed.
3. index.ts exists and re-exports named symbols only.
4. Routes include meta.requiresAuth on every entry.
5. PC-first layout rules are satisfied for module views.
6. Build and tests pass after changes.
