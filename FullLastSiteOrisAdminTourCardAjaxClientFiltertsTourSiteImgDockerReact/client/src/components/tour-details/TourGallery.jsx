export default function TourGallery({ tour }) {
  return (
    <section className="tour-gallery">
      <img className="main-image" src={tour.mainImage} alt={tour.title} />
      <div className="small-images">
        {tour.smallImages.map((img, index) => (
          <img key={index} src={img} alt="" />
        ))}
      </div>
    </section>
  );
}