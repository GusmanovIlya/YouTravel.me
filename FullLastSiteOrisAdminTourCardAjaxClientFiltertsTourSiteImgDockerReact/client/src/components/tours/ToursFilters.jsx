export default function ToursFilters({ filters, onChange }) {
  function setField(name, value) {
    onChange({ ...filters, [name]: value });
  }

  return (
    <aside className="filters-sidebar">
      <h3>Фильтры</h3>

      <div className="filter-group">
        <label>Страна или тур</label>
        <input
          value={filters.country}
          onChange={(e) => setField('country', e.target.value)}
          placeholder="Например, Кения"
        />
      </div>

      <div className="filter-group">
        <label>Цена от</label>
        <input
          type="number"
          value={filters.priceMin}
          onChange={(e) => setField('priceMin', e.target.value)}
        />
      </div>

      <div className="filter-group">
        <label>Цена до</label>
        <input
          type="number"
          value={filters.priceMax}
          onChange={(e) => setField('priceMax', e.target.value)}
        />
      </div>

      <div className="filter-group">
        <label>Длительность</label>
        <select
          value={filters.duration}
          onChange={(e) => setField('duration', e.target.value)}
        >
          <option value="">Любая</option>
          <option value="7-10">7–10 дней</option>
          <option value="11-14">11–14 дней</option>
          <option value="15+">15+ дней</option>
        </select>
      </div>

      <div className="filter-group">
        <label>Комфорт</label>
        <select
          value={filters.comfort}
          onChange={(e) => setField('comfort', e.target.value)}
        >
          <option value="">Любой</option>
          <option value="1">1</option>
          <option value="2">2</option>
          <option value="3">3</option>
          <option value="4">4</option>
          <option value="5">5</option>
        </select>
      </div>

      <div className="filter-group">
        <label>Активность</label>
        <select
          value={filters.activity}
          onChange={(e) => setField('activity', e.target.value)}
        >
          <option value="">Любая</option>
          <option value="1">1</option>
          <option value="2">2</option>
          <option value="3">3</option>
          <option value="4">4</option>
          <option value="5">5</option>
        </select>
      </div>

      <div className="filter-group">
        <label className="checkbox-label">
          <input
            type="checkbox"
            checked={filters.discount}
            onChange={(e) => setField('discount', e.target.checked)}
          />
          Только со скидкой
        </label>
      </div>
    </aside>
  );
}