import type { ParseResponse } from '../types/importTypes';

const API_BASE = '/api/import';

export async function parseText(text: string): Promise<ParseResponse> {
  const response = await fetch(`${API_BASE}/parse`, {
    method: 'POST',
    headers: { 'Content-Type': 'application/json' },
    body: JSON.stringify({ text }),
  });

  if (!response.ok && response.status !== 422) {
    throw new Error(`Unexpected server error: ${response.status}`);
  }

  return response.json() as Promise<ParseResponse>;
}
