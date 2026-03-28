export default function TourOrganizer({ tour }) {
  return (
    <section className="tour-section">
      <h2>Организатор</h2>
      <div className="organizer-card">
        <h3>{tour.organizerName}</h3>
        <p>Рейтинг: {tour.organizerRating}</p>
        <p>Отзывы: {tour.organizerReviewsCount}</p>
        <p>Туров: {tour.organizerToursCount}</p>
        <p>С нами с: {tour.organizerJoined}</p>
        <p>{tour.organizerDescription}</p>
      </div>
    </section>
  );
}