import { describe, it, expect } from "vitest";
import type { LogicalAccount, VirtualInstrument } from "api/types.gen";
import { isVirtualInstrument } from "utils/virtualAccounts";

describe("isVirtualInstrument", () => {
    it("is true when parentId is present", () => {
        const virtualInstrument = { id: "1", parentId: "parent-1" } as unknown as VirtualInstrument;
        expect(isVirtualInstrument(virtualInstrument)).toBe(true);
    });

    it("is false when parentId is absent", () => {
        const logicalAccount = { id: "1" } as unknown as LogicalAccount;
        expect(isVirtualInstrument(logicalAccount)).toBe(false);
    });

    it("is false when parentId is an empty string", () => {
        const virtualInstrument = { id: "1", parentId: "" } as unknown as VirtualInstrument;
        expect(isVirtualInstrument(virtualInstrument)).toBe(false);
    });
});
