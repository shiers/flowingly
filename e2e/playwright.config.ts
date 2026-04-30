import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: '.',
  timeout: 30_000,
  retries: 0,

  // Start both servers automatically before the test run and stop them after.
  // This means `npm test` in the e2e folder works without any manual setup.
  webServer: [
    {
      command: 'dotnet run --urls http://localhost:5000',
      cwd: '../backend/Flowingly.Import.Api',
      url: 'http://localhost:5000/swagger/index.html',
      reuseExistingServer: true,
      timeout: 60_000,
    },
    {
      command: 'npm run dev',
      cwd: '../frontend',
      url: 'http://localhost:5173',
      reuseExistingServer: true,
      timeout: 30_000,
    },
  ],

  use: {
    baseURL: 'http://localhost:5173',
    headless: true,
    ...devices['Desktop Chrome'],
  },
});
