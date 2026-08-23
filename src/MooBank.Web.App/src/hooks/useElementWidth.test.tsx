import { describe, it, expect, beforeEach, afterEach, vi } from "vitest";
import { useState } from "react";
import { render, act, screen, fireEvent } from "@testing-library/react";
import { useElementWidth } from "./useElementWidth";

/**
 * jsdom has no ResizeObserver and does no layout, so it is stubbed and the test drives the callback
 * the hook registered.
 */
let trigger: ((entries: { contentRect: { width: number } }[]) => void) | undefined;
const disconnect = vi.fn();
const observe = vi.fn();

beforeEach(() => {
    trigger = undefined;
    disconnect.mockClear();
    observe.mockClear();

    vi.stubGlobal("ResizeObserver", class {
        constructor(callback: (entries: { contentRect: { width: number } }[]) => void) {
            trigger = callback;
        }
        observe = observe;
        disconnect = disconnect;
    });

    // The hook coalesces into a frame; run it straight away rather than making the test wait.
    vi.stubGlobal("requestAnimationFrame", (cb: FrameRequestCallback) => { cb(0); return 0; });
    vi.stubGlobal("cancelAnimationFrame", () => { });
});

afterEach(() => {
    vi.unstubAllGlobals();
    // The measure-on-attach test spies on getBoundingClientRect; without this it stays spied and
    // the next test measures 480 instead of jsdom's 0.
    vi.restoreAllMocks();
});

const Measured: React.FC = () => {
    const [ref, width] = useElementWidth();
    return <div ref={ref} data-testid="box">{width === null ? "unmeasured" : String(width)}</div>;
};

/** Holds the element back until asked to show it, the way a chart waits on its data. */
const Deferred: React.FC = () => {
    const [ref, width] = useElementWidth();
    const [ready, setReady] = useState(false);

    return (
        <>
            <button onClick={() => setReady(true)}>load</button>
            {ready
                ? <div ref={ref} data-testid="box">{width === null ? "unmeasured" : String(width)}</div>
                : <span data-testid="box">loading</span>}
        </>
    );
};

describe("useElementWidth", () => {
    it("reports nothing until the element has been measured", () => {
        render(<Measured />);

        expect(screen.getByTestId("box")).toHaveTextContent("unmeasured");
    });

    it("measures on attach rather than waiting for the observer", () => {
        // Consumers render differently once the width is known, so it has to be known before the
        // browser paints. A width that only arrives on the observer's first callback means one
        // painted frame at the wrong size, which for a chart is a visible redraw.
        vi.spyOn(HTMLElement.prototype, "getBoundingClientRect")
            .mockReturnValue({ width: 480 } as DOMRect);

        render(<Measured />);

        expect(screen.getByTestId("box")).toHaveTextContent("480");
        expect(trigger).toBeDefined();
    });

    it("observes the element it is attached to", () => {
        render(<Measured />);

        expect(observe).toHaveBeenCalledOnce();
    });

    it("reports the observed width", () => {
        render(<Measured />);

        act(() => trigger?.([{ contentRect: { width: 640 } }]));

        expect(screen.getByTestId("box")).toHaveTextContent("640");
    });

    it("measures an element that only appears after the first render", () => {
        // The bug this hook shipped with. Breakdown returns a skeleton until its report loads, so
        // the measured element does not exist on the first render. An effect with an empty
        // dependency array runs once, finds nothing and never looks again, leaving the width null
        // forever — which read as the legend simply never moving.
        render(<Deferred />);

        expect(observe).not.toHaveBeenCalled();

        fireEvent.click(screen.getByRole("button", { name: "load" }));
        act(() => trigger?.([{ contentRect: { width: 320 } }]));

        expect(observe).toHaveBeenCalledOnce();
        expect(screen.getByTestId("box")).toHaveTextContent("320");
    });

    it("ignores a zero width rather than reporting the element as tiny", () => {
        // A detached or display:none element measures 0. Treating that as real would flip a chart's
        // legend on the frame before it is laid out.
        render(<Measured />);

        act(() => trigger?.([{ contentRect: { width: 0 } }]));

        expect(screen.getByTestId("box")).toHaveTextContent("unmeasured");
    });

    it("stops observing when the element goes away", () => {
        const { unmount } = render(<Measured />);

        unmount();

        expect(disconnect).toHaveBeenCalled();
    });
});
