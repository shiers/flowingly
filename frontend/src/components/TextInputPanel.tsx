interface TextInputPanelProps {
  value: string;
  loading: boolean;
  onChange: (value: string) => void;
  onSubmit: () => void;
  onClear: () => void;
}

export function TextInputPanel({
  value,
  loading,
  onChange,
  onSubmit,
  onClear,
}: TextInputPanelProps) {
  return (
    <div className="input-panel">
      <label htmlFor="import-input" className="input-label">
        Paste email or text content
      </label>
      <textarea
        id="import-input"
        className="input-textarea"
        value={value}
        onChange={(e) => onChange(e.target.value)}
        placeholder={`Hi Patricia, please create an expense claim\n<expense>\n  <cost_centre>DEV632</cost_centre>\n  <total>35,000</total>\n  <payment_method>personal card</payment_method>\n</expense>`}
        rows={10}
        disabled={loading}
        aria-label="Email or text input"
      />
      <div className="input-actions">
        <button
          type="button"
          className="btn btn-primary"
          onClick={onSubmit}
          disabled={loading || !value.trim()}
          aria-busy={loading}
        >
          {loading ? 'Parsing…' : 'Submit'}
        </button>
        <button
          type="button"
          className="btn btn-secondary"
          onClick={onClear}
          disabled={loading}
        >
          Clear
        </button>
      </div>
    </div>
  );
}
