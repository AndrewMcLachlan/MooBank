import type { CreateClientConfig } from '../api/client.gen'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  // The OpenAPI document declares an /api server and its operation paths are relative
  // (e.g. /accounts), so the client base URL carries the /api prefix.
  baseURL: "/api",
  withCredentials: true,
});
