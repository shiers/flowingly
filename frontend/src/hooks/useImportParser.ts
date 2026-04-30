import { useState } from 'react';
import { parseText } from '../services/importApi';
import type { ParseResponse } from '../types/importTypes';

const DEFAULT_TAX_RATE = 15;

interface ImportParserState {
  input: string;
  taxRate: number;
  result: ParseResponse | null;
  error: string | null;
  loading: boolean;
}

interface ImportParserActions {
  setInput: (value: string) => void;
  setTaxRate: (value: number) => void;
  submit: () => Promise<void>;
  clear: () => void;
}

export function useImportParser(): ImportParserState & ImportParserActions {
  const [input, setInput] = useState('');
  const [taxRate, setTaxRate] = useState(DEFAULT_TAX_RATE);
  const [result, setResult] = useState<ParseResponse | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [loading, setLoading] = useState(false);

  async function submit() {
    if (!input.trim()) return;

    setLoading(true);
    setResult(null);
    setError(null);

    try {
      const response = await parseText(input, taxRate);
      setResult(response);
    } catch (err) {
      setError(err instanceof Error ? err.message : 'An unexpected error occurred.');
    } finally {
      setLoading(false);
    }
  }

  function clear() {
    setInput('');
    setTaxRate(DEFAULT_TAX_RATE);
    setResult(null);
    setError(null);
  }

  return { input, taxRate, result, error, loading, setInput, setTaxRate, submit, clear };
}
