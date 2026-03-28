import { useNavigate } from 'react-router-dom';

function levelDots(level) {
  return '●'.repeat(level) + '○'.repeat(5 - level);
}

export default function TourCard({ tour }) {
  const navigate = useNavigate();

  const finalPrice = tour.discount
    ? Math.round(tour.price * (1 - tour.discount / 100))
    : tour.price;

  return (
    <article className="card" onClick={() => navigate(`/tour/${tour.id}`)}>
      <img src={tour.imagePath} alt={tour.title} />

      <div className="card-content">
        <div className="maininfo">
          <div className="rating">
            {tour.rating.toFixed(1)} · {tour.reviewsCount} отзывов
          </div>

          <h3>{tour.title}</h3>
          <div className="country">{tour.country}</div>
          <p>{tour.description}</p>

          <div className="meta-row">
            <span>Активность: {levelDots(tour.activityLevel)}</span>
            <span>Комфорт: {levelDots(tour.comfortLevel)}</span>
          </div>

          <div className="meta-row">
            <span>{tour.days} дней</span>
            <span>{tour.nights} ночей</span>
          </div>
        </div>

        <div className="price-block">
          {tour.discount ? (
            <div className="old-price">₽ {Number(tour.price).toLocaleString('ru-RU')}</div>
          ) : null}
          <div className="final-price">₽ {Number(finalPrice).toLocaleString('ru-RU')}</div>
        </div>
      </div>
    </article>
  );
}