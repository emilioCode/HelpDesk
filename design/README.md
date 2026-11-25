# HelpDesk design

The visual design of HelpDesk, drawn in [Pencil](https://pencil.dev) (pen.dev). The source of truth is `helpdesk.pen`.

This folder holds the design only. Nothing here is wired into the application yet: the implementation comes later, in waves, against these screens.

## Open it

1. Install the **pen.dev** extension in VS Code (or Cursor).
2. Open `design/helpdesk.pen`. The file is edited only through Pencil (it is not meant to be read or merged as text).

## What is inside (48 top-level frames)

| Group | Frames |
| --- | --- |
| Foundations | `Foundations`: color (light and dark), type scale, spacing, radii |
| Components | `Components`: reusable components and their variants |
| Access | Sign in, session expired, access denied |
| Home | `Inicio` |
| Orders | List (normal, loading, empty), inbox, unassigned (all, workshop, on-site), completed |
| Order form | General, new order with validation, completed (read only), activities, parts, confirm parts dialog |
| Calendar | Month, loading |
| Management | Customers (list, add/edit), users (list, add/edit), company, my profile, reports |
| Cross-cutting | Network error, empty search, disabled record |
| Dark mode | `Inicio`, orders list, order form, calendar |
| Mobile (390 px) | `Mobile components`, and 13 phone screens: home, menu open, inbox (and loading), sign in, order form (general, activities, parts, confirm parts sheet), calendar agenda, and the customer tracking page (in progress, approved, invalid link) |

Desktop screens are laid out in a grid below the components; the mobile frames sit to the right of that grid. The UI text is Spanish, as in the application.

## Tokens

All colors, spacing and radii are variables, never raw values. Colors have a `light` and a `dark` value (theme axis `mode`); a frame with `theme: { mode: "dark" }` renders the dark values.

| Group | Variables |
| --- | --- |
| Surfaces | `--bg`, `--surface`, `--head`, `--line`, `--line-strong`, `--scrim` |
| Text | `--ink`, `--muted` |
| Brand | `--accent`, `--accent-soft`, `--accent-ink`, `--focus` |
| Feedback | `--danger`; order states `--open`, `--prog`, `--done`, `--appr`, each with `-fg` and `-bg` |
| Type | `--font-primary` (Inter) |
| Space | `--space-xs` 4, `-sm` 8, `-md` 12, `-lg` 16, `-xl` 24, `-2xl` 32, `-3xl` 48 |
| Radius | `--radius-sm` 8, `-md` 12, `-lg` 16, `-pill` 999 |

Text and background pairs are chosen for WCAG AA contrast (4.5:1) in both modes. An order state is always shown with text and color, never color alone.

## Components

Mobile components: top bar, action bar, order card, timeline item and a four-step progress indicator (customer tracking).

Desktop components: button (primary, secondary, ghost, danger, disabled, in two sizes), status badge, nav item, tab, field (default, focus, error, disabled, with the required mark `*`, helper text and character counter), alert (success, info, warning, error), dialog, KPI card, search box, calendar event, table header and row, progress bar, spinner and loading state.

## Conventions

- Screens are built from component instances; a change to a component changes every screen.
- Every node has a readable name.
- Required fields carry `*`; limited text areas show a live counter (`used / max`) that mirrors the backend rules.
- Lists have a loading state (spinner) and an empty state.
- Icons are [Lucide](https://lucide.dev) (ISC license). The font is [Inter](https://rsms.me/inter/) (SIL Open Font License), to be self-hosted in the application.
- The "H" in the sidebar and on the sign-in card is a placeholder, not a logo.

## Decisions to review

- The sign-in labels are in Spanish (the application shows them in English today).
- Dark mode is designed for the four key screens; the tokens support the rest.
- The license and file format of `.pen` have not been reviewed.

## Next

Implementation by waves, validated with the browser battery and screenshots after each one: tokens and structure first, then base components and simple screens, then orders, then calendar and reports, and finally the removal of AdminLTE, Bootstrap 3 and jQuery.
