import type { CreateClientConfig } from '../api/client.gen'

export const createClientConfig: CreateClientConfig = (config) => ({
  ...config,
  baseURL: "",
  withCredentials: true,
});
