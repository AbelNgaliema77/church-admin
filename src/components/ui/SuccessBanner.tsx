type SuccessBannerProps = {
  message: string | null;
};

export function SuccessBanner({ message }: SuccessBannerProps) {
  if (!message) {
    return null;
  }

  return <div className="success-banner">{message}</div>;
}