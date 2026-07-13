import "@testing-library/jest-dom/vitest";
import { cleanup } from "@testing-library/react";
import { afterEach } from "vitest";

// React Testing Library does not auto-clean between tests under Vitest globals; do it explicitly
// so each test renders into a fresh DOM.
afterEach(() => {
    cleanup();
});
