import { Link } from 'react-router-dom';

export default function Header() {
  return (
    <header className="nav">
      <div className="nav1">
        <strong>You Travel Me</strong>
      </div>

      <div className="nav2">
        <div className="nav2main">
          <Link to="/">Туры</Link>
        </div>
      </div>

      <div className="nav3">
        <div className="login">Войти</div>
      </div>
    </header>
  );
}