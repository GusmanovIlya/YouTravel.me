export default function EmptyState({ text = 'Ничего не найдено' }) {
  return <div className="state-box">{text}</div>;
}