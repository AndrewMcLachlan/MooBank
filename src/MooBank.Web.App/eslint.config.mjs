import js from "@eslint/js";
import react from "@eslint-react/eslint-plugin";
import reactHooks from "eslint-plugin-react-hooks";
import globals from "globals";
import tseslint from "typescript-eslint";

export default tseslint.config(
    {
        ignores: [
            "dist/**",
            "node_modules/**",
            "**/*.gen.ts",
            "**/routeTree.gen.ts",
            "**/routes.gen.ts",
            "**/react-app-env.d.ts",
        ],
    },
    js.configs.recommended,
    tseslint.configs.recommended,
    {
        files: ["**/*.{ts,tsx}"],
        extends: [
            react.configs["recommended-typescript"],
            reactHooks.configs.flat.recommended,
        ],
        languageOptions: {
            globals: {
                ...globals.browser,
            },
            ecmaVersion: "latest",
            sourceType: "module",
        },
        rules: {
            "@typescript-eslint/no-explicit-any": "off",
            "@typescript-eslint/no-empty-object-type": "off",

            "@typescript-eslint/no-unused-vars": ["warn", {
                argsIgnorePattern: "^_",
                varsIgnorePattern: "^_",
                caughtErrorsIgnorePattern: "^_",
            }],

            // @ts-expect-error can't be used where strictNullChecks:false means the
            // next line may compile cleanly; allow documented @ts-ignore instead.
            "@typescript-eslint/ban-ts-comment": ["error", {
                "ts-ignore": "allow-with-description",
            }],

            // New compiler-powered rules in eslint-plugin-react-hooks v7 that flag
            // long-standing patterns across the codebase. Downgraded to warnings
            // until the affected components are refactored.
            "react-hooks/set-state-in-effect": "warn",
            "react-hooks/immutability": "warn",

            // Off: duplicates of rules already reported by eslint-plugin-react-hooks.
            "@eslint-react/exhaustive-deps": "off",
            "@eslint-react/set-state-in-effect": "off",
        },
    },
);
