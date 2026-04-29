import type { ParseResponse } from '../types/importTypes';

interface ResultPanelProps {
  result: ParseResponse;
}

export function ResultPanel({ result }: ResultPanelProps) {
  return (
    <div className="result-panel">
      <h2 className="result-heading">Parsed Result</h2>
      <pre className="result-json" aria-label="Parsed JSON output">
        {JSON.stringify(result.data, null, 2)}
      </pre>
    </div>
  );
}
