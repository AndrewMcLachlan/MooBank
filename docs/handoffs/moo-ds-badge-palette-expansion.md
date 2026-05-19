# Badge — broader palette + custom colour support

Spec for the moo-ds (`@andrewmclachlan/moo-ds`) Badge component.

## Context

Badge currently supports six semantic background variants (`primary`, `secondary`, `success`, `danger`, `warning`, `info`) plus `muted`, `outline`, and icon support that have already been added. The palette is too small for consumers that need to differentiate ~10+ categories (e.g. MooBank's account types: Transaction, Savings, Credit, Mortgage, Loan, Superannuation, Investment, Shares, Broker, Asset, Virtual, Reserved Sum).

This change expands Badge in two ways:

1. **Named hue tokens** for the 95% case — coherent, themeable, autocomplete-friendly.
2. **Custom `colour` prop** as an escape hatch — accepts any CSS colour, no design-system release required to use.

## Goals

- Add 8–10 named hue tokens (`blue`, `teal`, `purple`, `indigo`, etc.) selectable via the existing `bg` prop.
- Accept a `colour` prop that takes any CSS colour and overrides `bg`.
- All new hues compose correctly with the existing `muted`, `outline`, `pill`, and icon support.
- Light and dark themes both look good.
- No breaking change to existing `bg="primary"` / `bg="success"` / etc. usage.

## Non-goals

- Replacing or renaming the existing semantic variants. Semantic and hue tokens coexist.
- Theming overrides per consumer app. Apps may override the CSS custom properties externally; that's already possible and remains supported.
- A separate `iconColour` prop — icon should inherit colour from the badge (see "Icons" below).

## API

```ts
type BadgeSemantic = "primary" | "secondary" | "success" | "danger" | "warning" | "info";

type BadgeHue =
  | "blue" | "indigo" | "purple" | "pink" | "rose"
  | "orange" | "amber" | "yellow"
  | "green" | "emerald" | "teal" | "cyan"
  | "slate" | "neutral";

export interface BadgeProps extends React.HTMLAttributes<HTMLSpanElement> {
  bg?: BadgeSemantic | BadgeHue;
  /**
   * CSS colour value. When provided, overrides `bg` and any class-based colour.
   * Examples: "#7c6cff", "rgb(124,108,255)", "var(--brand-teal)".
   */
  colour?: string;
  /**
   * Optional foreground colour, used together with `colour` when the auto
   * default doesn't suit (e.g. light custom colours that need dark text).
   * Ignored when `bg` (named token) is used.
   */
  textColour?: string;
  muted?: boolean;
  outline?: boolean;
  pill?: boolean;
  icon?: React.ReactNode;
}
```

Behavioural rules:

- `colour` takes precedence over `bg`. If both are supplied, `bg` is ignored.
- `muted` and `outline` are mutually exclusive — if both are passed, `outline` wins (document this; do not throw).
- `textColour` only applies when `colour` is also provided. It is ignored for named-token `bg` (named tokens own their own foreground).

## CSS architecture

The whole component renders via two CSS custom properties: `--badge-bg` (background) and `--badge-fg` (foreground). Every variant — semantic, hue, custom — works by setting those two properties. This makes `muted` / `outline` / `icon` colour logic generic.

```css
.badge {
  --badge-bg: var(--secondary);
  --badge-fg: #fff;

  display: inline-flex;
  align-items: center;
  gap: 0.35em;
  padding: 0.35em 0.65em;
  font-size: 0.75em;
  font-weight: 700;
  line-height: 1.3;
  border-radius: var(--badge-border-radius, 0.375rem);
  background: var(--badge-bg);
  color: var(--badge-fg);
}

.badge.rounded-pill { border-radius: 999px; }

/* Muted: tinted background, hue-as-text */
.badge.muted {
  background: var(--badge-bg-muted, color-mix(in srgb, var(--badge-bg) 16%, transparent));
  color: var(--badge-bg);
}

/* Outline: transparent background, hue border + text */
.badge.outline {
  background: transparent;
  color: var(--badge-bg);
  box-shadow: inset 0 0 0 1px var(--badge-bg);
}

/* Icon picks up the resolved foreground automatically */
.badge svg { color: currentColor; width: 1em; height: 1em; }
```

Named tokens are CSS classes that only set the two properties:

```css
/* Existing semantic — unchanged, just re-expressed in the token system */
.bg-primary   { --badge-bg: var(--primary);     --badge-fg: #fff; }
.bg-secondary { --badge-bg: #6c757d;            --badge-fg: #fff; }
.bg-success   { --badge-bg: var(--green);       --badge-fg: #fff; }
.bg-danger    { --badge-bg: var(--red);         --badge-fg: #fff; }
.bg-warning   { --badge-bg: var(--amber);       --badge-fg: #000; }
.bg-info      { --badge-bg: #0dcaf0;            --badge-fg: #000; }

/* New hue tokens — dark-mode friendly defaults */
.bg-blue     { --badge-bg: var(--hue-blue);    --badge-fg: #fff; }
.bg-indigo   { --badge-bg: var(--hue-indigo);  --badge-fg: #fff; }
.bg-purple   { --badge-bg: var(--hue-purple);  --badge-fg: #fff; }
.bg-pink     { --badge-bg: var(--hue-pink);    --badge-fg: #fff; }
.bg-rose     { --badge-bg: var(--hue-rose);    --badge-fg: #fff; }
.bg-orange   { --badge-bg: var(--hue-orange);  --badge-fg: #1a0f00; }
.bg-amber    { --badge-bg: var(--hue-amber);   --badge-fg: #1a1300; }
.bg-yellow   { --badge-bg: var(--hue-yellow);  --badge-fg: #1a1700; }
.bg-green    { --badge-bg: var(--hue-green);   --badge-fg: #fff; }
.bg-emerald  { --badge-bg: var(--hue-emerald); --badge-fg: #fff; }
.bg-teal     { --badge-bg: var(--hue-teal);    --badge-fg: #001a1f; }
.bg-cyan     { --badge-bg: var(--hue-cyan);    --badge-fg: #001a1f; }
.bg-slate    { --badge-bg: var(--hue-slate);   --badge-fg: #0a0f15; }
.bg-neutral  { --badge-bg: var(--hue-neutral); --badge-fg: #0a0f15; }
```

### Hue token values

Defined at `:root` in moo-ds CSS. These are dark-mode-first; light-mode overrides follow.

```css
:root {
  --hue-blue:    #4d8bff;
  --hue-indigo:  #7c6cff;
  --hue-purple:  #a070ff;
  --hue-pink:    #ec4899;
  --hue-rose:    #f06292;
  --hue-orange:  #ff8a4c;
  --hue-amber:   #f5c451;
  --hue-yellow:  #facc15;
  --hue-green:   #22c55e;
  --hue-emerald: #25c281;
  --hue-teal:    #2bc4d4;
  --hue-cyan:    #06b6d4;
  --hue-slate:   #8aa0b8;
  --hue-neutral: #94a3b8;
}

/* Light mode — slightly darker, saturated hues so muted variant has enough contrast on white */
.light, :root[data-theme="light"] {
  --hue-blue:    #2563eb;
  --hue-indigo:  #4f46e5;
  --hue-purple:  #7c3aed;
  --hue-pink:    #db2777;
  --hue-rose:    #e11d48;
  --hue-orange:  #ea580c;
  --hue-amber:   #d97706;
  --hue-yellow:  #ca8a04;
  --hue-green:   #16a34a;
  --hue-emerald: #059669;
  --hue-teal:    #0891b2;
  --hue-cyan:    #0e7490;
  --hue-slate:   #475569;
  --hue-neutral: #64748b;
}
```

(Adjust to taste; the existing `.light` / `.dark` selector pattern in moo-ds determines which selector wins.)

### Light-mode muted text contrast

`color-mix(in srgb, var(--badge-bg) 16%, transparent)` over a white surface produces a very pale tint where the hue itself may not meet AA against the tint. Add a light-mode override:

```css
.light .badge.muted, :root[data-theme="light"] .badge.muted {
  background: color-mix(in srgb, var(--badge-bg) 14%, white);
  color: color-mix(in srgb, var(--badge-bg) 78%, black);
}
```

## Custom `colour` prop — implementation

When `colour` is set, the component applies the two custom properties inline. No new classes needed; the existing CSS already reads from them.

```tsx
export const Badge = forwardRef<HTMLSpanElement, BadgeProps>(
  ({ bg, colour, textColour, muted, outline, pill, icon, className, children, style, ...rest }, ref) => {
    const useCustom = !!colour;
    const inlineStyle: React.CSSProperties | undefined = useCustom
      ? { ...style, "--badge-bg": colour, ...(textColour ? { "--badge-fg": textColour } : {}) } as React.CSSProperties
      : style;

    return (
      <span
        ref={ref}
        className={classNames(
          "badge",
          !useCustom && bg && `bg-${bg}`,
          muted && !outline && "muted",
          outline && "outline",
          pill && "rounded-pill",
          className,
        )}
        style={inlineStyle}
        {...rest}
      >
        {icon}
        {children}
      </span>
    );
  },
);
```

Default foreground when only `colour` is supplied: `#fff` (already the base value on `.badge`). Consumers needing dark text on a light custom colour pass `textColour="#000"` (or similar).

## Icons

Icons should inherit `currentColor`, which already resolves to `var(--badge-fg)`. No extra work required — works uniformly across solid / muted / outline / custom. Add a tiny gap (`gap: 0.35em` on `.badge`) so the icon sits cleanly next to the label.

## Backward compatibility

- The six existing semantic variants keep their current visual output. Anyone passing `bg="primary"` etc. sees no change.
- The `bg` prop type widens from `string` (which it already is) to a union; consumers that currently pass arbitrary strings still work because TypeScript allows widening but the public type signals the supported set. If strict typing is undesirable, keep `bg?: string` and add the union as a documentation hint with JSDoc `@see`.
- `muted`, `outline`, `pill`, `icon` behaviour unchanged.

## Storybook

Add stories under the existing Badge group:

1. **All hues — solid** — grid showing every named hue with the type label.
2. **All hues — muted** — same grid, `muted` enabled.
3. **All hues — outline** — same grid, `outline` enabled.
4. **Light mode contrast** — the same three above rendered under `.light` to verify legibility.
5. **Custom colour** — examples with `colour` set to `#7c6cff`, a CSS var, and an HSL value; one with `textColour` override.
6. **With icon** — one of each variant style, with a sample icon.
7. **Composition** — `muted + pill + icon`, `outline + pill`, etc.

## Acceptance criteria

- All named hues render correctly in solid, muted, and outline styles under both light and dark themes.
- `colour` prop accepts any CSS colour value and renders without console warnings.
- `colour` overrides `bg` when both are supplied (verified by test).
- Icon picks up the correct foreground colour in every variant without explicit colour props.
- No regression to existing `bg="primary"` through `bg="info"` rendering — visual snapshot tests pass unchanged for those cases.
- TypeScript autocomplete suggests all named hues and semantic variants.

## Out of scope (future)

- Size variants (sm / md / lg) — separate spec.
- Status dot prefix (some design systems put a small filled circle before the label) — separate spec.
- Closable variant beyond the existing `CloseBadge`.
