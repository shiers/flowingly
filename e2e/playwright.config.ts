import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  timeout: 30_000,
  retries: 0,

  use: {
    baseURL: 'http://localhost:5173',
    headless: true,
    ...devices['Desktop Chrome'],
  },
});
