interface TextInputPanelProps {
  value: string;
  taxRate: number;
  loading: boolean;
  onChange: (value: string) => void;
  onTaxRateChange: (value: number) => void;
  onSubmit: () => void;
  onClear: () => void;
}

export function TextInputPanel({
  value,
  taxRate,
  loading,
  onChange,
  onTaxRateChange,
  onSubmit,
  onClear,
}: TextInputPanelProps) {
  function handleTaxRateChange(e: React.ChangeEvent<HTMLInputElement>) {
    const parsed = parseFloat(e.target.value);
    if (!isNaN(parsed) && parsed > 0 && parsed <= 100) {
      onTaxRateChange(parsed);
    }
  }

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
      <div className="tax-rate-row">
        <label htmlFor="tax-rate-input" className="input-label">
          Tax rate (%)
        </label>
        <input
          id="tax-rate-input"
          type="number"
          className="tax-rate-input"
          value={taxRate}
          min={0.01}
          max={100}
          step={0.01}
          onChange={handleTaxRateChange}
          disabled={loading}
          aria-label="Tax rate percentage"
        />
      </div>
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
