export default function TourProgram({ days }) {
  return (
    <section className="tour-section">
      <h2>Программа тура</h2>

      <div className="program-list">
        {days.map((day, index) => (
          <div key={index} className="program-item">
            <h3>День {index + 1}</h3>
            <p>{day || 'Описание пока не добавлено.'}</p>
          </div>
        ))}
      </div>
    </section>
  );
}