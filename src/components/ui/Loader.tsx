type LoaderProps = {
  message?: string;
};

export function Loader({ message = 'Loading...' }: LoaderProps) {
  return (
    <div className="loader-box">
      <div className="loader-spinner" />
      <div>{message}</div>
    </div>
  );
}