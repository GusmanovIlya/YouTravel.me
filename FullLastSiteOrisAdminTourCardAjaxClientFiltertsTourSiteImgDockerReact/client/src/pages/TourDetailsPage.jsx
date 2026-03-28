import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import Header from '../components/layout/Header';
import Breadcrumbs from '../components/layout/Breadcrumbs';
import Loader from '../components/common/Loader';
import { getTourById } from '../../../client/src/api/toursApi';
import TourGallery from '../components/tour-details/TourGallery';
import TourHero from '../components/tour-details/TourHero';
import TourOrganizer from '../components/tour-details/TourOrganizer';
import TourProgram from '../components/tour-details/TourProgram';

export default function TourDetailsPage() {
  const { id } = useParams();
  const [tour, setTour] = useState(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    loadTour();
  }, [id]);

  async function loadTour() {
    try {
      setLoading(true);
      const data = await getTourById(id);
      setTour(data);
    } catch (error) {
      console.error(error);
      setTour(null);
    } finally {
      setLoading(false);
    }
  }

  if (loading) {
    return (
      <>
        <Header />
        <Loader />
      </>
    );
  }

  if (!tour) {
    return (
      <>
        <Header />
        <div className="state-box">Тур не найден</div>
      </>
    );
  }

  return (
    <>
      <Header />
      <Breadcrumbs
        items={[
          { label: 'Главная', to: '/' },
          { label: 'Туры', to: '/' },
          { label: tour.title }
        ]}
      />

      <div className="details-page">
        <TourGallery tour={tour} />
        <TourHero tour={tour} />
        <TourOrganizer tour={tour} />
        <TourProgram days={tour.dayDescriptions || []} />
      </div>
    </>
  );
}