type WebGlFallbackProps = {
  message?: string;
  onReset: () => void;
};

export function WebGlFallback({ message, onReset }: WebGlFallbackProps) {
  return (
    <section className="screen fallback-screen" aria-labelledby="fallback-title">
      <div className="screen-copy">
        <p className="eyebrow">Rendu indisponible</p>
        <h1 id="fallback-title">Impossible de lancer la balade</h1>
        <p className="lead">
          {message ??
            "Le navigateur ne fournit pas le support WebGL requis pour afficher l'experience ride."}
        </p>
      </div>

      <button className="primary-action" type="button" onClick={onReset}>
        Retour au depart
      </button>
    </section>
  );
}
