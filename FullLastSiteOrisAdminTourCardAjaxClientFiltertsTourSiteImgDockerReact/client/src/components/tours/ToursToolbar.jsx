export default function ToursToolbar({ total, sort, onSortChange }) {
  return (
    <div className="top-panel">
      <div>Найдено туров: {total}</div>

      <select value={sort} onChange={(e) => onSortChange(e.target.value)}>
        <option value="popularity">По популярности</option>
        <option value="price_asc">Сначала дешевле</option>
        <option value="price_desc">Сначала дороже</option>
        <option value="rating_desc">По рейтингу</option>
        <option value="duration_asc">По длительности</option>
      </select>
    </div>
  );
}