import { Chart, registerables } from "chart.js";
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

Chart.register(...registerables, safeAnnotationPlugin, chartTrendline);
