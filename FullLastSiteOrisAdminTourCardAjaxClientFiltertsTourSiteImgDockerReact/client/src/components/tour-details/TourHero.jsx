export default function TourHero({ tour }) {
  const finalPrice = tour.discount
    ? Math.round(tour.price * (1 - tour.discount / 100))
    : tour.price;

  const pricePerDay = tour.days > 0 ? Math.round(finalPrice / tour.days) : finalPrice;

  return (
    <section className="tour-hero">
      <div className="tour-main">
        <h1>{tour.title}</h1>
        <div className="tour-subinfo">
          <span>★ {Number(tour.rating).toFixed(1)}</span>
          <span>{tour.reviewsCount} отзывов</span>
          <span>{tour.days} дней</span>
        </div>
        <p>{tour.description}</p>
      </div>

      <div className="tour-price-card">
        {tour.discount ? (
          <div className="old-price">₽ {Number(tour.price).toLocaleString('ru-RU')}</div>
        ) : null}
        <div className="final-price">₽ {Number(finalPrice).toLocaleString('ru-RU')}</div>
        <div className="price-per-day">≈ ₽ {Number(pricePerDay).toLocaleString('ru-RU')} / день</div>
      </div>
    </section>
  );
}