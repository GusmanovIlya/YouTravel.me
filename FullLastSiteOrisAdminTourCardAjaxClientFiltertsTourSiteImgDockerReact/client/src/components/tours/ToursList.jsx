import TourCard from './TourCard';
import EmptyState from '../common/EmptyState';

export default function ToursList({ tours }) {
  if (!tours.length) {
    return <EmptyState text="Туров не найдено" />;
  }

  return (
    <div className="cards-container">
      {tours.map((tour) => (
        <TourCard key={tour.id} tour={tour} />
      ))}
    </div>
  );
}