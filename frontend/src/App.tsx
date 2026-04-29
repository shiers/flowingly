import flowinglyLogo from './assets/flowingly-logo-23.svg';
import { useImportParser } from './hooks/useImportParser';
import { TextInputPanel } from './components/TextInputPanel';
import { ResultPanel } from './components/ResultPanel';
import { ErrorPanel } from './components/ErrorPanel';
import './App.css';

export default function App() {
  const { input, result, error, loading, setInput, submit, clear } = useImportParser();

  const showResult = result?.success && result.data != null;
  const showValidationErrors = result != null && !result.success && result.errors.length > 0;

  return (
    <div className="app">
      <header className="app-header">
        <img src={flowinglyLogo} alt="Flowingly" className="app-logo" />
        <h1 className="app-title">Flowingly Import Parser</h1>
        <p className="app-subtitle">Paste email content to extract and validate expense fields</p>
      </header>

      <main className="app-main">
        <TextInputPanel
          value={input}
          loading={loading}
          onChange={setInput}
          onSubmit={submit}
          onClear={clear}
        />

        {error && (
          <ErrorPanel networkError={error} />
        )}

        {showValidationErrors && (
          <ErrorPanel validationErrors={result!.errors} />
        )}

        {showResult && (
          <ResultPanel result={result!} />
        )}
      </main>
    </div>
  );
}
