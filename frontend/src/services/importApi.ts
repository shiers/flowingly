import type { ParseResponse } from '../types/importTypes';

// In local dev (Vite proxy) and Docker Compose (nginx proxy), VITE_API_URL is not set
// and requests go to /api which is proxied to the backend.
// In Azure (Static Web Apps), VITE_API_URL is set to the Container Apps backend URL.
const API_BASE = `${import.meta.env.VITE_API_URL ?? ''}/api/import`;

export async function parseText(text: string, taxRatePercent?: number): Promise<ParseResponse> {
  const response = await fetch(`${API_BASE}/parse`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text, taxRatePercent: taxRatePercent ?? null }),
  });

  if (!response.ok && response.status !== 422) {
    throw new Error(`Unexpected server error: ${response.status}`);
  }

  return response.json() as Promise<ParseResponse>;
}
