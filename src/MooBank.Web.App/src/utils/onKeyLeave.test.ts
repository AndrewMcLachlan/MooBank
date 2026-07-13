import { describe, it, expect, vi } from "vitest";
import { onKeyLeave } from "utils/onKeyLeave";

const makeEvent = (key: string, value: string) =>
    ({ key, currentTarget: { value } }) as unknown as React.KeyboardEvent<HTMLInputElement>;

describe("onKeyLeave", () => {
    it("calls the setter with the current value on Enter", () => {
        const setter = vi.fn();
        onKeyLeave(makeEvent("Enter", "hello"), setter);
        expect(setter).toHaveBeenCalledWith("hello");
        expect(setter).toHaveBeenCalledTimes(1);
    });

    it("calls the setter with the current value on Tab", () => {
        const setter = vi.fn();
        onKeyLeave(makeEvent("Tab", "world"), setter);
        expect(setter).toHaveBeenCalledWith("world");
        expect(setter).toHaveBeenCalledTimes(1);
    });

    it("does nothing for other keys", () => {
        const setter = vi.fn();
        onKeyLeave(makeEvent("a", "hello"), setter);
        expect(setter).not.toHaveBeenCalled();
    });
});
