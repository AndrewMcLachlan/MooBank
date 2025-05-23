import { useTheme } from "@andrewmclachlan/mooapp"

export const useChartColours = () => {

    const { theme, defaultTheme } = useTheme();
    const themeName = theme.theme === "" ? defaultTheme.theme : theme.theme;

    return {
        income: themeName.startsWith("dark") ? "color-mix(in srgb, #33CA36 75%, black)" : "#33CA36",
        incomeTrend:themeName.startsWith("dark") ? "#99fb99" : "#bbfbbb",
        expenses: themeName.startsWith("dark") ? "color-mix(in srgb, #FC2D3B 75%, black)" : "#FC2D3B",
        expensesTrend: themeName.startsWith("dark") ? "#FF7790" : "#FFAAC0",
        neutralTrend: themeName.startsWith("dark") ? "#808080" : "#b0b0b0",
        grid: themeName.startsWith("dark") ? "#333" : "#E5E5E5",
    };
}

export const chartColours = [
    "#003f5c",
    "#2f4b7c",
    "#665191",
    "#a05195",
    "#d45087",
    "#f95d6a",
    "#ff7c43",
    "#ffa600",
    "#00876c",
    "#3d996e",
    "#62aa6f",
    "#86ba71",
    "#acca74",
    "#d2d87a",
    "#fae684",
    "#f9cb6e",
    "#f7b15d",
    "#f29553",
    "#eb794e",
    "#e15c4e",
    "#d43d51",
]

export const desaturatedChartColours = [
    "#1c3540",
    "#465165",
    "#6d647e",
    "#6d647e",
    "#ac788e",
    "#ca8c91",
    "#c7927b",
    "#b38f4d",
    "#285e54",
    "#3d996e",
    "#597d6c",
    "#8fa487",
    "#acca74",
    "#a4b08e",
    "#d7cfa7",
    "#cfbd98",
    "#c9ad8b",
    "#c29d83",
    "#bc8e7d",
    "#b5807a",
    "#a76a72",
]
