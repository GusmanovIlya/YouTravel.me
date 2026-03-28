const API_BASE = '';

export async function getTours(filters) {
  const params = new URLSearchParams();

  if (filters.country) params.set('country', filters.country);
  if (filters.priceMin) params.set('price_min', filters.priceMin);
  if (filters.priceMax) params.set('price_max', filters.priceMax);
  if (filters.duration) params.set('duration', filters.duration);
  if (filters.activity) params.set('activity', filters.activity);
  if (filters.comfort) params.set('comfort', filters.comfort);
  if (filters.discount) params.set('discount', '1');
  if (filters.sort) params.set('sort', filters.sort);

  const response = await fetch(`${API_BASE}/api/tours?${params.toString()}`);
  if (!response.ok) {
    throw new Error('Не удалось загрузить туры');
  }

  return await response.json();
}

export async function getTourById(id) {
  const response = await fetch(`${API_BASE}/api/tours/${id}`);
  if (!response.ok) {
    throw new Error('Не удалось загрузить тур');
  }

  return await response.json();
}