import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: 'http://localhost:5005/openapi/v1.json',
    output: './src/api',
    plugins: [
        {
            name: '@hey-api/client-axios',
            runtimeConfigPath: '../utils/axios-config.ts',
        },
        '@tanstack/react-query'
    ],
});