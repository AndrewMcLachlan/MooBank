import { createRootRoute } from "@tanstack/react-router";
import Layout from "../Layout";

export const Route = createRootRoute({
  component: Layout //() => import("../App").then((mod) => mod.default)
});
