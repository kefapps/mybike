type StartScreenProps = {
  onStart: () => void;
};

export function StartScreen({ onStart }: StartScreenProps) {
  return (
    <section className="screen start-screen" aria-labelledby="start-title">
      <div className="screen-copy">
        <p className="eyebrow">Vertical slice mock</p>
        <h1 id="start-title">Echappee 3D</h1>
        <p className="lead">Premiere sortie mock, cap clair, rythme doux.</p>
      </div>

      <button className="primary-action" type="button" onClick={onStart}>
        Lancer la balade mock
      </button>
    </section>
  );
}
