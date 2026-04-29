import { useState } from 'react';
import { parseText } from '../services/importApi';
import type { ParseResponse } from '../types/importTypes';

interface ImportParserState {
  input: string;
  result: ParseResponse | null;
  error: string | null;
  loading: boolean;
}

interface ImportParserActions {
  setInput: (value: string) => void;
  submit: () => Promise<void>;
  clear: () => void;
}

export function useImportParser(): ImportParserState & ImportParserActions {
  const [input, setInput] = useState('');
  const [result, setResult] = useState<ParseResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit() {
    if (!input.trim()) return;

    setLoading(true);
    setResult(null);
    setError(null);

    try {
      const response = await parseText(input);
      setResult(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred.');
    } finally {
      setLoading(false);
    }
  }

  function clear() {
    setInput('');
    setResult(null);
    setError(null);
  }

  return { input, result, error, loading, setInput, submit, clear };
}
