# Base components (`app/ui`)

Small standalone pieces that carry the look of the design (`design/helpdesk.pen`). Their styles live in `src/styles/ui.css` on the tokens
of `tokens.css`, so they follow the light and dark themes. Import the ones you use in the `imports` of your component.

| Piece | Use | Notes |
|---|---|---|
| `HdBadgeComponent` `hd-badge` | `<hd-badge tone="prog">En proceso</hd-badge>` | tones `open`, `prog`, `done`, `appr`; `[small]="true"` for a smaller pill |
| `HdProgressComponent` `hd-progress` | `<hd-progress [value]="50" label="Progreso de la orden">` | `tone="done"` paints the bar green; has `role="progressbar"` |
| `HdSpinnerComponent` `hd-spinner` | `<hd-spinner size="lg" label="Cargando">` | respects "reduce motion" |
| `HdLoadingComponent` `hd-loading` | `<hd-loading [active]="service.isLoading">` | a veil over a `position: relative` parent (`.box`, `.modal-content`) |
| `HdAlertComponent` `hd-alert` | `<hd-alert tone="info">…</hd-alert>` | tones `info`, `warn`, `error`, `success` |
| `HdEmptyComponent` `hd-empty` | `<hd-empty>No hay órdenes que mostrar.</hd-empty>` | |
| `HdButtonDirective` `[hdButton]` | `<button hdButton variant="primary" size="lg" [block]="true">` | on `<button>` or `<a>`; variants `primary`, `secondary`, `ghost`, `danger` |
| `HdDialogComponent` `hd-dialog` | `<hd-dialog dialogId="terms" title="…">body</hd-dialog>` | opened by `data-toggle="modal" data-target="#terms"` or `showModal()`; closes with Escape, the × and "Cerrar" |
| `HdIconComponent` `hd-icon` | `<hd-icon name="mail" [size]="20">` | the Lucide icons listed in `icons.ts` |

Not components, on purpose:

- **Form fields.** `FieldHintsDirective` (`components/shared/field-hints`) adds the `*` mark and the messages to every reactive control, and
  `.hd-form-grid` / `.form-group` lay them out. A wrapper component would duplicate that.
- **Tables.** The lists are plain `<table>` that turn into cards on the phone through CSS (`.hd-cards`, `.hd-devices`, `.hd-parts`).
- **Order, customer, user and company windows.** They are `.modal` blocks driven by `page-behaviors.ts` and many tests find them by id;
  `hd-dialog` is used for the simple ones (terms, calendar detail). Move the others when their forms are rewritten with reactive forms.

Other helpers in this folder: `page-behaviors.ts` (modals, tabs, collapse without jQuery), `theme.service.ts` (light, dark, system), `shell-state.service.ts` (side menu).
