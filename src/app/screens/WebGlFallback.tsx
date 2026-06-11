import { COPY } from "../../ui/copy";

type WebGlFallbackProps = {
  message?: string;
  onReset: () => void;
};

export function WebGlFallback({ message, onReset }: WebGlFallbackProps) {
  return (
    <section className="screen fallback-screen" aria-labelledby="fallback-title">
      <div className="screen-copy">
        <p className="eyebrow">{COPY["error.webgl.eyebrow"]}</p>
        <h1 id="fallback-title">{COPY["error.webgl.title"]}</h1>
        <p className="lead">
          {message ?? COPY["error.webgl.default"]}
        </p>
      </div>

      <button className="primary-action" type="button" onClick={onReset}>
        {COPY["summary.button"]}
      </button>
    </section>
  );
}
