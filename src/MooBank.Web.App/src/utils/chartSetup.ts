import { Chart, registerables } from "chart.js";
import type { Plugin } from "chart.js";
import annotationPlugin from "chartjs-plugin-annotation";
import chartTrendline from "chartjs-plugin-trendline";

// Wrap the annotation plugin to guard against beforeUpdate firing
// on charts whose state was already cleaned up by afterDestroy.
// This is a race condition between react-chartjs-2's update and
// cleanup effects during React component unmount.
const safeAnnotationPlugin: typeof annotationPlugin = {
    ...annotationPlugin,
    beforeUpdate(chart, args, options) {
        try {
            return annotationPlugin.beforeUpdate(chart, args, options);
        } catch {
            // State was already cleaned up by afterDestroy — chart is being unmounted
        }
    },
};

// Show a pointer cursor when the mouse is over a data element on a chart
// that has declared a Chart.js `options.onClick` handler. Charts whose click
// behaviour is wired via the React `onClick` prop won't surface here — they
// need to use Chart.js's own `onClick` option to opt in.
const pointerCursorPlugin: Plugin = {
    id: "pointerCursor",
    afterEvent(chart, args) {
        const event = args.event;
        if (event.type !== "mousemove") return;
        const canvas = chart.canvas;
        if (!canvas || typeof chart.options.onClick !== "function") return;
        const nativeEvent = (event.native ?? event) as Event;
        const elements = chart.getElementsAtEventForMode(
            nativeEvent,
            "nearest",
            { intersect: true },
            false,
        );
        canvas.style.cursor = elements.length > 0 ? "pointer" : "default";
    },
};

Chart.register(...registerables, safeAnnotationPlugin, chartTrendline, pointerCursorPlugin);
