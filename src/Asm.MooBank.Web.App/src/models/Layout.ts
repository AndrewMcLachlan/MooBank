export interface Layout {
    theme?: Theme;
    setTheme?: (theme?: Theme) => void;
    defaultTheme: Theme;
}

export type Theme = "dark" | "light";