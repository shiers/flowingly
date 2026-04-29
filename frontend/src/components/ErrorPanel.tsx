import type { ValidationErrorDto } from '../types/importTypes';

interface ErrorPanelProps {
  /** Validation errors returned by the API (structured) */
  validationErrors?: ValidationErrorDto[];
  /** Network or unexpected errors (plain string) */
  networkError?: string | null;
}

export function ErrorPanel({ validationErrors, networkError }: ErrorPanelProps) {
  if (networkError) {
    return (
      <div className="error-panel" role="alert">
        <h2 className="error-heading">Error</h2>
        <p className="error-network">{networkError}</p>
      </div>
    );
  }

  if (!validationErrors || validationErrors.length === 0) return null;

  return (
    <div className="error-panel" role="alert">
      <h2 className="error-heading">Validation Errors</h2>
      <ul className="error-list">
        {validationErrors.map((err) => (
          <li key={err.code} className="error-item">
            <span className="error-code">{err.code}</span>
            <span className="error-message">{err.message}</span>
          </li>
        ))}
      </ul>
    </div>
  );
}
