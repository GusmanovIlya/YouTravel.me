import { Routes, Route } from 'react-router-dom';
import HomePage from './pages/HomePage';
import TourDetailsPage from './pages/TourDetailsPage';

export default function App() {
  return (
    <Routes>
      <Route path="/" element={<HomePage />} />
      <Route path="/tour/:id" element={<TourDetailsPage />} />
    </Routes>
  );
}