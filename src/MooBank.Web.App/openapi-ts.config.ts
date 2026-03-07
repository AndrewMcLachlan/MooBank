import { defineConfig } from '@hey-api/openapi-ts';

export default defineConfig({
    input: '../Asm.MooBank.Web.Api/openapi-v1.json',
    output: './src/api',
    plugins: [
        {
            name: '@hey-api/client-axios',
            runtimeConfigPath: '../utils/axios-config.ts',
            bundle: false,
        },
        '@tanstack/react-query'
    ],
});