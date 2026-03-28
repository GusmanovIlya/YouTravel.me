import { useEffect, useState } from 'react';
import Header from '../components/layout/Header';
import Breadcrumbs from '../components/layout/Breadcrumbs';
import HeroBanner from '../components/tours/HeroBanner';
import PromoBlocks from '../components/tours/PromoBlocks';
import ToursToolbar from '../components/tours/ToursToolbar';
import ToursFilters from '../components/tours/ToursFilters';
import ToursList from '../components/tours/ToursList';
import Loader from '../components/common/Loader';
import { getTours } from '../../../client/src/api/toursApi';

const initialFilters = {
  country: '',
  priceMin: '',
  priceMax: '',
  duration: '',
  comfort: '',
  activity: '',
  discount: false,
  sort: 'popularity'
};

export default function HomePage() {
  const [filters, setFilters] = useState(initialFilters);
  const [tours, setTours] = useState([]);
  const [total, setTotal] = useState(0);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTours();
  }, [filters]);

  async function loadTours() {
    try {
      setLoading(true);
      const data = await getTours(filters);
      setTours(data.items ?? []);
      setTotal(data.total ?? 0);
    } catch (error) {
      console.error(error);
      setTours([]);
      setTotal(0);
    } finally {
      setLoading(false);
    }
  }

  return (
    <>
      <Header />
      <Breadcrumbs items={[{ label: 'Главная' }, { label: 'Туры' }]} />
      <HeroBanner />
      <PromoBlocks />
      <ToursToolbar
        total={total}
        sort={filters.sort}
        onSortChange={(value) => setFilters({ ...filters, sort: value })}
      />

      <div className="content-wrapper">
        <ToursFilters filters={filters} onChange={setFilters} />
        {loading ? <Loader /> : <ToursList tours={tours} />}
      </div>
    </>
  );
}