import { describe, it, expect, afterEach, vi } from "vitest";
import { renderHook } from "@testing-library/react";

// Stable spy shared with the mocked module (hoisted above vi.mock).
const mocks = vi.hoisted(() => ({
    getActiveAccount: vi.fn(),
}));

vi.mock("@azure/msal-react", () => ({
    useMsal: () => ({ instance: { getActiveAccount: mocks.getActiveAccount } }),
}));

import { useHasRole } from "hooks/useHasRole";

afterEach(() => {
    mocks.getActiveAccount.mockReset();
    vi.restoreAllMocks();
});

describe("useHasRole", () => {
    it("returns true when the active account has the role", () => {
        mocks.getActiveAccount.mockReturnValue({ idTokenClaims: { roles: ["Admin", "User"] } });

        const { result } = renderHook(() => useHasRole());

        expect(result.current("Admin")).toBe(true);
    });

    it("returns false when the active account does not have the role", () => {
        mocks.getActiveAccount.mockReturnValue({ idTokenClaims: { roles: ["User"] } });

        const { result } = renderHook(() => useHasRole());

        expect(result.current("Admin")).toBe(false);
    });

    it("returns false when there is no active account", () => {
        mocks.getActiveAccount.mockReturnValue(null);

        const { result } = renderHook(() => useHasRole());

        expect(result.current("Admin")).toBe(false);
    });

    it("returns false when the active account has no roles claim", () => {
        mocks.getActiveAccount.mockReturnValue({ idTokenClaims: {} });

        const { result } = renderHook(() => useHasRole());

        expect(result.current("Admin")).toBe(false);
    });
});
