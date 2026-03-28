export default function PromoBlocks() {
  const images = [
    '/images/UAE1.jpg',
    '/images/Maldives1.jpg',
    '/images/Russia1.jpg',
    '/images/Seychelles1.jpg'
  ];

  return (
    <div className="small-blocks">
      {images.map((src) => (
        <img key={src} src={src} alt="" />
      ))}
    </div>
  );
}